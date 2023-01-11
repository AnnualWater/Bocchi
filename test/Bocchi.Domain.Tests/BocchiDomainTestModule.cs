using Bocchi.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Bocchi;

[DependsOn(
    typeof(BocchiEntityFrameworkCoreTestModule)
    )]
public class BocchiDomainTestModule : AbpModule
{

}
