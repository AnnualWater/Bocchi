using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Bocchi.PluginSwitch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sora;
using Sora.Entities.Base;
using Sora.EventArgs.SoraEvent;
using Sora.Interfaces;
using Sora.Net.Config;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Threading;

namespace Bocchi.SoraBotCore;

public sealed class SoraBotService : ISoraBotService, ISingletonDependency
{
    /// <summary>
    /// Sora服务
    /// </summary>
    private readonly ISoraService _service;

    /// <summary>
    /// ID表
    /// </summary>
    private readonly Dictionary<long, Guid> _idList = new();

    /// <summary>
    /// 插件表
    /// </summary>
    private readonly IDictionary<Type, IDictionary<EventEnum, List<MethodInfo>>> _pluginMethod =
        new Dictionary<Type, IDictionary<EventEnum, List<MethodInfo>>>();

    /// <summary>
    /// 带Session的插件表
    /// </summary>
    private readonly IDictionary<Type, IDictionary<EventEnum, List<MethodInfo>>> _sessionPluginMethod =
        new Dictionary<Type, IDictionary<EventEnum, List<MethodInfo>>>();

    /// <summary>
    /// 依赖注入获取插件
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 参数表服务
    /// </summary>
    private readonly IPluginParamService _pluginParamService;

    /// <summary>
    /// Session管理器
    /// </summary>
    private readonly ISessionManager _sessionManager;

    /// <summary>
    /// 插件开关服务
    /// </summary>
    private readonly IPluginSwitchService _pluginSwitchService;

    private bool _isRun;
    private readonly ILogger<SoraBotService> _logger;

    public SoraBotService(IServiceProvider serviceProvider, IConfiguration configuration,
        ILogger<SoraBotService> logger, IPluginParamService pluginParamService, ISessionManager sessionManager,
        IPluginSwitchService pluginSwitchService)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _pluginParamService = pluginParamService;
        _sessionManager = sessionManager;
        _pluginSwitchService = pluginSwitchService;
        // 获取配置
        _logger.LogDebug("初始化SoraBotService");
        var defaultConfig = new ServerConfig();
        var config = new ServerConfig
        {
            Host = configuration.GetValue("SoraBot:Host", defaultConfig.Host),
            Port = configuration.GetValue("SoraBot:Port", defaultConfig.Port),
            AccessToken = configuration.GetValue("SoraBot:AccessToken", defaultConfig.AccessToken),
            UniversalPath = configuration.GetValue("SoraBot:UniversalPath", defaultConfig.UniversalPath),
            SuperUsers = configuration.GetSection("SoraBot:SuperUsers").Get<long[]>() ?? defaultConfig.SuperUsers,
            BlockUsers = configuration.GetSection("SoraBot:BlockUsers").Get<long[]>() ?? defaultConfig.BlockUsers,
            HeartBeatTimeOut = configuration.GetValue("SoraBot:HeartBeatTimeOut", defaultConfig.HeartBeatTimeOut),
            ApiTimeOut = configuration.GetValue("SoraBot:ApiTimeOut", defaultConfig.ApiTimeOut),
            EnableSoraCommandManager = configuration.GetValue("SoraBot:EnableSoraCommandManager",
                defaultConfig.EnableSoraCommandManager),
            EnableSocketMessage =
                configuration.GetValue("SoraBot:EnableSocketMessage", defaultConfig.EnableSocketMessage),
            AutoMarkMessageRead =
                configuration.GetValue("SoraBot:AutoMarkMessageRead", defaultConfig.AutoMarkMessageRead),
            ThrowCommandException =
                configuration.GetValue("SoraBot:ThrowCommandException", defaultConfig.ThrowCommandException),
            SendCommandErrMsg =
                configuration.GetValue("SoraBot:SendCommandErrMsg", defaultConfig.SendCommandErrMsg),
        };
        _service = SoraServiceFactory.CreateService(config);
        // 管理连接表
        _service.ConnManager.OnOpenConnectionAsync += (id, args) =>
        {
            _idList[args.SelfId] = id;
            return ValueTask.CompletedTask;
        };
        _service.ConnManager.OnCloseConnectionAsync += (_, args) =>
        {
            _idList.Remove(args.SelfId);
            return ValueTask.CompletedTask;
        };

        _logger.LogDebug("加载插件表");
        // 获取所有继承Plugin的类
        var pluginsTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes().Where(type =>
                !type.IsAbstract && !type.IsInterface &&
                type.GetBaseClasses().Contains(typeof(Plugin))));
        var sessionPluginTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes().Where(type =>
                !type.IsAbstract && !type.IsInterface &&
                type.GetBaseClasses().Contains(typeof(PluginWithSession))));
        // 获取所有能正常通过IOC获取的插件
        var plugins = new List<Plugin>();
        foreach (var pluginType in pluginsTypes)
        {
            var plugin = (Plugin)serviceProvider.GetService(pluginType);
            if (plugin == null)
            {
                _logger.LogWarning("插件{PluginTypeFullName}载入失败", pluginType.FullName);
                continue;
            }

            _logger.LogDebug("载入插件[{Plugin}]", plugin.GetType().FullName);

            plugins.Add(plugin);
        }

        var sessionPlugins = new List<PluginWithSession>();
        foreach (var pluginType in sessionPluginTypes)
        {
            var plugin = (PluginWithSession)serviceProvider.GetService(pluginType);
            if (plugin == null)
            {
                _logger.LogWarning("插件{PluginTypeFullName}载入失败", pluginType.FullName);
                continue;
            }

            _logger.LogDebug("载入插件[{Plugin}]", plugin.GetType().FullName);

            sessionPlugins.Add(plugin);
        }

        // 通过插件优先级排序
        foreach (var plugin in plugins.OrderBy(plugin => plugin.Priority))
        {
            _pluginMethod[plugin.GetType()] = new Dictionary<EventEnum, List<MethodInfo>>
            {
                { EventEnum.OnGroupMessage, plugin.GetAllPluginMethod<OnGroupMessageAttribute>().ToList() },
                { EventEnum.OnPrivateMessage, plugin.GetAllPluginMethod<OnPrivateMessageAttribute>().ToList() },
                { EventEnum.OnGroupMemberChange, plugin.GetAllPluginMethod<OnGroupMemberChangeAttribute>().ToList() },
                { EventEnum.OnGroupMemberMute, plugin.GetAllPluginMethod<OnGroupMemberMuteAttribute>().ToList() },
                { EventEnum.OnGroupRecall, plugin.GetAllPluginMethod<OnGroupRecallAttribute>().ToList() },
                { EventEnum.OnGroupPoke, plugin.GetAllPluginMethod<OnGroupPokeAttribute>().ToList() },
                { EventEnum.OnGroupRequest, plugin.GetAllPluginMethod<OnGroupRequestAttribute>().ToList() },
                { EventEnum.OnGroupAdminChange, plugin.GetAllPluginMethod<OnGroupAdminChangeAttribute>().ToList() },
                { EventEnum.OnGroupCardUpdate, plugin.GetAllPluginMethod<OnGroupCardUpdateAttribute>().ToList() },
                { EventEnum.OnFriendRecall, plugin.GetAllPluginMethod<OnFriendRecallAttribute>().ToList() },
                { EventEnum.OnFriendRequest, plugin.GetAllPluginMethod<OnFriendRequestAttribute>().ToList() },
                { EventEnum.OnFriendAdd, plugin.GetAllPluginMethod<OnFriendAddAttribute>().ToList() }
            };
        }

        foreach (var pluginWithSession in sessionPlugins.OrderBy(plugin => plugin.Priority))
        {
            _sessionPluginMethod[pluginWithSession.GetType()] = new Dictionary<EventEnum, List<MethodInfo>>
            {
                { EventEnum.OnGroupMessage, pluginWithSession.GetAllPluginMethod<OnGroupMessageAttribute>().ToList() },
                {
                    EventEnum.OnPrivateMessage,
                    pluginWithSession.GetAllPluginMethod<OnPrivateMessageAttribute>().ToList()
                }
            };
        }

        // 连接方法

        #region 群插件

        _service.Event.OnGroupMessage += async (type, args) =>
        {
            await InvokeSessionPluginMethod(EventEnum.OnGroupMessage, type, args);
            await InvokePluginMethod(EventEnum.OnGroupMessage, type, args);
        };
        _service.Event.OnGroupMemberChange += async (type, args) =>
        {
            await InvokePluginMethod(EventEnum.OnGroupMemberChange, type, args);
        };
        _service.Event.OnGroupMemberMute += async (type, args) =>
        {
            await InvokePluginMethod(EventEnum.OnGroupMemberMute, type, args);
        };
        _service.Event.OnGroupRecall += async (type, args) =>
        {
            await InvokePluginMethod(EventEnum.OnGroupRecall, type, args);
        };
        _service.Event.OnGroupPoke += async (type, args) =>
        {
            await InvokePluginMethod(EventEnum.OnGroupPoke, type, args);
        };
        _service.Event.OnGroupRequest += async (type, args) =>
        {
            await InvokePluginMethod(EventEnum.OnGroupRequest, type, args);
        };
        _service.Event.OnGroupAdminChange += async (type, args) =>
        {
            await InvokePluginMethod(EventEnum.OnGroupAdminChange, type, args);
        };
        _service.Event.OnGroupCardUpdate += async (type, args) =>
        {
            await InvokePluginMethod(EventEnum.OnGroupCardUpdate, type, args);
        };

        #endregion

        #region 私聊插件

        _service.Event.OnPrivateMessage += async (type, args) =>
        {
            await InvokeSessionPluginMethod(EventEnum.OnPrivateMessage, type, args);
            await InvokePluginMethod(EventEnum.OnPrivateMessage, type, args);
        };
        _service.Event.OnFriendRecall += async (type, args) =>
        {
            await InvokePluginMethod(EventEnum.OnFriendRecall, type, args);
        };

        #endregion

        #region 好友相关

        _service.Event.OnFriendRequest += async (type, args) =>
        {
            await InvokePluginMethod(EventEnum.OnFriendRequest, type, args);
        };
        _service.Event.OnFriendAdd += async (type, args) =>
        {
            await InvokePluginMethod(EventEnum.OnFriendAdd, type, args);
        };

        #endregion
    }

    private async Task InvokeSessionPluginMethod(EventEnum eventType, string type, BaseMessageEventArgs args)
    {
        void EndPlugin(Guid sessionId, Type pluginType, BaseMessageEventArgs msg)
        {
            switch (msg)
            {
                case PrivateMessageEventArgs privateMessageEventArgs:
                    _sessionManager.EndPrivateSession(sessionId, privateMessageEventArgs.SenderInfo.UserId, pluginType);
                    break;
                case GroupMessageEventArgs groupMessageEventArgs:
                    _sessionManager.EndGroupSession(sessionId, groupMessageEventArgs.SourceGroup.Id,
                        groupMessageEventArgs.SenderInfo.UserId, pluginType);
                    break;
            }
        }

        foreach (var (pluginType, methods) in _sessionPluginMethod)
        {
            var allow = args switch
            {
                PrivateMessageEventArgs privateMessageEventArgs => await _pluginSwitchService.CheckPlugin(
                    pluginType.FullName, privateMessageEventArgs.SenderInfo.UserId, PluginSwitchType.Private),
                GroupMessageEventArgs groupMessageEventArgs => await _pluginSwitchService.CheckPlugin(
                    pluginType.FullName, groupMessageEventArgs.SourceGroup.Id, PluginSwitchType.Group),
                _ => false
            };
            if (!allow)
            {
                continue;
            }

            var sessionId = args switch
            {
                PrivateMessageEventArgs privateMessageEventArgs => _sessionManager.GetPrivateSession(
                    privateMessageEventArgs.SenderInfo.UserId, pluginType),
                GroupMessageEventArgs groupMessageEventArgs => _sessionManager.GetGroupSession(
                    groupMessageEventArgs.SourceGroup.Id, groupMessageEventArgs.SenderInfo.UserId, pluginType),
                _ => Guid.Empty
            };

            _pluginParamService.InitParamList(sessionId, new Dictionary<string, object>
            {
                { "type", type },
                { "args", args }
            });

            var plugin = _sessionManager.GetPlugin(sessionId, pluginType);
            if (plugin == null)
            {
                _logger.LogWarning("插件{PluginTypeFullName}载入失败", pluginType.FullName);
                continue;
            }

            var i = plugin.GetFinished();
            foreach (var methodInfo in methods[eventType])
            {
                if (i != 0)
                {
                    i--;
                    continue;
                }

                var pList = new List<object>();
                foreach (var parameter in methodInfo.GetParameters())
                {
                    // 名称匹配
                    if (_pluginParamService.ContainsKey(sessionId, parameter.Name))
                    {
                        pList.Add(_pluginParamService.GetValue(sessionId, parameter.Name));
                    }
                    else
                    {
                        // 类型匹配
                        var p = _pluginParamService.ContainsValueType(sessionId, parameter.ParameterType);
                        if (p == null || !p.Any())
                        {
                            _logger.LogWarning("方法[{Plugin}]-[{Method}]的参数[{Param}]未找到合适的对象！",
                                plugin.GetType().FullName, methodInfo.Name, parameter.Name);
                            pList.Add(null);
                        }

                        pList.Add(p.First().Value);
                    }
                }

                try
                {
                    if (methodInfo.IsAsync())
                    {
                        var task = (Task)methodInfo.Invoke(plugin, pList.ToArray());
                        if (task != null)
                        {
                            await task;
                        }
                    }
                    else
                    {
                        methodInfo.Invoke(plugin, pList.ToArray());
                    }
                }
                catch (PluginFinishException)
                {
                    _logger.LogDebug("插件[{Plugin}]提前完成", pluginType.FullName);
                    EndPlugin(sessionId, pluginType, args);
                    break;
                }
                catch (MethodWaitNewEventException)
                {
                    _logger.LogDebug("插件[{Plugin}]的方法[{Method}]等待新事件", pluginType.FullName, methodInfo.Name);
                    break;
                }

                plugin.FinishMethod();
            }

            if (plugin.GetFinished() >= methods[eventType].Count)
            {
                EndPlugin(sessionId, pluginType, args);
            }
        }
    }

    private async Task InvokePluginMethod(EventEnum eventType, string type, BaseSoraEventArgs args)
    {
        foreach (var (pluginType, methods) in _pluginMethod)
        {
            var allow = args switch
            {
                PrivateMessageEventArgs privateMessageEventArgs => await _pluginSwitchService.CheckPlugin(
                    pluginType.FullName, privateMessageEventArgs.SenderInfo.UserId, PluginSwitchType.Private),
                GroupMessageEventArgs groupMessageEventArgs => await _pluginSwitchService.CheckPlugin(
                    pluginType.FullName, groupMessageEventArgs.SourceGroup.Id, PluginSwitchType.Group),
                _ => true
            };
            if (!allow)
            {
                continue;
            }

            foreach (var methodInfo in methods[eventType])
            {
                var plugin = (Plugin)_serviceProvider.GetService(pluginType);
                if (plugin == null)
                {
                    _logger.LogWarning("插件{PluginTypeFullName}载入失败", pluginType.FullName);
                    continue;
                }

                var pList = new List<object>();
                foreach (var parameterInfo in methodInfo.GetParameters())
                {
                    if (parameterInfo.ParameterType == args.GetType())
                    {
                        pList.Add(args);
                        continue;
                    }

                    if (parameterInfo.Name == "type" && parameterInfo.ParameterType == typeof(string))
                    {
                        pList.Add(type);
                        continue;
                    }

                    pList.Add(null);
                }

                if (methodInfo.IsAsync())
                {
                    var task = (Task)methodInfo.Invoke(plugin, pList.ToArray());
                    if (task != null)
                    {
                        await task;
                    }
                }
                else
                {
                    methodInfo.Invoke(plugin, pList.ToArray());
                }
            }
        }
    }

    public async Task RunService()
    {
        // 只能启动一次
        if (_isRun)
        {
            _logger.LogWarning("只能启动一次！");
            return;
        }

        await _service.StartService();
        _isRun = true;
    }

    public IEnumerable<long> GetAllTencentId()
        => _idList.Select(x => x.Key);

    public SoraApi GetApi(long tencentId)
        => _service.GetApi(_idList[tencentId]);

    public IEnumerable<SoraApi> GetAllApi()
    {
        return _idList.Select(x => _service.GetApi(x.Value)).Where(x => x != null).ToList();
    }
}