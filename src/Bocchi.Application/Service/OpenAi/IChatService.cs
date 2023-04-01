using System.Threading.Tasks;

namespace Bocchi.OpenAi;

public interface IChatService
{
    public Task<string> Chat(string message);
    public void ClearMessages();

    public string GetSessionJson();
}