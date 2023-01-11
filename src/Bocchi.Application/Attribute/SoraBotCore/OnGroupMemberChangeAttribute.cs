using System;

namespace Bocchi.SoraBotCore;

public class OnGroupMemberChangeAttribute : Attribute, IOnSoraMessageAttribute
{
    public uint Priority { get; init; }

    public OnGroupMemberChangeAttribute(uint priority = 100)
    {
        Priority = priority;
    }
}