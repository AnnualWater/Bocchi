using System.Threading.Tasks;
using Bocchi.SoraBotCore;
using Bocchi.SoraBotCore.NoPasswordToken;
using Bocchi.WebCore;
using Sora.EventArgs.SoraEvent;

namespace Bocchi.SoraBotPlugin.Login;

public class LoginPlugin : Plugin
{
    public override uint Priority => 1;
    private readonly INoPasswordTokenService _tokenService;
    private readonly IWebCoreService _webCoreService;

    public LoginPlugin(INoPasswordTokenService tokenService, IWebCoreService webCoreService)
    {
        _tokenService = tokenService;
        _webCoreService = webCoreService;
    }


    [OnPrivateMessage(1)]
    public async Task Check(PrivateMessageEventArgs args)
    {
        var rawText = args.Message.RawText;
        if (rawText is "登录" or "login")
        {
            var token = await _tokenService.GetLoginToken(args.SenderInfo.UserId);
            await args.Reply($"{await _webCoreService.GetWebUrl()}api/bocchi/account/no_password?token={token}");
        }
    }
}