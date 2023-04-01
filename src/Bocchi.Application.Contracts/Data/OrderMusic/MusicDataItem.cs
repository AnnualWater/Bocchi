namespace Bocchi.OrderMusic;

public class MusicDataItem
{
    /// <summary>
    /// 序号
    /// </summary>
    public int Num { get; set; }

    /// <summary>
    /// 音乐ID
    /// </summary>
    public long MusicId { get; set; }

    /// <summary>
    /// 歌曲名
    /// </summary>
    public string SongName { get; set; }

    /// <summary>
    /// 歌手
    /// </summary>
    public string Singer { get; set; }

    /// <summary>
    /// 平台
    /// </summary>
    public string Platform { get; set; }

    public string GetNum
    {
        get
        {
            return Platform switch
            {
                "网易云音乐" => $"w{Num}",
                "QQ音乐" => $"q{Num}",
                _ => $"{Num}"
            };
        }
        set { }
    }
}