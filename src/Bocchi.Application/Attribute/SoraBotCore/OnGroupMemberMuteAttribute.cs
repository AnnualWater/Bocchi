using System;

namespace Bocchi.SoraBotCore;

public class OnGroupMemberMuteAttribute : Attribute, IOnSoraMessageAttribute
{
    public uint Priority { get; init; }

    public OnGroupMemberMuteAttribute(uint priority = 100)
    {
        Priority = priority;
    }
}