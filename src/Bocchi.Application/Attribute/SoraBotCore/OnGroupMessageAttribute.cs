using System;

namespace Bocchi.SoraBotCore;

public class OnGroupMessageAttribute : Attribute, IOnSoraMessageAttribute
{
    public uint Priority { get; init; }

    public OnGroupMessageAttribute(uint priority = 100)
    {
        Priority = priority;
    }
}