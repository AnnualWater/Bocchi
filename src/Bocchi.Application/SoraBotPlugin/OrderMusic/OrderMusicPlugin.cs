using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bocchi.ScreenShoot;
using Bocchi.SoraBotCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sora.Entities;
using Sora.Entities.Segment;
using Sora.Enumeration.EventParamsType;
using Sora.EventArgs.SoraEvent;

namespace Bocchi.SoraBotPlugin.OrderMusic;

public class OrderMusicPlugin : PluginWithSession
{
    private readonly SearchMusicService _searchMusicService;
    private readonly ISearchCacheService _cacheService;
    private readonly string _shootUrl;
    private readonly IScreenShootService _screenShootService;
    private readonly ILogger<OrderMusicPlugin> _logger;

    public OrderMusicPlugin(IPluginParamService pluginParamService, ILogger<OrderMusicPlugin> logger,
        IConfiguration configuration, SearchMusicService searchMusicService, ISearchCacheService cacheService,
        IScreenShootService screenShootService) : base(pluginParamService)
    {
        _logger = logger;
        _searchMusicService = searchMusicService;
        _cacheService = cacheService;
        _screenShootService = screenShootService;
        _shootUrl = configuration.GetValue<string>("Urls", null) + "/sora/order_music";
    }

    public override uint Priority => 100;

    [OnGroupMessage(1)]
    [OnPrivateMessage(1)]
    public async Task Check(BaseSoraEventArgs args)
    {
        var rawText = args.TryGetRawText();
        if (rawText == null)
        {
            FinishPlugin();
            return;
        }

        if (Regex.IsMatch(rawText, "^(网易云|wyy)"))
        {
            var r = Regex.Match(rawText, "[0-9]+");
            if (!r.Success)
            {
                await args.TryReply("序号匹配失败");
                FinishPlugin();
                return;
            }

            var num = int.Parse(r.Value);
            await args.TryReply(new MessageBody(new List<SoraSegment>()
            {
                SoraSegment.Music(MusicShareType.Netease, num)
            }));
            FinishPlugin();
            return;
        }

        if (Regex.IsMatch(rawText, "^(QQ音乐|qqmusic)"))
        {
            var r = Regex.Match(rawText, "[0-9]+");
            if (!r.Success)
            {
                await args.TryReply("序号匹配失败");
                FinishPlugin();
                return;
            }

            var num = int.Parse(r.Value);
            await args.TryReply(new MessageBody(new List<SoraSegment>()
            {
                SoraSegment.Music(MusicShareType.QQMusic, num)
            }));
            FinishPlugin();
            return;
        }

        if (!Regex.IsMatch(rawText, "^点歌"))
        {
            FinishPlugin();
            return;
        }

        if (rawText.Length == 2)
        {
            await args.TryReply("请输入搜索关键词");
            NextMethodWaitNewEvent();
            return;
        }

        SetValue("search", rawText[2..]);
    }

    [OnGroupMessage(2)]
    [OnPrivateMessage(2)]
    public async Task Search(BaseSoraEventArgs args)
    {
        var search = GetValue<string>("search");
        if (search == null)
        {
            SetValue("search", args.TryGetRawText());
            search = args.TryGetRawText();
        }

        // 用服务搜索
        var music163SearchResponse = await _searchMusicService.SearchSong163(search);
        var musicTencentSearchResponse = await _searchMusicService.SearchSongTencent(search);


        if (music163SearchResponse.Count == 0 && musicTencentSearchResponse.Count == 0)
        {
            await args.TryReply($"未搜索到歌曲 {search}");
            FinishPlugin();
            return;
        }

        SetValue("music163SearchResponse", music163SearchResponse);
        SetValue("musicTencentSearchResponse", musicTencentSearchResponse);

        try
        {
            #region 发送图片

            // 设定缓存
            var cache = new List<MusicDataItem>();
            cache.AddRange(music163SearchResponse);
            cache.AddRange(musicTencentSearchResponse);

            var cacheId = _cacheService.AddItem(cache);

            // 请求页面
            var b64 = await _screenShootService.ScreenShoot2B64($"{_shootUrl}?id={cacheId}",
                "//div[@id='screenshot']");
            await args.TryReply(new MessageBody(new List<SoraSegment>()
            {
                SoraSegment.Image($"base64://{b64}")
            }));

            #endregion
        }
        catch (Exception e)
        {
            _logger.LogInformation("发送图片失败，选择发送文字");
            _logger.LogDebug(exception: e, "发送图片失败");

            # region 发送文字

            // 整理序号并回复

            var repText = "请输入编号点歌(输入c或C退出点歌)：\n";
            if (music163SearchResponse.Count != 0)
            {
                repText += "网易云：\n";
                repText = music163SearchResponse.Aggregate(repText,
                    (current, songInfo) =>
                        current + $"[w{songInfo.Num}]:{songInfo.SongName} - {songInfo.Singer}" + "\n");
            }

            if (musicTencentSearchResponse.Count != 0)
            {
                repText += "QQ音乐：\n";
                repText = musicTencentSearchResponse.Aggregate(repText,
                    (current, songInfo) =>
                        current + $"[q{songInfo.Num}]:{songInfo.SongName} - {songInfo.Singer}" + "\n");
            }

            await args.TryReply(repText);

            #endregion
        }

        NextMethodWaitNewEvent();
    }

    [OnGroupMessage(3)]
    [OnPrivateMessage(3)]
    public async Task CheckNum(BaseSoraEventArgs args)
    {
        var music163SearchResponse =
            GetValue<List<MusicDataItem>>("music163SearchResponse");
        var musicTencentSearchResponse =
            GetValue<List<MusicDataItem>>("musicTencentSearchResponse");
        if (music163SearchResponse == null || musicTencentSearchResponse == null)
        {
            return;
        }

        var numText = args.TryGetRawText();
        if (numText.Any() && (numText[0] == 'c' || numText[0] == 'C'))
        {
            FinishPlugin();
            return;
        }

        if (Regex.IsMatch(numText, "^[c|C|w|W|q|Q][0-9]$"))
        {
            if (!int.TryParse(numText[1].ToString(), out var num))
            {
                await args.TryReply("序号错误，请重新输入");
                WaitNewEvent();
                return;
            }

            if (numText[0] == 'w' || numText[0] == 'W')
            {
                var song = music163SearchResponse.Where(x => x.Num == num).ToList();
                if (!song.Any())
                {
                    await args.TryReply("序号不存在");
                    WaitNewEvent();
                    return;
                }

                await args.TryReply(new MessageBody(new List<SoraSegment>
                {
                    SoraSegment.Music(MusicShareType.Netease, song.First().MusicId)
                }));
                return;
            }

            if (numText[0] == 'q' || numText[0] == 'Q')
            {
                var song = musicTencentSearchResponse.Where(x => x.Num == num).ToList();
                if (!song.Any())
                {
                    await args.TryReply("序号不存在");
                    WaitNewEvent();
                    return;
                }

                await args.TryReply(new MessageBody(new List<SoraSegment>
                {
                    SoraSegment.Music(MusicShareType.QQMusic, song.First().MusicId)
                }));
                return;
            }
        }

        await args.TryReply("序号错误，请重新输入");
        WaitNewEvent();
    }
}