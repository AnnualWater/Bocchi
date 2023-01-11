using System.Threading.Tasks;
using Bocchi.SoraBotCore;
using Sora.EventArgs.SoraEvent;

namespace Bocchi.SoraBotPlugin.Friend;

public class FriendPlugin : Plugin
{
    public override uint Priority => 100;

    [OnFriendRequest(1)]
    public async Task Check(FriendRequestEventArgs args)
    {
        // 同意所有加好友请求
        await args.Accept();
    }
}