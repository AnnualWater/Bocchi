using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bocchi.OrderMusic;

public class Artists
{
    /// <summary>
    /// 歌手名
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class Songs
{
    /// <summary>
    /// 歌曲ID
    /// </summary>
    [JsonPropertyName("id")]
    public long Id { get; set; }

    /// <summary>
    /// 歌曲名
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 歌手列表
    /// </summary>
    [JsonPropertyName("artists")]
    public List<Artists> Artists { get; set; }
}

public class Result
{
    /// <summary>
    /// 歌曲列表
    /// </summary>
    [JsonPropertyName("songs")]
    public List<Songs> Songs { get; set; }

    /// <summary>
    /// 歌曲计数
    /// </summary>
    [JsonPropertyName("songCount")]
    public long SongCount { get; set; }
}

public class Music163SearchResponse
{
    /// <summary>
    /// 结果
    /// </summary>
    [JsonPropertyName("result")]
    public Result Result { get; set; }

    /// <summary>
    /// 状态码
    /// </summary>
    [JsonPropertyName("code")]
    public long Code { get; set; }
}