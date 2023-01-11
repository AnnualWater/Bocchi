using System;

namespace Bocchi.SoraBotCore;

public class OnGroupRecallAttribute : Attribute, IOnSoraMessageAttribute
{
    public uint Priority { get; init; }

    public OnGroupRecallAttribute(uint priority = 100)
    {
        Priority = priority;
    }
}