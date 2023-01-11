using Bocchi.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Bocchi.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(BocchiEntityFrameworkCoreModule),
    typeof(BocchiApplicationContractsModule)
    )]
public class BocchiDbMigratorModule : AbpModule
{

}
