namespace Bocchi.Blazor.Menus;

public class BocchiMenus
{
    private const string Prefix = "Bocchi";
    public const string Home = Prefix + ".Home";

    //Add your menu items here...

    public const string Help = Prefix + ".Help";

    public static class Sora
    {
        private const string SoraPrefix = Prefix + ".Sora";
        public const string SoraHome = SoraPrefix + ".Home";

        public const string PluginManager = SoraPrefix + ".PluginManager";
        public const string ComicSubscription = SoraPrefix + ".ComicSubscription";
    }
}