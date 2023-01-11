using System.Threading.Tasks;
using Bocchi.PluginSwitch;

namespace Bocchi.SoraBotCore;

public interface IPluginSwitchService
{
    public Task<bool> CheckPlugin(string pluginName, long recordId, PluginSwitchType pluginSwitchType);
}