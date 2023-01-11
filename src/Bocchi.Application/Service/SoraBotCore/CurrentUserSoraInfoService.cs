using System.Threading.Tasks;
using Bocchi.Core;
using Sora.Entities.Info;
using Sora.Enumeration.ApiType;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Bocchi.SoraBotCore;

public class CurrentUserSoraInfoService : ICurrentUserSoraInfoService, ITransientDependency
{
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<IdentityUserIdToTencentUserIdEntity> _identityUserToTencentUserIdRepository;
    private readonly ISoraBotService _soraBotService;

    private IdentityUserIdToTencentUserIdEntity _identityUserIdToTencentUserIdEntityCache;

    private UserInfo? _userInfoCache;

    private async Task<IdentityUserIdToTencentUserIdEntity> GetIdentityUserIdToTencentUserId()
        => _identityUserIdToTencentUserIdEntityCache ??=
            await _identityUserToTencentUserIdRepository.FindAsync(e => e.IdentityUserId == _currentUser.Id);


    public CurrentUserSoraInfoService(ICurrentUser currentUser,
        IRepository<IdentityUserIdToTencentUserIdEntity> identityUserToTencentUserIdRepository,
        ISoraBotService soraBotService)
    {
        _currentUser = currentUser;
        _identityUserToTencentUserIdRepository = identityUserToTencentUserIdRepository;
        _soraBotService = soraBotService;
    }

    public async Task<UserInfo?> GetUserInfo()
    {
        if (_userInfoCache != null)
        {
            return _userInfoCache;
        }

        var rel = await GetIdentityUserIdToTencentUserId();
        if (rel == null)
        {
            throw new EntityNotFoundException(typeof(IdentityUserIdToTencentUserIdEntity), _currentUser.Id);
        }

        var soraApi = _soraBotService.GetAllApi();
        foreach (var api in soraApi)
        {
            var resp = await api.GetUserInfo(rel.TencentUserId);
            if (resp.apiStatus.RetCode == ApiStatusType.Ok)
            {
                _userInfoCache = resp.userInfo;
                return resp.userInfo;
            }
        }

        return null;
    }

    public async Task<long?> GetUserTencentId()
        => (await GetIdentityUserIdToTencentUserId())?.TencentUserId;

    public async Task<GroupMemberInfo> GetGroupMemberInfo(long groupId)
    {
        var rel = await GetIdentityUserIdToTencentUserId();
        if (rel == null)
        {
            throw new EntityNotFoundException(typeof(IdentityUserIdToTencentUserIdEntity), _currentUser.Id);
        }

        var soraApi = _soraBotService.GetAllApi();
        foreach (var api in soraApi)
        {
            var resp = await api.GetGroupMemberInfo(groupId, rel.TencentUserId);
            if (resp.apiStatus.RetCode == ApiStatusType.Ok)
            {
                return resp.memberInfo;
            }
        }

        return null;
    }
}