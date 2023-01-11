using System;
using Volo.Abp.Domain.Entities;

namespace Bocchi.Core;

public class IdentityUserIdToTencentUserIdEntity : Entity<Guid>
{
    public Guid IdentityUserId { get; set; }
    public long TencentUserId { get; set; }
}