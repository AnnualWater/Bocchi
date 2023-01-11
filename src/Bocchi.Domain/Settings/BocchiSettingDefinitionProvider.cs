using Volo.Abp.Settings;

namespace Bocchi.Settings;

public class BocchiSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(BocchiSettings.MySetting1));
    }
}
