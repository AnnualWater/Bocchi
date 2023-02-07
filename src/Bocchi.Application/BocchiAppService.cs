using Bocchi.Localization;
using Volo.Abp.Application.Services;

namespace Bocchi;

/* Inherit your application services from this class.
 */
public abstract class BocchiAppService : ApplicationService
{
    protected BocchiAppService()
    {
        LocalizationResource = typeof(BocchiResource);
    }
}
