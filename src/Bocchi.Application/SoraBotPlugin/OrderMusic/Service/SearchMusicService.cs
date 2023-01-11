using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using RestSharp;
using Volo.Abp.DependencyInjection;

namespace Bocchi.SoraBotPlugin.OrderMusic;

public class SearchMusicService : ITransientDependency
{
    /// <summary>
    /// 点歌加载量
    /// </summary>
    private const int SongSearchCount = 5;

    /// <summary>
    /// 网易云搜索请求Client
    /// </summary>
    private readonly RestClient _music163 = new("http://music.163.com/api/search/get/web");

    /// <summary>
    /// QQ音乐搜索请求Client
    /// </summary>
    private readonly RestClient _musicTencent = new("http://c.y.qq.com/soso/fcgi-bin/search_for_qq_cp");

    public SearchMusicService()
    {
        // 加载默认参数
        _music163.AddDefaultHeader("Referer", "http://music.163.com");
        _music163.AddDefaultParameter("type", "1");
        _music163.AddDefaultParameter("offset", "0");
        _music163.AddDefaultParameter("total", "true");
        _music163.AddDefaultParameter("limit", "60");

        _musicTencent.AddDefaultHeader("Referer", "http://c.y.qq.com");
        _musicTencent.AddDefaultParameter("format", "json");
        _musicTencent.AddDefaultParameter("p", "0");
        _musicTencent.AddDefaultParameter("n", "5");
    }

    /// <summary>
    /// 网易云搜索
    /// </summary>
    /// <param name="search">关键词</param>
    /// <returns></returns>
    public async Task<List<MusicDataItem>> SearchSong163(string search)
    {
        var request = new RestRequest
        {
            Method = Method.Get
        };
        // 添加搜索关键词
        request.AddParameter("s", search);
        // 进行搜索
        var restResponse = await _music163.ExecuteAsync(request);
        if (restResponse.StatusCode != HttpStatusCode.OK || restResponse.Content == null)
        {
            return new();
        }

        // 序列化
        var response = JsonSerializer.Deserialize<Music163SearchResponse>(restResponse.Content);
        var rep = new List<MusicDataItem>();
        foreach (var song in response?.Result?.Songs!)
        {
            var art = song?.Artists?.Aggregate("", (current, artist) => current + (artist.Name + "、"));

            art = art?[..^1] ?? "";
            rep.Add(new MusicDataItem()
            {
                Num = rep.Count + 1,
                MusicId = song?.Id ?? 0,
                SongName = song?.Name ?? "",
                Singer = art,
                Platform = "网易云音乐"
            });

            if (rep.Count >= SongSearchCount)
            {
                return rep;
            }
        }

        return new();
    }

    /// <summary>
    /// QQ音乐搜索
    /// </summary>
    /// <param name="search">关键词</param>
    /// <returns></returns>
    public async Task<List<MusicDataItem>> SearchSongTencent(string search)
    {
        var request = new RestRequest
        {
            Method = Method.Get
        };
        request.AddParameter("w", search);
        var restResponse = await _musicTencent.ExecuteAsync(request);
        if (restResponse.StatusCode != HttpStatusCode.OK || restResponse.Content == null)
        {
            return new();
        }

        var response = JsonSerializer.Deserialize<MusicTencentSearchResponse>(restResponse.Content);
        var rep = new List<MusicDataItem>();
        foreach (var song in response?.Data?.Song?.List!)
        {
            var art = song?.Singer?.Aggregate("", (current, artist) => current + (artist.Name + "、"));

            art = art?[..^1] ?? "";
            rep.Add(new MusicDataItem()
            {
                Num = rep.Count + 1,
                MusicId = song?.Songid ?? 0,
                SongName = song?.SongName ?? "",
                Singer = art,
                Platform = "QQ音乐"
            });
            if (rep.Count >= SongSearchCount)
            {
                return rep;
            }
        }

        return new();
    }
}