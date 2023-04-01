using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.DependencyInjection;

namespace Bocchi.OpenAi;

public class ChatSessionService : ISingletonDependency
{
    private readonly IServiceProvider _serviceProvider;

    private readonly Dictionary<long, IChatService> _sessions = new();

    public ChatSessionService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IChatService GetService(long userId)
    {
        if (_sessions.TryGetValue(userId, out var service))
        {
            return service;
        }
        else
        {
            _sessions[userId] = _serviceProvider.GetService<IChatService>();
            return _sessions[userId];
        }
    }
}