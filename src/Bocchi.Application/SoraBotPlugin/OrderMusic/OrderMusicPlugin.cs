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
using Sora.OnebotAdapter;

namespace Bocchi.SoraBotPlugin.OrderMusic;

public class OrderMusicPlugin : IOnGroupMessagePlugin, IOnPrivateMessagePlugin
{
    public int Priority => 100;

    public EventAdapter.EventAsyncCallBackHandler<GroupMessageEventArgs> OnGroupMessage =>
        async (_, args) => { await StepMethod(args); };

    public EventAdapter.EventAsyncCallBackHandler<PrivateMessageEventArgs> OnPrivateMessage =>
        async (_, args) => { await StepMethod(args); };

    private readonly SearchMusicService _searchMusicService;
    private readonly ISearchCacheService _cacheService;
    private readonly string _shootUrl;
    private readonly IScreenShootService _screenShootService;
    private readonly ILogger<OrderMusicPlugin> _logger;

    public OrderMusicPlugin(ILogger<OrderMusicPlugin> logger,
        IConfiguration configuration, SearchMusicService searchMusicService, ISearchCacheService cacheService,
        IScreenShootService screenShootService)
    {
        _logger = logger;
        _searchMusicService = searchMusicService;
        _cacheService = cacheService;
        _screenShootService = screenShootService;
        _shootUrl = configuration.GetValue<string>("Urls", null) + "/sora/order_music";
    }

    private string _search;
    private List<MusicDataItem> _music163SearchResponse;
    private List<MusicDataItem> _musicTencentSearchResponse;
    private FuncStep _step = FuncStep.Check;

    private enum FuncStep
    {
        Check,
        Search,
        CheckNum
    }

    private async Task StepMethod(BaseMessageEventArgs args)
    {
        switch (_step)
        {
            case FuncStep.Check:
                await Check(args);
                break;
            case FuncStep.Search:
                await Search(args);
                break;

            case FuncStep.CheckNum:
                await CheckNum(args);
                break;
        }
    }

    private async Task Check(BaseMessageEventArgs args)
    {
        var rawText = args.Message.RawText;
        if (rawText == null)
        {
            return;
        }

        if (Regex.IsMatch(rawText, "^(网易云|wyy)"))
        {
            var r = Regex.Match(rawText, "[0-9]+");
            if (!r.Success)
            {
                await args.TryReply("序号匹配失败");
                return;
            }

            var num = int.Parse(r.Value);
            await args.TryReply(new MessageBody(new List<SoraSegment>()
            {
                SoraSegment.Music(MusicShareType.Netease, num)
            }));
            return;
        }

        if (Regex.IsMatch(rawText, "^(QQ音乐|qqmusic)"))
        {
            var r = Regex.Match(rawText, "[0-9]+");
            if (!r.Success)
            {
                await args.TryReply("序号匹配失败");
                return;
            }

            var num = int.Parse(r.Value);
            await args.TryReply(new MessageBody(new List<SoraSegment>()
            {
                SoraSegment.Music(MusicShareType.QQMusic, num)
            }));
            return;
        }

        if (!Regex.IsMatch(rawText, "^点歌"))
        {
            return;
        }

        if (rawText.Length == 2)
        {
            await args.TryReply("请输入搜索关键词");
            _step = FuncStep.Search;
            throw new WaitPluginException();
        }

        _search = rawText[2..];
        _step = FuncStep.Search;
        await Search(args);
    }

    private async Task Search(BaseMessageEventArgs args)
    {
        _search ??= args.Message.RawText;

        // 用服务搜索
        _music163SearchResponse = await _searchMusicService.SearchSong163(_search);
        _musicTencentSearchResponse = await _searchMusicService.SearchSongTencent(_search);


        if (_music163SearchResponse.Count == 0 && _musicTencentSearchResponse.Count == 0)
        {
            await args.TryReply($"未搜索到歌曲 {_search}");
            throw new WaitPluginException();
        }

        try
        {
            #region 发送图片

            // 设定缓存
            var cache = new List<MusicDataItem>();
            cache.AddRange(_music163SearchResponse);
            cache.AddRange(_musicTencentSearchResponse);

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
            if (_music163SearchResponse.Count != 0)
            {
                repText += "网易云：\n";
                repText = _music163SearchResponse.Aggregate(repText,
                    (current, songInfo) =>
                        current + $"[w{songInfo.Num}]:{songInfo.SongName} - {songInfo.Singer}" + "\n");
            }

            if (_musicTencentSearchResponse.Count != 0)
            {
                repText += "QQ音乐：\n";
                repText = _musicTencentSearchResponse.Aggregate(repText,
                    (current, songInfo) =>
                        current + $"[q{songInfo.Num}]:{songInfo.SongName} - {songInfo.Singer}" + "\n");
            }

            await args.TryReply(repText);

            #endregion
        }

        _step = FuncStep.CheckNum;
        throw new WaitPluginException();
    }

    private async Task CheckNum(BaseMessageEventArgs args)
    {
        if (_music163SearchResponse == null || _musicTencentSearchResponse == null)
        {
            return;
        }

        var numText = args.Message.RawText;
        if (numText.Any() && (numText[0] == 'c' || numText[0] == 'C'))
        {
            return;
        }

        if (Regex.IsMatch(numText, "^[c|C|w|W|q|Q][0-9]$"))
        {
            if (!int.TryParse(numText[1].ToString(), out var num))
            {
                await args.TryReply("序号错误，请重新输入");
                throw new WaitPluginException();
            }

            if (numText[0] == 'w' || numText[0] == 'W')
            {
                var song = _music163SearchResponse.Where(x => x.Num == num).ToList();
                if (!song.Any())
                {
                    await args.TryReply("序号不存在，请重新输入");
                    throw new WaitPluginException();
                }

                await args.TryReply(new MessageBody(new List<SoraSegment>
                {
                    SoraSegment.Music(MusicShareType.Netease, song.First().MusicId)
                }));
                return;
            }

            if (numText[0] == 'q' || numText[0] == 'Q')
            {
                var song = _musicTencentSearchResponse.Where(x => x.Num == num).ToList();
                if (!song.Any())
                {
                    await args.TryReply("序号不存在");
                    throw new WaitPluginException();
                }

                await args.TryReply(new MessageBody(new List<SoraSegment>
                {
                    SoraSegment.Music(MusicShareType.QQMusic, song.First().MusicId)
                }));
                return;
            }
        }

        await args.TryReply("序号错误，请重新输入");
        throw new WaitPluginException();
    }
}