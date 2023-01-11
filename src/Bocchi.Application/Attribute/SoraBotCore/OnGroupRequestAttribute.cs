using System;

namespace Bocchi.SoraBotCore;

public class OnGroupRequestAttribute : Attribute, IOnSoraMessageAttribute
{
    public uint Priority { get; init; }

    public OnGroupRequestAttribute(uint priority = 100)
    {
        Priority = priority;
    }
}