using Bocchi.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Bocchi.Permissions;

public class BocchiPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(BocchiPermissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(BocchiPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<BocchiResource>(name);
    }
}
