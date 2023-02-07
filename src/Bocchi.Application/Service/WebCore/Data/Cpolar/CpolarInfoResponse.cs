using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bocchi.WebCore.Cpolar;

public class CpolarInfoResponse
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("data")]
    public CpolarInfoResponseData CpolarInfoResponseData { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; }
}

public class PublishTunnelsItem
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("public_url")]
    public string PublicUrl { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("proto")]
    public string Proto { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("addr")]
    public string Addr { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("create_datetime")]
    public string CreateDatetime { get; set; }
}

public class ItemsItem
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("public_url")]
    public string PublicUrl { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("publish_tunnels")]
    public List<PublishTunnelsItem> PublishTunnels { get; set; }
}

public class CpolarInfoResponseData
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("items")]
    public List<ItemsItem> Items { get; set; }
}