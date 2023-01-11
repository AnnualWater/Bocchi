using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bocchi.ComicSubscription;
using Bocchi.SoraBotCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Bocchi.SoraBotPlugin.ComicSubscription;

public class ComicSubscriptionWebService : ApplicationService, IComicSubscriptionWebService
{
    private readonly IRepository<ComicSubscriptionEntity> _repository;
    private readonly ICurrentUserSoraInfoService _currentSoraUser;
    private readonly SearchService _searchService;

    public ComicSubscriptionWebService(IRepository<ComicSubscriptionEntity> repository,
        ICurrentUserSoraInfoService currentSoraUser, SearchService searchService)
    {
        _repository = repository;
        _currentSoraUser = currentSoraUser;
        _searchService = searchService;
    }

    public async Task<List<ComicSubscriptionDto>> GetComicSubscriptionList(ScheduledType type, long groupId = 0)
    {
        var userTencentId = await _currentSoraUser.GetUserTencentId();
        if (userTencentId == null)
        {
            return new List<ComicSubscriptionDto>();
        }

        switch (type)
        {
            case ScheduledType.Private:

                return (await _repository.GetListAsync(e =>
                        e.ScheduledType == ScheduledType.Private && e.CreateUserId == userTencentId))
                    .Select(e => ObjectMapper.Map<ComicSubscriptionEntity, ComicSubscriptionDto>(e))
                    .ToList();

            case ScheduledType.Group:
                var userMemberInfo = await _currentSoraUser.GetGroupMemberInfo(groupId);
                if (userMemberInfo == null)
                {
                    return new List<ComicSubscriptionDto>();
                }

                return (await _repository.GetListAsync(e =>
                        e.ScheduledType == ScheduledType.Group && e.GroupId == groupId))
                    .Select(e => ObjectMapper.Map<ComicSubscriptionEntity, ComicSubscriptionDto>(e))
                    .ToList();
            default: return new List<ComicSubscriptionDto>();
        }
    }

    public async Task DelComicSubscription(ScheduledType type, string comicId, long groupId = 0)
    {
        var userTencentId = await _currentSoraUser.GetUserTencentId();
        if (userTencentId == null)
        {
            return;
        }

        switch (type)
        {
            case ScheduledType.Private:
                var userInfo = await _currentSoraUser.GetUserInfo();
                if (userInfo == null)
                {
                    return;
                }

                var pEntity = await _repository.GetAsync(e =>
                    e.ComicId == comicId &&
                    e.ScheduledType == ScheduledType.Private && e.CreateUserId == userInfo.Value.UserId);
                if (pEntity != null)
                {
                    await _repository.DeleteAsync(pEntity);
                }

                return;
            case ScheduledType.Group:
                var userMemberInfo = await _currentSoraUser.GetGroupMemberInfo(groupId);
                if (userMemberInfo == null)
                {
                    return;
                }

                var gEntity = await _repository.GetAsync(e =>
                    e.ScheduledType == ScheduledType.Group && e.GroupId == groupId);
                if (gEntity != null)
                {
                    await _repository.DeleteAsync(gEntity);
                }

                return;
            default: return;
        }
    }

    public async Task<(bool success, string msg)> SubscribeComicPrivate(string comicId)
    {
        var userId = await _currentSoraUser.GetUserTencentId();
        if (userId == null)
        {
            return (false, "获取用户ID失败");
        }

        var comicInfo = await _searchService.SearchComicById(comicId);
        if (comicInfo == null)
        {
            return (false, "番剧信息获取失败");
        }

        if (Regex.IsMatch(comicInfo.NowEpisode, "完结"))
        {
            return (false, "番剧已完结，无法订阅");
        }

        var e = await _repository.InsertAsync(new ComicSubscriptionEntity
        {
            CreateUserId = (long)userId,
            GroupId = 0,
            ComicId = comicId,
            Title = comicInfo.Title,
            Episode = comicInfo.NowEpisode,
            UpdateInfo = comicInfo.UpdateInfo,
            NewLink = string.Empty,
            ScheduledType = ScheduledType.Private,
            IsEnd = false
        });
        if (e == null)
        {
            return (false, "订阅失败");
        }

        return (true, $"订阅番剧[{comicInfo.Title}:{comicId}]成功");
    }
}