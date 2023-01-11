using System;

namespace Bocchi.SoraBotCore;

public class OnGroupAdminChangeAttribute : Attribute, IOnSoraMessageAttribute
{
    public uint Priority { get; init; }

    public OnGroupAdminChangeAttribute(uint priority = 100)
    {
        Priority = priority;
    }
}