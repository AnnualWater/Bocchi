namespace Bocchi.Permissions;

public static class BocchiPermissions
{
    public const string GroupName = "Bocchi";

    //Add your own permission names. Example:
    //public const string MyPermission1 = GroupName + ".MyPermission1";

    public static class OpenAiApi
    {
        public const string Default = GroupName + ".OpenAiApi";
        public const string Chat = Default + ".Chat";
    }
}