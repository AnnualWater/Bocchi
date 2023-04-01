using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bocchi.OpenAi;

public interface IChatWebService
{
    public Task<string> Chat(List<ChatMessageDto> messages);
}