using System.Collections.Generic;
using System.IO;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Bocchi.AuthorizeFilter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Bocchi.Blazor.Menus;
using Bocchi.EntityFrameworkCore;
using Bocchi.Localization;
using Bocchi.MultiTenancy;
using Hangfire;
using Microsoft.AspNetCore.Localization;
using OpenIddict.Validation.AspNetCore;
using Volo.Abp;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Components.Server.BasicTheme;
using Volo.Abp.AspNetCore.Components.Server.BasicTheme.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic.Bundling;
using Volo.Abp.AspNetCore.Components.Web.Theming.Routing;
using Volo.Abp.AspNetCore.Components.Web.Theming.Toolbars;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.Hangfire;
using Volo.Abp.Identity.Blazor.Server;
using Volo.Abp.Modularity;
using Volo.Abp.SettingManagement.Blazor.Server;
using Volo.Abp.Swashbuckle;
using Volo.Abp.TenantManagement.Blazor.Server;
using Volo.Abp.UI.Navigation;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;

namespace Bocchi.Blazor;

[DependsOn(
    typeof(BocchiApplicationModule),
    typeof(BocchiEntityFrameworkCoreModule),
    typeof(BocchiHttpApiModule),
    typeof(AbpAutofacModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpAspNetCoreComponentsServerBasicThemeModule),
    typeof(AbpAspNetCoreMvcUiBasicThemeModule),
    typeof(AbpIdentityBlazorServerModule),
    typeof(AbpTenantManagementBlazorServerModule),
    typeof(AbpSettingManagementBlazorServerModule)
)]
public class BocchiBlazorModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        // http cookies
        context.Services.AddSameSiteCookiePolicy();

        context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
        {
            options.AddAssemblyResource(
                typeof(BocchiResource),
                typeof(BocchiDomainModule).Assembly,
                typeof(BocchiDomainSharedModule).Assembly,
                typeof(BocchiApplicationModule).Assembly,
                typeof(BocchiApplicationContractsModule).Assembly,
                typeof(BocchiBlazorModule).Assembly
            );
        });

        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("Bocchi");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        // Toolbar
        Configure<AbpToolbarOptions>(options => { options.Contributors.Add(new BocchiToolbarContributor()); });

        ConfigureAuthentication(context);
        ConfigureUrls(configuration);
        ConfigureBundles();
        ConfigureAutoMapper();
        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureSwaggerServices(context.Services);
        ConfigureAutoApiControllers();
        ConfigureUiService(context);
        ConfigureRouter(context);
        ConfigureMenu(context);
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults
            .AuthenticationScheme);
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            options.RedirectAllowedUrls.AddRange(configuration["App:RedirectAllowedUrls"].Split(','));
        });
    }

    private void ConfigureBundles()
    {
        Configure<AbpBundlingOptions>(options =>
        {
            // MVC UI
            options.StyleBundles.Configure(
                BasicThemeBundles.Styles.Global,
                bundle => { bundle.AddFiles("/global-styles.css"); }
            );

            //BLAZOR UI
            options.StyleBundles.Configure(
                BlazorBasicThemeBundles.Styles.Global,
                bundle =>
                {
                    bundle.AddFiles("/blazor-global-styles.css");
                    //You can remove the following line if you don't use Blazor CSS isolation for components
                    bundle.AddFiles("/Bocchi.Blazor.styles.css");
                }
            );
        });
    }

    private void ConfigureVirtualFileSystem(IWebHostEnvironment hostingEnvironment)
    {
        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<BocchiDomainSharedModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}Bocchi.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<BocchiDomainModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}Bocchi.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<BocchiApplicationContractsModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}Bocchi.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<BocchiApplicationModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}Bocchi.Application"));
                options.FileSets.ReplaceEmbeddedByPhysical<BocchiBlazorModule>(hostingEnvironment.ContentRootPath);
            });
        }
    }

    private void ConfigureSwaggerServices(IServiceCollection services)
    {
        services.AddAbpSwaggerGen(
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Bocchi API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
            }
        );
    }

    private void ConfigureUiService(ServiceConfigurationContext context)
    {
        context.Services
            .AddBootstrap5Providers()
            .AddFontAwesomeIcons();
        context.Services.AddBootstrapBlazor();
    }

    private void ConfigureMenu(ServiceConfigurationContext context)
    {
        Configure<AbpNavigationOptions>(options => { options.MenuContributors.Add(new BocchiMenuContributor()); });
    }

    private void ConfigureRouter(ServiceConfigurationContext context)
    {
        Configure<AbpRouterOptions>(options => { options.AppAssembly = typeof(BocchiBlazorModule).Assembly; });
    }

    private void ConfigureAutoApiControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(BocchiApplicationModule).Assembly);
        });
    }

    private void ConfigureAutoMapper()
    {
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<BocchiBlazorModule>(); });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var env = context.GetEnvironment();
        var app = context.GetApplicationBuilder();


        app.UseAbpRequestLocalization(options =>
        {
            options.RequestCultureProviders.RemoveAll(provider =>
                provider is AcceptLanguageHeaderRequestCultureProvider);
        });

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        // app.UseHttpsRedirection();
        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();
        // http cookies
        app.UseCookiePolicy();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();

        if (MultiTenancyConsts.IsEnabled)
#pragma warning disable CS0162
        {
            app.UseMultiTenancy();
        }
#pragma warning restore CS0162

        app.UseUnitOfWork();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseAbpSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "Bocchi API"); });
        // Hangfire仪表盘
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            AsyncAuthorization = new[] { new CustomHangfireAuthorizeFilter() }
        });
        app.UseConfiguredEndpoints();
    }
}