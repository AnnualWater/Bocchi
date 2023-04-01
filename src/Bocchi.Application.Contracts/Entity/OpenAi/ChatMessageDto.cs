namespace Bocchi.OpenAi;

public class ChatMessageDto
{
    public string Role { get; set; }

    public string Content { get; set; }

    public ChatMessageDto()
    {
    }

    public ChatMessageDto(string role, string content)
    {
        Role = role;
        Content = content;
    }
}