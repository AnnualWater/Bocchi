using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;

namespace Bocchi.SoraBotCore;

public class SessionManager : ISessionManager, ISingletonDependency
{
    private readonly TimeSpan _overtime = new TimeSpan(0, 10, 0);
    private readonly Dictionary<(long userId, Type plugin), (Guid sessionId, DateTime time)> _privateSessions = new();

    private readonly Dictionary<(long groupId, long userId, Type plugin), (Guid sessionId, DateTime time)>
        _groupSessions = new();

    private readonly IGuidGenerator _guidGenerator;
    private readonly List<(Guid sessionId, PluginWithSession plugin)> _sessionPlugin = new();
    private readonly IPluginParamService _pluginParamService;
    private readonly ILogger<SessionManager> _logger;

    private readonly IServiceProvider _serviceProvider;

    public SessionManager(IGuidGenerator guidGenerator, IServiceProvider serviceProvider,
        ILogger<SessionManager> logger, IPluginParamService pluginParamService)
    {
        _guidGenerator = guidGenerator;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _pluginParamService = pluginParamService;
    }

    public PluginWithSession GetPlugin(Guid id, Type pluginType)
    {
        var ls = _sessionPlugin
            .Where(item => item.sessionId == id && item.plugin.GetType() == pluginType).ToList();
        if (ls.Count == 0)
        {
            var plugin = (PluginWithSession)_serviceProvider.GetService(pluginType);
            if (plugin == null)
            {
                return null;
            }

            plugin.SetSessionId(id);
            _sessionPlugin.Add((id, plugin));
            return plugin;
        }
        else
        {
            return ls.First().plugin;
        }
    }


    public Guid GetPrivateSession(long userId, Type pluginType)
    {
        var key = (userId, pluginType);
        if (_privateSessions.ContainsKey(key))
        {
            if (DateTime.Now - _privateSessions[key].time <= _overtime)
            {
                // 刷新超时时间
                _privateSessions[key] = (_privateSessions[key].sessionId, DateTime.Now);
                return _privateSessions[key].sessionId;
            }

            // 超时直接结束这个session
            _logger.LogDebug("PrivateSession[{Session}]超时->插件[{Plugin}]", _privateSessions[key].sessionId,
                pluginType.FullName);
            EndPrivateSession(_privateSessions[key].sessionId, userId, pluginType);
        }

        var sessionId = _guidGenerator.Create();
        _privateSessions.Add(key, (sessionId, DateTime.Now));
        _logger.LogDebug("新的PrivateSession[{Session}]->插件[{Plugin}]", sessionId, pluginType.FullName);
        return sessionId;
    }

    public Guid GetGroupSession(long groupId, long userId, Type pluginType)
    {
        var key = (groupId, userId, pluginType);
        if (_groupSessions.ContainsKey(key))
        {
            if (DateTime.Now - _groupSessions[key].time <= _overtime)
            {
                // 刷新超时时间
                _groupSessions[key] = (_groupSessions[key].sessionId, DateTime.Now);
                return _groupSessions[key].sessionId;
            }

            // 超时直接结束这个session
            _logger.LogDebug("GroupSession[{Session}]超时->插件[{Plugin}]", _groupSessions[key].sessionId,
                pluginType.FullName);
            EndGroupSession(_groupSessions[key].sessionId, groupId, userId, pluginType);
        }

        var sessionId = _guidGenerator.Create();
        _groupSessions.Add(key, (sessionId, DateTime.Now));
        _logger.LogDebug("新的GroupSession[{Session}]->插件[{Plugin}]", sessionId, pluginType.FullName);
        return sessionId;
    }

    public void EndPrivateSession(Guid id, long userId, Type pluginType)
    {
        _privateSessions.Remove((userId, pluginType));
        EndPlugin(id, pluginType);
    }

    public void EndGroupSession(Guid id, long groupId, long userId, Type pluginType)
    {
        _groupSessions.Remove((groupId, userId, pluginType));
        EndPlugin(id, pluginType);
    }

    private void EndPlugin(Guid id, Type pluginType)
    {
        _logger.LogDebug("Session[{Session}]->[{Plugin}]结束", id, pluginType.FullName);
        var ls = _sessionPlugin
            .Where(item => item.sessionId == id && item.plugin.GetType() == pluginType).ToList();
        if (ls.Count == 0)
        {
            var plugin = (PluginWithSession)_serviceProvider.GetService(pluginType);
            if (plugin == null)
            {
                return;
            }


            _sessionPlugin.Remove((id, plugin));
            plugin = null;

            _pluginParamService.CleanList(id);
            return;
        }
    }
}