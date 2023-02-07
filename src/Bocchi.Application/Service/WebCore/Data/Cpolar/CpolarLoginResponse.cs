using System.Text.Json.Serialization;

namespace Bocchi.WebCore.Cpolar;

public class CpolarLoginResponse
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("data")]
    public CpolarLoginResponseData Data { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("code")]
    public int Code { get; set; }
}

public class CpolarLoginResponseData
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("token")]
    public string Token { get; set; }
}