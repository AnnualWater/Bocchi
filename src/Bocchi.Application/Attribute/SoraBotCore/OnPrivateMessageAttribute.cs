using System;

namespace Bocchi.SoraBotCore;

public class OnPrivateMessageAttribute : Attribute, IOnSoraMessageAttribute
{
    public uint Priority { get; init; }

    public OnPrivateMessageAttribute(uint priority = 100)
    {
        Priority = priority;
    }
}