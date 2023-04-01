using System.Threading.Tasks;
using Bocchi.SoraBotCore;
using Sora.EventArgs.SoraEvent;
using Sora.OnebotAdapter;

namespace Bocchi.AutoAllowFriend;

public class AutoAllowFriendPlugin : IOnFriendRequestPlugin
{
    public int Priority => 0;

    public EventAdapter.EventAsyncCallBackHandler<FriendRequestEventArgs> OnFriendRequest =>
        async (_, args) => { await Check(args); };

    private async ValueTask Check(FriendRequestEventArgs eventArgs)
    {
        // 同意所有加好友请求
        await eventArgs.Accept();
    }
}