using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Bocchi.SoraBotCore;

public sealed class
    SoraBotService : ISoraBotService, ISingletonDependency
{
    /// <summary>
    /// Sora服务
    /// </summary>
    private readonly ISoraService _service;

    ~SoraBotService()
    {
        if (_service != null)
        {
            // Linux 释放端口
            _service.StopService().AsTask().Wait();
        }
    }

    /// <summary>
    /// ID表
    /// </summary>
    private readonly Dictionary<long, Guid> _idList = new();

    private readonly Dictionary<EventEnum, List<Type>> _plugins = new()
    {
        { EventEnum.OnGroupMessage, new List<Type>() },
        { EventEnum.OnPrivateMessage, new List<Type>() },
        { EventEnum.OnGroupMemberChange, new List<Type>() },
        { EventEnum.OnGroupMemberMute, new List<Type>() },
        { EventEnum.OnGroupRecall, new List<Type>() },
        { EventEnum.OnGroupPoke, new List<Type>() },
        { EventEnum.OnGroupRequest, new List<Type>() },
        { EventEnum.OnGroupAdminChange, new List<Type>() },
        { EventEnum.OnGroupCardUpdate, new List<Type>() },
        { EventEnum.OnFriendRecall, new List<Type>() },
        { EventEnum.OnFriendRequest, new List<Type>() },
        { EventEnum.OnFriendAdd, new List<Type>() }
    };

    private readonly ISessionPluginService _sessionPluginService;

    /// <summary>
    /// 插件开关服务
    /// </summary>
    private readonly IPluginSwitchService _pluginSwitchService;

    private bool _isRun;
    private readonly ILogger<SoraBotService> _logger;

    public SoraBotService(IServiceProvider serviceProvider, IConfiguration configuration,
        ILogger<SoraBotService> logger, IPluginSwitchService pluginSwitchService,
        ISessionPluginService sessionPluginService)
    {
        _logger = logger;
        _pluginSwitchService = pluginSwitchService;
        _sessionPluginService = sessionPluginService;
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
                type.GetInterfaces().Contains(typeof(IPlugin))));
        // 获取所有能正常通过IOC获取的插件
        var plugins = new List<IPlugin>();
        foreach (var pluginType in pluginsTypes)
        {
            var plugin = (IPlugin)serviceProvider.GetService(pluginType);
            if (plugin == null)
            {
                _logger.LogWarning("插件{PluginTypeFullName}载入失败", pluginType.FullName);
                continue;
            }

            _logger.LogDebug("载入插件[{Plugin}]", plugin.GetType().FullName);

            plugins.Add(plugin);
        }

        // 通过插件优先级排序
        foreach (var plugin in plugins.OrderBy(plugin => plugin.Priority))
        {
            if (plugin is IOnGroupMessagePlugin)
            {
                _plugins[EventEnum.OnGroupMessage].Add(plugin.GetType());
            }

            if (plugin is IOnPrivateMessagePlugin)
            {
                _plugins[EventEnum.OnPrivateMessage].Add(plugin.GetType());
            }

            if (plugin is IOnGroupMemberChangePlugin)
            {
                _plugins[EventEnum.OnGroupMemberChange].Add(plugin.GetType());
            }

            if (plugin is IOnGroupMemberMutePlugin)
            {
                _plugins[EventEnum.OnGroupMemberMute].Add(plugin.GetType());
            }

            if (plugin is IOnGroupRecallPlugin)
            {
                _plugins[EventEnum.OnGroupRecall].Add(plugin.GetType());
            }

            if (plugin is IOnGroupPokePlugin)
            {
                _plugins[EventEnum.OnGroupPoke].Add(plugin.GetType());
            }

            if (plugin is IOnGroupRequestPlugin)
            {
                _plugins[EventEnum.OnGroupRequest].Add(plugin.GetType());
            }

            if (plugin is IOnGroupAdminChangePlugin)
            {
                _plugins[EventEnum.OnGroupAdminChange].Add(plugin.GetType());
            }

            if (plugin is IOnGroupCardUpdatePlugin)
            {
                _plugins[EventEnum.OnGroupCardUpdate].Add(plugin.GetType());
            }

            if (plugin is IOnFriendRecallPlugin)
            {
                _plugins[EventEnum.OnFriendRecall].Add(plugin.GetType());
            }

            if (plugin is IOnFriendRequestPlugin)
            {
                _plugins[EventEnum.OnFriendRequest].Add(plugin.GetType());
            }

            if (plugin is IOnFriendAddPlugin)
            {
                _plugins[EventEnum.OnFriendAdd].Add(plugin.GetType());
            }
        }


        // 连接方法

        #region 群插件

        _service.Event.OnGroupMessage += async (type, args) =>
        {
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

    private async Task InvokePluginMethod(EventEnum eventType, string type, BaseSoraEventArgs args)
    {
        foreach (var pluginType in _plugins[eventType])
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

            long userId = 0, groupId = 0;
            if (args is BaseMessageEventArgs messageEventArgs)
            {
                userId = messageEventArgs.Sender.Id;
            }

            if (args is GroupMessageEventArgs gMessageEventArgs)
            {
                groupId = gMessageEventArgs.SourceGroup.Id;
            }

            // 获取plugin
            var plugin = _sessionPluginService.GetPlugin(groupId, userId, pluginType);

            try
            {
                switch (eventType)
                {
                    case EventEnum.OnGroupMessage:
                        await ((IOnGroupMessagePlugin)plugin).OnGroupMessage.Invoke(type, (GroupMessageEventArgs)args);
                        break;
                    case EventEnum.OnGroupMemberChange:
                        await ((IOnGroupMemberChangePlugin)plugin).OnGroupMemberChange.Invoke(type,
                            (GroupMemberChangeEventArgs)args);
                        break;
                    case EventEnum.OnGroupMemberMute:
                        await ((IOnGroupMemberMutePlugin)plugin).OnGroupMemberMute.Invoke(type,
                            (GroupMuteEventArgs)args);
                        break;
                    case EventEnum.OnGroupRecall:
                        await ((IOnGroupRecallPlugin)plugin).OnGroupRecall.Invoke(type, (GroupRecallEventArgs)args);
                        break;
                    case EventEnum.OnGroupPoke:
                        await ((IOnGroupPokePlugin)plugin).OnGroupPoke.Invoke(type, (GroupPokeEventArgs)args);
                        break;
                    case EventEnum.OnGroupRequest:
                        await ((IOnGroupRequestPlugin)plugin).OnGroupRequest.Invoke(type,
                            (AddGroupRequestEventArgs)args);
                        break;
                    case EventEnum.OnGroupAdminChange:
                        await ((IOnGroupAdminChangePlugin)plugin).OnGroupAdminChange.Invoke(type,
                            (GroupAdminChangeEventArgs)args);
                        break;
                    case EventEnum.OnGroupCardUpdate:
                        await ((IOnGroupCardUpdatePlugin)plugin).OnGroupCardUpdate.Invoke(type,
                            (GroupCardUpdateEventArgs)args);
                        break;
                    case EventEnum.OnPrivateMessage:
                        await ((IOnPrivateMessagePlugin)plugin).OnPrivateMessage.Invoke(type,
                            (PrivateMessageEventArgs)args);
                        break;
                    case EventEnum.OnFriendRecall:
                        await ((IOnFriendRecallPlugin)plugin).OnFriendRecall.Invoke(type, (FriendRecallEventArgs)args);
                        break;
                    case EventEnum.OnFriendRequest:
                        await ((IOnFriendRequestPlugin)plugin).OnFriendRequest.Invoke(type,
                            (FriendRequestEventArgs)args);
                        break;
                    case EventEnum.OnFriendAdd:
                        await ((IOnFriendAddPlugin)plugin).OnFriendAdd.Invoke(type, (FriendAddEventArgs)args);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null);
                }
            }
            catch (WaitPluginException)
            {
                continue;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "插件[{Plugin}]执行出错！", plugin.GetType().FullName);
                _sessionPluginService.Finish(groupId, userId, pluginType);
            }

            _sessionPluginService.Finish(groupId, userId, pluginType);
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