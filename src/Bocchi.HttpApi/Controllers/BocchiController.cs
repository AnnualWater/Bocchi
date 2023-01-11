using Bocchi.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Bocchi.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class BocchiController : AbpControllerBase
{
    protected BocchiController()
    {
        LocalizationResource = typeof(BocchiResource);
    }
}
