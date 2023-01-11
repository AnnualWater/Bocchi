using System;

namespace Bocchi.SoraBotCore;

public class OnFriendRecallAttribute : Attribute, IOnSoraMessageAttribute
{
    public uint Priority { get; init; }

    public OnFriendRecallAttribute(uint priority = 100)
    {
        Priority = priority;
    }
}