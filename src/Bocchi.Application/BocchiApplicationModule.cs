using System.Threading.Tasks;
using Bocchi.SoraBotPlugin.ComicSubscription;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.BackgroundJobs.Hangfire;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.BackgroundWorkers.Hangfire;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace Bocchi;

[DependsOn(
    typeof(BocchiDomainModule),
    typeof(AbpAccountApplicationModule),
    typeof(BocchiApplicationContractsModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(AbpBackgroundWorkersHangfireModule)
)]
public class BocchiApplicationModule : AbpModule
{
    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        await base.OnApplicationInitializationAsync(context);
        await context.AddBackgroundWorkerAsync<ComicSubscriptionWorker>();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<BocchiApplicationModule>(); });
    }
}