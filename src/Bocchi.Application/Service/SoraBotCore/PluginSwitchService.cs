using System.Collections.Generic;
using System.Threading.Tasks;
using Bocchi.AutoAllowFriend;
using Bocchi.NoPasswordLogin;
using Bocchi.PluginSwitch;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Bocchi.SoraBotCore;

public class PluginSwitchService : IPluginSwitchService, ITransientDependency
{
    private IRepository<PluginSwitchEntity> _repository;

    private static readonly List<string> SystemPluginName = new()
    {
        typeof(NoPasswordLoginPlugin).FullName,
        typeof(AutoAllowFriendPlugin).FullName
    };

    public PluginSwitchService(IRepository<PluginSwitchEntity> repository)
    {
        _repository = repository;
    }

    public async Task<bool> CheckPlugin(string pluginName, long recordId, PluginSwitchType pluginSwitchType)
    {
        if (SystemPluginName.Contains(pluginName))
        {
            return true;
        }

        var item = await _repository.FindAsync(e => e.PluginFullName == pluginName &&
                                                    e.PluginSwitchType == pluginSwitchType &&
                                                    e.RecordId == recordId);
        if (item == null)
        {
            await _repository.InsertAsync(new PluginSwitchEntity
            {
                PluginFullName = pluginName,
                RecordId = recordId,
                PluginSwitchType = pluginSwitchType,
                PluginBanLevel = PluginBanLevel.Allow
            });
            return true;
        }

        return item.PluginBanLevel == PluginBanLevel.Allow;
    }
}