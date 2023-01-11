using System;

namespace Bocchi.SoraBotCore;

public interface ISessionManager
{
    public PluginWithSession GetPlugin(Guid id, Type pluginType);
    public Guid GetPrivateSession(long userId, Type pluginType);
    public Guid GetGroupSession(long groupId, long userId, Type pluginType);
    public void EndPrivateSession(Guid id, long userId, Type pluginType);
    public void EndGroupSession(Guid id, long groupId, long userId, Type pluginType);
}