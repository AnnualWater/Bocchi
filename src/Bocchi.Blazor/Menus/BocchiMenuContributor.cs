using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.SettingManagement;
using Volo.Abp.SettingManagement.Blazor.Menus;
using Volo.Abp.UI.Navigation;
using Volo.Abp.Users;

namespace Bocchi.Blazor.Menus;

public class BocchiMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        // 不修改User菜单
        if (context.Menu.Name == "User") 
        {
            return;
        }
        context.Menu.Items.RemoveAll(_ => true);
        context.Menu.Items.Insert(
            0,
            new ApplicationMenuItem(
                BocchiMenus.Home,
                "首页",
                "/",
                icon: "fas fa-home",
                order: 0
            )
        );
        context.Menu.Items.Insert(1,
            new ApplicationMenuItem(
                BocchiMenus.Help,
                "帮助",
                "/help",
                icon: "fas fa-question-circle"));

        var currentUser = context.ServiceProvider.GetRequiredService<ICurrentUser>();
        if (currentUser.IsAuthenticated)
        {
            var soraMenu = new ApplicationMenuItem(BocchiMenus.Sora.SoraHome, "Sora", icon: "fas fa-cube");
            soraMenu.AddItem(new ApplicationMenuItem(
                BocchiMenus.Sora.PluginManager,
                "插件管理",
                "/sora/plugin_switch/private",
                icon: "fas fa-plug"));
            soraMenu.AddItem(new ApplicationMenuItem(
                BocchiMenus.Sora.ComicSubscription,
                "番剧订阅管理",
                "/sora/comic_subscription/list?type=private"
            ));
            context.Menu.Items.Insert(2,soraMenu);
        }

        var settingMenu = new ApplicationMenuItem(
            SettingManagementMenus.GroupName,
            "管理");
        if (await context.IsGrantedAsync(SettingManagementPermissions.Emailing))
        {
            settingMenu.AddItem(new ApplicationMenuItem(
                SettingManagementMenus.GroupName,
                "设置管理",
                "/setting-management"));
        }

        if (await context.IsGrantedAsync(IdentityPermissions.Users.Default))
        {
            settingMenu.AddItem(new ApplicationMenuItem(
                SettingManagementMenus.GroupName,
                "用户管理",
                "/identity/users"));
        }

        if (await context.IsGrantedAsync(IdentityPermissions.Roles.Default))
        {
            settingMenu.AddItem(new ApplicationMenuItem(
                SettingManagementMenus.GroupName,
                "角色管理",
                "/identity/roles"));
        }

        context.Menu.AddItem(settingMenu);
    }
}