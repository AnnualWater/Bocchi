using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bocchi.ComicSubscription;
using Bocchi.ScreenShoot;
using Bocchi.SoraBotCore;
using Bocchi.SoraBotCore.NoPasswordToken;
using Bocchi.WebCore;
using Microsoft.Extensions.Configuration;
using Sora.Entities;
using Sora.Entities.Segment;
using Sora.EventArgs.SoraEvent;
using Volo.Abp.Domain.Repositories;

namespace Bocchi.SoraBotPlugin.ComicSubscription;

public class ComicSubscriptionPlugin : PluginWithSession
{
    private readonly string _searchUrl;
    private readonly SearchService _searchService;
    private readonly IRepository<ComicSubscriptionEntity> _repository;
    private readonly IScreenShootService _screenShootService;
    private readonly INoPasswordTokenService _tokenService;
    private readonly IWebCoreService _webCoreService;
    public override uint Priority => 100;

    public ComicSubscriptionPlugin(IPluginParamService pluginParamService, IConfiguration configuration,
        IScreenShootService screenShootService, SearchService searchService,
        IRepository<ComicSubscriptionEntity> repository, INoPasswordTokenService tokenService,
        IWebCoreService webCoreService) :
        base(pluginParamService)
    {
        _screenShootService = screenShootService;
        _searchService = searchService;
        _repository = repository;
        _tokenService = tokenService;
        _webCoreService = webCoreService;
        _searchUrl = configuration.GetValue<string>("Urls", null) + "/sora/comic_subscription/search";
    }

    [OnGroupMessage(1)]
    [OnPrivateMessage(1)]
    public async Task Check(BaseMessageEventArgs args)
    {
        var rawText = args.TryGetRawText();
        if (Regex.IsMatch(rawText, "^番剧搜索"))
        {
            await SearchComic(args, rawText);
            return;
        }

        if (Regex.IsMatch(rawText, "^番剧订阅"))
        {
            await SubscribeComic(args, rawText);
            return;
        }

        if (Regex.IsMatch(rawText, "^番剧列表$"))
        {
            await GetSubscribeComic(args);
            return;
        }

        if (Regex.IsMatch(rawText, "^番剧删除"))
        {
            await GetSubscribeComic(args);
        }
    }

    private async Task SubscribeComic(BaseMessageEventArgs args, string rawText)
    {
        var comicId = Regex.Match(rawText, "[0-9]+").Value;
        if (string.IsNullOrEmpty(comicId))
        {
            await args.TryReply("番剧ID解析错误！");
            return;
        }

        var entity = args switch
        {
            PrivateMessageEventArgs pArgs => await _repository.FindAsync(e =>
                e.ScheduledType == ScheduledType.Private && e.CreateUserId == pArgs.SenderInfo.UserId &&
                e.ComicId == comicId),
            GroupMessageEventArgs gArgs => await _repository.FindAsync(e => e.ScheduledType == ScheduledType.Group &&
                                                                            e.GroupId == gArgs.SourceGroup.Id &&
                                                                            e.ComicId == comicId),
            _ => null
        };

        if (entity != null)
        {
            await args.TryReply($"番剧[{entity.Title}]已经订阅过了");
            return;
        }

        await args.TryReply("正在获取番剧信息，请稍后......");
        var comicInfo = await _searchService.SearchComicById(comicId);
        if (comicInfo == null)
        {
            await args.TryReply("番剧信息获取失败");
            return;
        }

        if (Regex.IsMatch(comicInfo.NowEpisode, "完结"))
        {
            await args.TryReply("番剧已完结，无法订阅");
            return;
        }

        switch (args)
        {
            case PrivateMessageEventArgs pArgs2:
                await _repository.InsertAsync(new ComicSubscriptionEntity
                {
                    CreateUserId = pArgs2.SenderInfo.UserId,
                    GroupId = 0,
                    ComicId = comicId,
                    Title = comicInfo.Title,
                    Episode = comicInfo.NowEpisode,
                    UpdateInfo = comicInfo.UpdateInfo,
                    NewLink = string.Empty,
                    ScheduledType = ScheduledType.Private,
                    IsEnd = false
                });
                break;
            case GroupMessageEventArgs gArgs2:
                await _repository.InsertAsync(new ComicSubscriptionEntity
                {
                    CreateUserId = gArgs2.SenderInfo.UserId,
                    GroupId = gArgs2.SourceGroup.Id,
                    ComicId = comicId,
                    Title = comicInfo.Title,
                    Episode = comicInfo.NowEpisode,
                    UpdateInfo = comicInfo.UpdateInfo,
                    NewLink = string.Empty,
                    ScheduledType = ScheduledType.Group,
                    IsEnd = false
                });
                break;
        }

        await args.TryReply($"番剧[{comicInfo.Title}]订阅成功\n" +
                            $"当前：{comicInfo.NowEpisode}\n" +
                            $"更新：{comicInfo.UpdateInfo}");
    }

    private async Task GetSubscribeComic(BaseMessageEventArgs args)
    {
        if (args is PrivateMessageEventArgs)
        {
            var token = await _tokenService.GetLoginToken(args.Sender.Id);
            await args.TryReply(
                $"{await _webCoreService.GetWebUrl()}api/bocchi/account/no_password?token={token}&redirect=/sora/comic_subscription/list?type=private");
        }

        if (args is GroupMessageEventArgs gArgs)
        {
            await args.TryReply(
                $"{await _webCoreService.GetWebUrl()}sora/comic_subscription/list?type=group&gid={gArgs.SourceGroup.Id}");
        }
    }

    private async Task SearchComic(BaseMessageEventArgs args, string rawText)
    {
        if (rawText.Length <= 4)
        {
            return;
        }

        await args.TryReply("正在搜索，请稍后......");
        var searchKey = rawText[4..];
        var b64 = await _screenShootService.ScreenShoot2B64($"{_searchUrl}?shoot=true&search={searchKey}",
            "//div[@id='screenshot']");
        await args.TryReply(new MessageBody(new List<SoraSegment>()
        {
            SoraSegment.Image($"base64://{b64}")
        }));
    }
}