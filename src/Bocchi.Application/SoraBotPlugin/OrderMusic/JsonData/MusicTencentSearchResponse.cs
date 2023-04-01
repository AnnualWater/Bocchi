using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bocchi.OrderMusic;

public class Singer
{
    /// <summary>
    /// 歌手名
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class SingerList
{
    /// <summary>
    /// 歌手列表
    /// </summary>
    [JsonPropertyName("singer")]
    public List<Singer> Singer { get; set; }

    /// <summary>
    /// 歌曲ID
    /// </summary>
    [JsonPropertyName("songid")]
    public long Songid { get; set; }

    /// <summary>
    /// 歌曲名
    /// </summary>
    [JsonPropertyName("songname")]
    public string SongName { get; set; }
}

public class Song
{
    /// <summary>
    /// 歌曲列表
    /// </summary>
    [JsonPropertyName("list")]
    public List<SingerList> List { get; set; }
}

public class Data
{
    /// <summary>
    /// 歌曲
    /// </summary>
    [JsonPropertyName("song")]
    public Song Song { get; set; }
}

public class MusicTencentSearchResponse
{
    /// <summary>
    /// 返回数据
    /// </summary>
    [JsonPropertyName("data")]
    public Data Data { get; set; }
}