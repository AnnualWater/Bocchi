using Bocchi.Localization;
using Volo.Abp.AspNetCore.Components;

namespace Bocchi.Blazor;

public abstract class BocchiComponentBase : AbpComponentBase
{
    protected BocchiComponentBase()
    {
        LocalizationResource = typeof(BocchiResource);
    }
}
