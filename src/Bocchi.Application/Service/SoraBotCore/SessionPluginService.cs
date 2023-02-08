using System;
using System.Collections.Generic;
using Volo.Abp.DependencyInjection;

namespace Bocchi.SoraBotCore;

public class SessionPluginService : ISessionPluginService, ISingletonDependency
{
    private readonly Dictionary<(long groupId, long userId, string pluginFullName),
        (IPlugin plugin, DateTime createTime)> _plugins = new();

    private const int OverTimeSecond = 600;

    private readonly IServiceProvider _serviceProvider;

    public SessionPluginService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IPlugin GetPlugin(long groupId, long userId, Type pluginType)
    {
        if (groupId == 0 && userId == 0)
        {
            return (IPlugin)_serviceProvider.GetService(pluginType);
        }

        if (_plugins.TryGetValue((groupId, userId, pluginType.FullName), out var value))
        {
            var (plugin, createTime) = value;
            if ((DateTime.Now - createTime).TotalSeconds <= OverTimeSecond)
            {
                return plugin;
            }
        }

        var newPlugin = (IPlugin)_serviceProvider.GetService(pluginType);
        _plugins[(groupId, userId, pluginType.FullName)] = (newPlugin, DateTime.Now);
        return newPlugin;
    }

    public void Finish(long groupId, long userId, Type pluginType)
    {
        _plugins.Remove((groupId, userId, pluginType.FullName));
    }
}