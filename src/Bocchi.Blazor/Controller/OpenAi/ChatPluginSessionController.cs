using System.Threading.Tasks;
using Bocchi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.BlobStoring;

namespace Bocchi.Blazor.Controller.OpenAi;

[Route("api/app/chat-plugin-session-web")]
public class ChatPluginSessionController : BocchiController
{
    private readonly IBlobContainer _blobContainer;

    public ChatPluginSessionController(IBlobContainer blobContainer)
    {
        _blobContainer = blobContainer;
    }

    [HttpGet]
    [Route("file")]
    public async Task<IActionResult> GetFile(string key)
    {
        var file = await _blobContainer.GetOrNullAsync($"chat_gpt_blob:{key}.json");
        if (file == null)
        {
            return new NotFoundResult();
        }

        return new FileStreamResult(file, "application/octet-stream")
        {
            FileDownloadName = $"{key}.json"
        };
    }
}