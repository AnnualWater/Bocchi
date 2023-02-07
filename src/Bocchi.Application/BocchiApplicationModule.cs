using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
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
        // 自动添加定时任务
        var workers = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName != null && a.FullName.Contains(nameof(Bocchi))).SelectMany(a =>
                a.GetTypes().Where(type =>
                    !type.IsAbstract && !type.IsInterface &&
                    type.GetBaseClasses().Contains(typeof(HangfireBackgroundWorkerBase))));

        foreach (var worker in workers)
        {
            await context.AddBackgroundWorkerAsync(worker);
        }
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<BocchiApplicationModule>(); });
    }
}