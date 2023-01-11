namespace Bocchi.PluginSwitch;

public enum PluginBanLevel
{
    /// <summary>
    /// 没有禁用
    /// </summary>
    Allow,
    /// <summary>
    /// 管理员禁用
    /// </summary>
    AdminBan,

    /// <summary>
    /// 群主禁用
    /// </summary>
    OwnerBan,

    /// <summary>
    /// 超级用户禁用
    /// </summary>
    SuperBan
}