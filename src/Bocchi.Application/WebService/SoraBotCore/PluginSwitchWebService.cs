using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bocchi.PluginSwitch;
using Microsoft.AspNetCore.Authorization;
using Sora.Enumeration.EventParamsType;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Bocchi.SoraBotCore;

[Authorize]
public class PluginSwitchWebService : ApplicationService, IPluginSwitchWebService
{
    private readonly IRepository<PluginSwitchEntity> _pluginSwitchItemRepository;
    private readonly ICurrentUserSoraInfoService _currentSoraUser;

    public PluginSwitchWebService(IRepository<PluginSwitchEntity> pluginSwitchItemRepository,
        ICurrentUserSoraInfoService currentSoraUser)
    {
        _pluginSwitchItemRepository = pluginSwitchItemRepository;
        _currentSoraUser = currentSoraUser;
    }


    public async Task<List<PluginSwitchItemDto>> GetPrivatePluginSwitch()
    {
        var userTencentId = await _currentSoraUser.GetUserTencentId();
        if (userTencentId == null)
        {
            return new List<PluginSwitchItemDto>();
        }

        return (await _pluginSwitchItemRepository.GetListAsync(e =>
                e.PluginSwitchType == PluginSwitchType.Private && e.RecordId == userTencentId))
            .Select(e => ObjectMapper.Map<PluginSwitchEntity, PluginSwitchItemDto>(e)).ToList();
    }

    public async Task<PluginSwitchItemDto> BanPrivatePlugin(string pluginFullName)
    {
        var soraUserInfo = await _currentSoraUser.GetUserInfo();
        if (soraUserInfo == null)
        {
            return null;
        }

        var item = await _pluginSwitchItemRepository.GetAsync(e => e.PluginSwitchType == PluginSwitchType.Private &&
                                                                   e.RecordId == soraUserInfo.Value.UserId &&
                                                                   e.PluginFullName == pluginFullName);
        if (item == null)
        {
            return null;
        }
        else
        {
            item.PluginBanLevel = PluginBanLevel.OwnerBan;
            await _pluginSwitchItemRepository.UpdateAsync(item);
            return ObjectMapper.Map<PluginSwitchEntity, PluginSwitchItemDto>(item);
        }
    }

    public async Task<PluginSwitchItemDto> AllowPrivatePlugin(string pluginFullName)
    {
        var soraUserInfo = await _currentSoraUser.GetUserInfo();
        if (soraUserInfo == null)
        {
            return null;
        }

        var item = await _pluginSwitchItemRepository.GetAsync(e => e.PluginSwitchType == PluginSwitchType.Private &&
                                                                   e.RecordId == soraUserInfo.Value.UserId &&
                                                                   e.PluginFullName == pluginFullName);
        if (item == null)
        {
            return null;
        }
        else
        {
            item.PluginBanLevel = PluginBanLevel.Allow;
            await _pluginSwitchItemRepository.UpdateAsync(item);
            return ObjectMapper.Map<PluginSwitchEntity, PluginSwitchItemDto>(item);
        }
    }

    public async Task<List<PluginSwitchItemDto>> GetGroupPluginSwitch(long groupId)
    {
        var memberInfo = await _currentSoraUser.GetGroupMemberInfo(groupId);
        if (memberInfo == null || memberInfo.Role < MemberRoleType.Admin)
        {
            return new List<PluginSwitchItemDto>();
        }

        return (await _pluginSwitchItemRepository.GetListAsync(e =>
                e.PluginSwitchType == PluginSwitchType.Group && e.RecordId == groupId))
            .Select(e => ObjectMapper.Map<PluginSwitchEntity, PluginSwitchItemDto>(e)).ToList();
    }

    public async Task<PluginSwitchItemDto> BanGroupPluginSwitch(long groupId, string pluginFullName)
    {
        var memberInfo = await _currentSoraUser.GetGroupMemberInfo(groupId);
        if (memberInfo == null || memberInfo.Role < MemberRoleType.Admin)
        {
            return null;
        }

        var item = await _pluginSwitchItemRepository.GetAsync(e => e.PluginSwitchType == PluginSwitchType.Group &&
                                                                   e.RecordId == groupId &&
                                                                   e.PluginFullName == pluginFullName);
        if (item == null)
        {
            return null;
        }
        else
        {
            if (item.PluginBanLevel == PluginBanLevel.OwnerBan)
            {
                return ObjectMapper.Map<PluginSwitchEntity, PluginSwitchItemDto>(item);
            }

            item.PluginBanLevel = memberInfo.Role == MemberRoleType.Owner
                ? PluginBanLevel.OwnerBan
                : PluginBanLevel.AdminBan;
            await _pluginSwitchItemRepository.UpdateAsync(item);
            return ObjectMapper.Map<PluginSwitchEntity, PluginSwitchItemDto>(item);
        }
    }

    public async Task<PluginSwitchItemDto> AllowGroupPluginSwitch(long groupId, string pluginFullName)
    {
        var memberInfo = await _currentSoraUser.GetGroupMemberInfo(groupId);
        if (memberInfo == null || memberInfo.Role < MemberRoleType.Admin)
        {
            return null;
        }

        var item = await _pluginSwitchItemRepository.GetAsync(e => e.PluginSwitchType == PluginSwitchType.Group &&
                                                                   e.RecordId == groupId &&
                                                                   e.PluginFullName == pluginFullName);
        if (item == null)
        {
            return null;
        }


        if (memberInfo.Role == MemberRoleType.Owner)
        {
            item.PluginBanLevel = PluginBanLevel.Allow;
        }
        else
        {
            if (item.PluginBanLevel == PluginBanLevel.AdminBan)
            {
                item.PluginBanLevel = PluginBanLevel.Allow;
            }
        }

        await _pluginSwitchItemRepository.UpdateAsync(item);
        return ObjectMapper.Map<PluginSwitchEntity, PluginSwitchItemDto>(item);
    }
}