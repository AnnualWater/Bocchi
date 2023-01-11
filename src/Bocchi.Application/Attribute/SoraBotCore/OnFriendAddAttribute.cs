using System;

namespace Bocchi.SoraBotCore;

public class OnFriendAddAttribute : Attribute, IOnSoraMessageAttribute
{
    public uint Priority { get; init; }

    public OnFriendAddAttribute(uint priority = 100)
    {
        Priority = priority;
    }
}