using System.Threading.Tasks;
using Bocchi.Controllers;
using Bocchi.SoraBotCore.NoPasswordToken;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Identity;
using Volo.Abp.Identity.AspNetCore;

namespace Bocchi.Blazor.Controller;

[Route("api/bocchi/account")]
public class BocchiLoginController : BocchiController
{
    private readonly AbpSignInManager _signInManager;
    private readonly IdentityUserManager _identityUserManager;
    private readonly INoPasswordTokenService _tokenService;
    private readonly IdentitySecurityLogManager _identitySecurityLogManager;

    public BocchiLoginController(AbpSignInManager signInManager, IdentityUserManager identityUserManager,
        INoPasswordTokenService tokenService, IdentitySecurityLogManager identitySecurityLogManager)
    {
        _signInManager = signInManager;
        _identityUserManager = identityUserManager;
        _tokenService = tokenService;
        _identitySecurityLogManager = identitySecurityLogManager;
    }

    [HttpGet]
    [Route("no_password")]
    public async Task<ActionResult> NoPasswordLogin([FromQuery] string token, [FromQuery] string redirect)
    {
        var userId = await _tokenService.GetUserId(token);

        var user = await _identityUserManager.FindByIdAsync(userId);
        if (user == null)
        {
            await _identitySecurityLogManager.SaveAsync(new IdentitySecurityLogContext()
            {
                Identity = IdentitySecurityLogIdentityConsts.Identity,
                Action = IdentitySecurityLogActionConsts.LoginFailed
            });
            return Redirect("/account/login");
        }

        await _signInManager.SignInAsync(user, true);
        if (string.IsNullOrEmpty(redirect))
        {
            redirect = "/";
        }

        await _identitySecurityLogManager.SaveAsync(new IdentitySecurityLogContext()
        {
            Identity = IdentitySecurityLogIdentityConsts.Identity,
            Action = IdentitySecurityLogActionConsts.LoginSucceeded
        });

        return Redirect(redirect);
    }


}