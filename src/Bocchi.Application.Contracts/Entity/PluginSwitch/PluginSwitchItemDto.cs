using System;
using Volo.Abp.Application.Dtos;

namespace Bocchi.PluginSwitch;

public class PluginSwitchItemDto : EntityDto<Guid>
{
    public string PluginFullName { get; set; }
    public long RecordId { get; set; }
    public PluginSwitchType PluginSwitchType { get; set; }
    public PluginBanLevel PluginBanLevel { get; set; }
}