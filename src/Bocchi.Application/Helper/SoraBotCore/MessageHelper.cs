using System;
using System.Threading.Tasks;
using Sora.Entities;
using Sora.EventArgs.SoraEvent;

namespace Bocchi.SoraBotCore;

public static class MessageHelper
{
    public static async Task<(ApiStatus apiStatus, int messageId)> TryReply(this BaseSoraEventArgs args,
        MessageBody message, TimeSpan? timeout = null)
        => args switch
        {
            PrivateMessageEventArgs eventArgs => await eventArgs.Reply(message, timeout),
            GroupMessageEventArgs eventArgs => await eventArgs.Reply(message, timeout),
            _ => (default, 0)
        };
}