using System;

namespace Bocchi.SoraBotCore;

public class PluginAttribute : Attribute
{
    /// <summary>
    /// 插件名
    /// </summary>
    public string PluginName { get; init; }
    /// <summary>
    /// 插件描述
    /// </summary>
    public string Description { get; init; }
    /// <summary>
    /// 插件帮助
    /// </summary>
    public string Help { get; init; }

    /// <summary>
    /// 插件信息
    /// </summary>
    /// <param name="pluginName">插件名</param>
    /// <param name="description">插件描述</param>
    /// <param name="help">插件帮助</param>
    public PluginAttribute(string pluginName, string description, string help = "")
    {
        PluginName = pluginName;
        Description = description;
        Help = help;
    }
}