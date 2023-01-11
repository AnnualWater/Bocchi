using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Bocchi.Blazor;

[Dependency(ReplaceServices = true)]
public class BocchiBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "Bocchi";
}
