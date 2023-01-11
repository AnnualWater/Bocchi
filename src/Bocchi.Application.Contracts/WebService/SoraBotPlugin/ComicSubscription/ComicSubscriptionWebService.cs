using System.Collections.Generic;
using System.Threading.Tasks;
using Bocchi.ComicSubscription;
using Volo.Abp.Application.Services;

namespace Bocchi.SoraBotPlugin.ComicSubscription;

public interface IComicSubscriptionWebService : IApplicationService
{
    public Task<List<ComicSubscriptionDto>> GetComicSubscriptionList(ScheduledType type, long groupId = 0);
    public Task DelComicSubscription(ScheduledType type, string comicId, long groupId = 0);
    public Task<(bool success,string msg)> SubscribeComicPrivate(string comicId);
}