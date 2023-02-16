using System.Threading.Tasks;
using Bocchi.SoraBotCore;
using Bocchi.SoraBotCore.NoPasswordToken;
using Bocchi.WebCore;
using Sora.EventArgs.SoraEvent;
using Sora.OnebotAdapter;

namespace Bocchi.SoraBotPlugin.Login;

public class LoginPlugin : IOnPrivateMessagePlugin
{
    public int Priority => 100;

    public EventAdapter.EventAsyncCallBackHandler<PrivateMessageEventArgs> OnPrivateMessage =>
        async (_, args) => { await Check(args); };

    private readonly INoPasswordTokenService _tokenService;
    private readonly IWebCoreService _webCoreService;

    public LoginPlugin(INoPasswordTokenService tokenService, IWebCoreService webCoreService)
    {
        _tokenService = tokenService;
        _webCoreService = webCoreService;
    }

    private async Task Check(PrivateMessageEventArgs args)
    {
        var rawText = args.Message.RawText;
        if (rawText is "登录" or "login")
        {
            var token = await _tokenService.GetLoginToken(args.SenderInfo.UserId);
            await args.Reply($"{await _webCoreService.GetWebUrl()}/api/bocchi/account/no_password?token={token}");
        }
    }
}