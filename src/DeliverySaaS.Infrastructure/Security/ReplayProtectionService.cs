using DeliverySaaS.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace DeliverySaaS.Infrastructure.Security;

public class ReplayProtectionService : IReplayProtectionService
{
    private readonly IMemoryCache _memoryCache;

    public ReplayProtectionService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public bool IsReplay(string key, TimeSpan ttl)
    {
        if (_memoryCache.TryGetValue(key, out _))
        {
            return true;
        }

        _memoryCache.Set(key, true, ttl);
        return false;
    }
}
