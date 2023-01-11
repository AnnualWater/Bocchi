using System;
using Volo.Abp.Domain.Entities;

namespace Bocchi.PluginSwitch;

public class PluginSwitchEntity : Entity<Guid>
{
    public string PluginFullName { get; set; }
    public long RecordId { get; set; }
    public PluginSwitchType PluginSwitchType { get; set; }
    public PluginBanLevel PluginBanLevel { get; set; }
}