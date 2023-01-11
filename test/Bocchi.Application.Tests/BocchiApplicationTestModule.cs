using Volo.Abp.Modularity;

namespace Bocchi;

[DependsOn(
    typeof(BocchiApplicationModule),
    typeof(BocchiDomainTestModule)
    )]
public class BocchiApplicationTestModule : AbpModule
{

}
