using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bocchi.Core;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;

namespace Bocchi.SoraBotCore.NoPasswordToken;

public class NoPasswordTokenService : INoPasswordTokenService, ISingletonDependency
{
    /// <summary>
    /// 超时时间
    /// </summary>
    private readonly TimeSpan _overtime = new TimeSpan(0, 3, 0);

    private readonly Dictionary<string, (string userId, DateTime dateTime)> _cache = new();

    private readonly IRepository<IdentityUserIdToTencentUserIdEntity> _repository;
    private readonly IdentityUserManager _identityUserManager;
    private readonly IGuidGenerator _guidGenerator;

    public NoPasswordTokenService(IRepository<IdentityUserIdToTencentUserIdEntity> repository,
        IdentityUserManager identityUserManager, IGuidGenerator guidGenerator)
    {
        _repository = repository;
        _identityUserManager = identityUserManager;
        _guidGenerator = guidGenerator;
    }

    public async Task<string> GetLoginToken(long userId)
    {
        // 清除超时Token
        CleanOverTime();
        // 查询是否有未超时Token
        var hasToken = _cache.FirstOrDefault(item => item.Value.userId == userId.ToString());
        if (!string.IsNullOrEmpty(hasToken.Key))
        {
            _cache.Remove(hasToken.Key);
        }
        // 生成Token
        var token = Guid.NewGuid().ToString().Replace("-", "");
        // 检查有无绑定的User
        var rel = await _repository.FindAsync(e => e.TencentUserId == userId);
        if (rel == null)
        {
            var identityUserId = _guidGenerator.Create();
            await _identityUserManager.CreateAsync(
                new IdentityUser(identityUserId, $"qq{userId}", $"{userId}@qq.com")
                {
                    Name = $"qq{userId}"
                },
                $"Qq@{userId}");
            await _repository.InsertAsync(new IdentityUserIdToTencentUserIdEntity
            {
                IdentityUserId = identityUserId,
                TencentUserId = userId
            });
            _cache.Add(token, (identityUserId.ToString(), DateTime.Now));
            return token;
        }
        else
        {
            _cache.Add(token, (rel.IdentityUserId.ToString(), DateTime.Now));
            return token;
        }
    }

    public Task<string> GetUserId(string token)
    {
        CleanOverTime();
        if (!_cache.ContainsKey(token))
        {
            return Task.FromResult(Guid.Empty.ToString());
        }

        var (userId, _) = _cache[token];

        _cache.Remove(token);
        return Task.FromResult(userId);
    }

    private void CleanOverTime()
    {
        _cache.RemoveAll(item =>
        {
            var (_, (_, date)) = item;
            return date - DateTime.Now > _overtime;
        });
    }
}