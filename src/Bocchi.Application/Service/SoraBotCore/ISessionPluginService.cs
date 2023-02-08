using System;

namespace Bocchi.SoraBotCore;

public interface ISessionPluginService
{
    public IPlugin GetPlugin(long groupId, long userId, Type pluginType);
    public void Finish(long groupId, long userId, Type pluginType);
}