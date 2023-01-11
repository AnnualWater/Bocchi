using System.Collections.Generic;
using System.Threading.Tasks;
using Bocchi.PluginSwitch;
using Volo.Abp.Application.Services;

namespace Bocchi.SoraBotCore;

public interface IPluginSwitchWebService : IApplicationService
{
    public Task<List<PluginSwitchItemDto>> GetPrivatePluginSwitch();
    public Task<PluginSwitchItemDto> BanPrivatePlugin(string pluginFullName);
    public Task<PluginSwitchItemDto> AllowPrivatePlugin(string pluginFullName);

    public Task<List<PluginSwitchItemDto>> GetGroupPluginSwitch(long groupId);
    public Task<PluginSwitchItemDto> BanGroupPluginSwitch(long groupId, string pluginFullName);
    public Task<PluginSwitchItemDto> AllowGroupPluginSwitch(long groupId, string pluginFullName);
}