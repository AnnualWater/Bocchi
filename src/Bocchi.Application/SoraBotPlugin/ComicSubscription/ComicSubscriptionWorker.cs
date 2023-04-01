using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Bocchi.PluginSwitch;
using Bocchi.SoraBotCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sora.Entities;
using Sora.Entities.Segment;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace Bocchi.ComicSubscription;

/// <summary>
/// 检查番剧更新后台任务
/// </summary>
public class ComicSubscriptionWorker : AsyncPeriodicBackgroundWorkerBase
{
    public ComicSubscriptionWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory) : base(timer,
        serviceScopeFactory)
    {
        Timer.Period = 600000;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var cancellationToken = workerContext.CancellationToken;
        Logger.LogInformation("定时任务启动：检查番剧更新");
        // 检查番剧更新
        // 获取服务
        var soraService = ServiceProvider.GetRequiredService<ISoraBotService>();
        var searchService = ServiceProvider.GetRequiredService<SearchService>();
        var repository = ServiceProvider.GetRequiredService<IRepository<ComicSubscriptionEntity>>();
        var checkService = ServiceProvider.GetRequiredService<IPluginSwitchService>();
        // 初始化资源
        var soraApis = soraService.GetAllApi().ToList();
        var comicSubscriptions = await repository.GetListAsync(i => !i.IsEnd, cancellationToken: cancellationToken);
        var privateComic = comicSubscriptions.Where(i => i.ScheduledType == ScheduledType.Private);
        var groupComic = comicSubscriptions.Where(i => i.ScheduledType == ScheduledType.Group);
        // uow
        using (var uow = LazyServiceProvider.LazyGetRequiredService<IUnitOfWorkManager>().Begin())
        {
            foreach (var comic in privateComic)
            {
                // 检查插件是否开启
                if (!await checkService.CheckPlugin(typeof(ComicSubscriptionPlugin).FullName, comic.CreateUserId,
                        PluginSwitchType.Private))
                {
                    continue;
                }

                // 获取番剧信息
                var info = await searchService.SearchComicById(comic.ComicId);
                // 检查信息
                if (comic.Episode == info.NowEpisode)
                {
                    continue;
                }

                Logger.LogInformation("番剧[{Id}:{Title}]更新推送给用户[{UserId}]", comic.ComicId, info.Title,
                    comic.CreateUserId);

                // 发送消息
                var (success, _) = await soraApis.TrySendPrivateMessage(comic.CreateUserId, GetUpdateMessage(info));
                if (success)
                {
                    comic.Episode = info.NowEpisode;
                    comic.NewLink = info.LastUrl;
                    if (Regex.IsMatch(info.NowEpisode, "完结"))
                    {
                        comic.IsEnd = true;
                    }

                    await repository.UpdateAsync(comic, cancellationToken: cancellationToken);
                }
            }

            foreach (var comic in groupComic)
            {
                // 检查插件是否开启
                if (!await checkService.CheckPlugin(typeof(ComicSubscriptionPlugin).FullName, comic.GroupId,
                        PluginSwitchType.Group))
                {
                    continue;
                }

                // 获取番剧信息
                var info = await searchService.SearchComicById(comic.ComicId);
                // 检查信息
                if (comic.Episode == info.NowEpisode)
                {
                    continue;
                }

                Logger.LogInformation("番剧[{Id}:{Title}]更新推送给群[{GroupId}]", comic.ComicId, info.Title,
                    comic.GroupId);
                // 发送消息
                var (success, _) = await soraApis.TrySendGroupMessage(comic.GroupId, GetUpdateMessage(info));
                if (success)
                {
                    comic.Episode = info.NowEpisode;
                    comic.NewLink = info.LastUrl;
                    if (Regex.IsMatch(info.NowEpisode, "完结"))
                    {
                        comic.IsEnd = true;
                    }

                    await repository.UpdateAsync(comic, cancellationToken: cancellationToken);
                }
            }

            await uow.CompleteAsync(cancellationToken);
            Logger.LogInformation("定时任务完成：检查番剧更新");
        }
    }


    private static MessageBody GetUpdateMessage(SearchComicInfo info)
        => new()
        {
            SoraSegment.Text($"[樱花]->[{info.Title}]更新了！\n" +
                             $"{info.NowEpisode}\n" +
                             $"https://www.yhdmp.net{info.LastUrl}")
        };
}