using System;
using Volo.Abp.Domain.Entities;

namespace Bocchi.Core;

public class IdentityUserIdToTencentUserIdEntity : AggregateRoot
{
    public Guid IdentityUserId { get; set; }
    public long TencentUserId { get; set; }

    public override object[] GetKeys()
        => new object[] { IdentityUserId, TencentUserId };
}