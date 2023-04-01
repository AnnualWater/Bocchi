using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bocchi.OpenAi;

public class ChatResponse
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("created")]
    public int? Created { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("usage")]
    public Usage Usage { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("choices")]
    public List<ChoicesItem> Choices { get; set; }
}


public class Usage
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}
public class ChoicesItem
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("message")]
    public ChatMessage Message { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }
}
