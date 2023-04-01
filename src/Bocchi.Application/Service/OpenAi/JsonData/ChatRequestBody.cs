using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bocchi.OpenAi;

public class ChatRequestBody
{
    [JsonPropertyName("model")] public string Model { get; set; }

    [JsonPropertyName("max_tokens")] public int? MaxTokens { get; set; }

    [JsonPropertyName("messages")] public List<ChatMessage> Messages { get; set; }
}