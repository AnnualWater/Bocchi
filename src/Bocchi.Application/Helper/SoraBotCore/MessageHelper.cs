using System;
using System.Threading.Tasks;
using Sora.Entities;
using Sora.EventArgs.SoraEvent;

namespace Bocchi.SoraBotCore;

public static class MessageHelper
{
    public static async Task<(ApiStatus apiStatus, int messageId)> TryReply(this BaseSoraEventArgs args,
        MessageBody message, TimeSpan? timeout = null)
    {
        switch (args)
        {
            case PrivateMessageEventArgs eventArgs:
                return await eventArgs.Reply(message, timeout);
            case GroupMessageEventArgs eventArgs:
                return await eventArgs.Reply(message, timeout);
        }

        return (default, 0);
    }

    public static string TryGetRawText(this BaseSoraEventArgs args)
    {
        return args switch
        {
            BaseMessageEventArgs eventArgs => eventArgs.Message.RawText,
            _ => null
        };
    }
}