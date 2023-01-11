using System;

namespace Bocchi.SoraBotCore;

public class OnGroupPokeAttribute : Attribute, IOnSoraMessageAttribute
{
    public uint Priority { get; init; }

    public OnGroupPokeAttribute(uint priority = 100)
    {
        Priority = priority;
    }
}