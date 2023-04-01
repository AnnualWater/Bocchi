using System;
using System.IO.MemoryMappedFiles;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bocchi.SoraBotCore;
using Bocchi.WebCore;
using Sora.Entities.Segment;
using Sora.EventArgs.SoraEvent;
using Sora.OnebotAdapter;
using Volo.Abp.BlobStoring;

namespace Bocchi.OpenAi;

public class ChatPlugin : IOnGroupMessagePlugin, IOnPrivateMessagePlugin
{
    public int Priority => 100;

    public EventAdapter.EventAsyncCallBackHandler<GroupMessageEventArgs> OnGroupMessage =>
        async (_, args) => { await Check(args); };

    public EventAdapter.EventAsyncCallBackHandler<PrivateMessageEventArgs> OnPrivateMessage =>
        async (_, args) => { await Check(args); };

    private readonly ChatSessionService _chatSessionService;
    private readonly IWebCoreService _webCoreService;
    private readonly IBlobContainer _blobContainer;

    public ChatPlugin(ChatSessionService chatSessionService, IWebCoreService webCoreService,
        IBlobContainer blobContainer)
    {
        _chatSessionService = chatSessionService;
        _webCoreService = webCoreService;
        _blobContainer = blobContainer;
    }

    private async Task Check(BaseMessageEventArgs eventArgs)
    {
        var rawText = eventArgs.Message.RawText;
        if (Regex.IsMatch(rawText, "^chat"))
        {
            await Chat(eventArgs, rawText);
            return;
        }

        if (Regex.IsMatch(rawText, "^clear$"))
        {
            await ClearSession(eventArgs);
            return;
        }

        if (Regex.IsMatch(rawText, "^save$"))
        {
            await SaveJson(eventArgs);
            return;
        }
    }

    private async Task SaveJson(BaseMessageEventArgs eventArgs)
    {
        var service = _chatSessionService.GetService(eventArgs.Sender.Id);
        var json = service.GetSessionJson();
        if (json == string.Empty)
        {
            await eventArgs.TryReply(SoraSegment.Reply(eventArgs.Message.MessageId) + "未找打任何记录");
            return;
        }
        else
        {
            // 保存到blob
            var key = Guid.NewGuid().ToString().Replace("-", "").ToLower();
            var fileName = $"chat_gpt_blob:{key}.json";
            await _blobContainer.SaveAsync(fileName, System.Text.Encoding.UTF8.GetBytes(json));
            // 回复API网址
            await eventArgs.TryReply(SoraSegment.Reply(eventArgs.Message.MessageId) +
                                     $"{await _webCoreService.GetWebUrl()}/api/app/chat-plugin-session-web/file?key={key}");
        }
    }

    private async Task ClearSession(BaseMessageEventArgs eventArgs)
    {
        var service = _chatSessionService.GetService(eventArgs.Sender.Id);
        service.ClearMessages();
        await eventArgs.TryReply(SoraSegment.Reply(eventArgs.Message.MessageId) + "清理完成");
    }

    private async Task Chat(BaseMessageEventArgs eventArgs, string rawText)
    {
        var service = _chatSessionService.GetService(eventArgs.Sender.Id);
        if (rawText.Length <= 4)
        {
            await eventArgs.TryReply(SoraSegment.Reply(eventArgs.Message.MessageId) + "问题呢？");
        }

        await eventArgs.TryReply(SoraSegment.Reply(eventArgs.Message.MessageId) + "GPT思考中");
        var response = await service.Chat(rawText[4..]);
        await eventArgs.TryReply(SoraSegment.Reply(eventArgs.Message.MessageId) + response);
    }
}