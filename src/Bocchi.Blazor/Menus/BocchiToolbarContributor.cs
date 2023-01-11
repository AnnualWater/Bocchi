using System.Threading.Tasks;
using Bocchi.Blazor.Component.Toolbar;
using Volo.Abp.AspNetCore.Components.Web.Theming.Toolbars;

namespace Bocchi.Blazor.Menus;

public class BocchiToolbarContributor : IToolbarContributor
{
    public Task ConfigureToolbarAsync(IToolbarConfigurationContext context)
    {
        context.Toolbar.Items.RemoveAll(_ => true);
        context.Toolbar.Items.Add(new ToolbarItem(typeof(AccuontToolbar)));
        return Task.CompletedTask;
    }
}