using System.Text.Json.Serialization;

namespace Bocchi.OpenAi;

public class ChatMessage
{
    [JsonPropertyName("role")] public string Role { get; set; }

    [JsonPropertyName("content")] public string Content { get; set; }

    public ChatMessage()
    {
    }

    public ChatMessage(string role, string content)
    {
        Role = role;
        Content = content;
    }
}