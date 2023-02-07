using Sora.EventArgs.SoraEvent;
using Volo.Abp.DependencyInjection;
using Sora.OnebotAdapter;

namespace Bocchi.SoraBotCore;

public interface IPlugin : ITransientDependency
{
    public int Priority { get; }
}

public interface IOnGroupMessagePlugin : IPlugin
{
    public EventAdapter.EventAsyncCallBackHandler<GroupMessageEventArgs> OnGroupMessage { get; }
}

public interface IOnGroupMemberChangePlugin : IPlugin
{
    public EventAdapter.EventAsyncCallBackHandler<GroupMemberChangeEventArgs> OnGroupMemberChange { get; }
}

public interface IOnGroupMemberMutePlugin : IPlugin
{
    public EventAdapter.EventAsyncCallBackHandler<GroupMuteEventArgs> OnGroupMemberMute { get; }
}

public interface IOnGroupRecallPlugin : IPlugin
{
    public EventAdapter.EventAsyncCallBackHandler<GroupRecallEventArgs> OnGroupRecall { get; }
}

public interface IOnGroupPokePlugin : IPlugin
{
    public EventAdapter.EventAsyncCallBackHandler<GroupPokeEventArgs> OnGroupPoke { get; }
}

public interface IOnGroupRequestPlugin : IPlugin
{
    public EventAdapter.EventAsyncCallBackHandler<AddGroupRequestEventArgs> OnGroupRequest { get; }
}

public interface IOnGroupAdminChangePlugin : IPlugin
{
    public EventAdapter.EventAsyncCallBackHandler<GroupAdminChangeEventArgs> OnGroupAdminChange { get; }
}

public interface IOnGroupCardUpdatePlugin : IPlugin
{
    public EventAdapter.EventAsyncCallBackHandler<GroupCardUpdateEventArgs> OnGroupCardUpdate { get; }
}

public interface IOnPrivateMessagePlugin : IPlugin
{
    public EventAdapter.EventAsyncCallBackHandler<PrivateMessageEventArgs> OnPrivateMessage { get; }
}

public interface IOnFriendRecallPlugin : IPlugin
{
    public EventAdapter.EventAsyncCallBackHandler<FriendRecallEventArgs> OnFriendRecall { get; }
}

public interface IOnFriendRequestPlugin : IPlugin
{
    public EventAdapter.EventAsyncCallBackHandler<FriendRequestEventArgs> OnFriendRequest { get; }
}

public interface IOnFriendAddPlugin : IPlugin
{
    public EventAdapter.EventAsyncCallBackHandler<FriendAddEventArgs> OnFriendAdd { get; }
}