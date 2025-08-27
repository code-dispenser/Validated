using Microsoft.Extensions.Caching.Hybrid;
using System.Collections.Concurrent;

namespace Validated.ValueObject.Infrastructure.Common.Caching;

public class CacheProvider(HybridCache hybridCache)
{
    private readonly HybridCache _hybridCache = hybridCache;

    public async Task<T> GetOrCreate<T>(Func<Task<T>> getData, string itemKey, int cacheForMinutes)
    
       => await _hybridCache.GetOrCreateAsync<T>(itemKey, async _ => await getData(), new HybridCacheEntryOptions { LocalCacheExpiration = TimeSpan.FromMinutes(cacheForMinutes) });
    
}
