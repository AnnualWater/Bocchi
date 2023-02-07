using System.Threading.Tasks;
using Bocchi.SoraBotCore;
using Sora.EventArgs.SoraEvent;
using Sora.OnebotAdapter;

namespace Bocchi.SoraBotPlugin.Friend;

public class FriendPlugin : IOnFriendRequestPlugin
{
    public int Priority => 0;

    public EventAdapter.EventAsyncCallBackHandler<FriendRequestEventArgs> OnFriendRequest =>
        async (type, args) => { await Check(args); };

    private async ValueTask Check(FriendRequestEventArgs eventArgs)
    {
        // 同意所有加好友请求
        await eventArgs.Accept();
    }
}