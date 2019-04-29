#if NETSTANDARD2_0
using Microsoft.Extensions.Caching.Memory;
#elif NET45
using System.Runtime.Caching;
#endif

namespace Mooski.Caching.Extensions
{
    internal static class MemoryCacheExtensions
    {
#if NETSTANDARD2_0
        internal static bool TryGetCacheValue<T>(this IMemoryCache memoryCache, object key, out T value)
        {
            return memoryCache.TryGetValue<T>(key, out value);
        }
#elif NET45
        internal static bool TryGetCacheValue<T>(this MemoryCache memoryCache, object key, out T value)
        {
            var cacheItem = memoryCache.GetCacheItem((string)key);

            value = cacheItem != null ? (T)cacheItem.Value : default(T);

            return cacheItem != null;
        }
#endif
    }
}
