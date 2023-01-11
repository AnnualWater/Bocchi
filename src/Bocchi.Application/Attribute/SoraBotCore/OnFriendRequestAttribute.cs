using System;

namespace Bocchi.SoraBotCore;

public class OnFriendRequestAttribute : Attribute, IOnSoraMessageAttribute
{
    public uint Priority { get; init; }

    public OnFriendRequestAttribute(uint priority = 100)
    {
        Priority = priority;
    }
}