using System;

namespace Bocchi.SoraBotCore;

public class OnGroupCardUpdateAttribute : Attribute, IOnSoraMessageAttribute
{
    public uint Priority { get; init; }

    public OnGroupCardUpdateAttribute(uint priority = 100)
    {
        Priority = priority;
    }
}