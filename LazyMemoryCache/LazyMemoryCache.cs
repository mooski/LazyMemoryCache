using System;
using Microsoft.Extensions.Caching.Memory;

namespace Mooski.Caching
{
    /// <summary>
    /// Provides support for lazy in-memory caching.
    /// </summary>
    public class LazyMemoryCache<T>
    {
        private Func<T> ValueFactory { get; }

        private IMemoryCache MemoryCache { get; }

        private TimeSpan Expiration { get; }

        private object CacheLock { get; }

        private string CacheKey { get; }

        /// <summary>
        /// Gets the lazily initialized value. This value will come from the cache object with the key <see cref="CacheKey"/> if it exists, or from the value factory if not.
        /// </summary>
        public T Value
        {
            get
            {
                var isInCache = MemoryCache.TryGetValue<T>(CacheKey, out var value);

                if (!isInCache)
                {
                    lock (CacheLock)
                    {
                        isInCache = MemoryCache.TryGetValue<T>(CacheKey, out value);

                        if (!isInCache)
                        {
                            value = ValueFactory();
                            MemoryCache.Set(CacheKey, value, Expiration);
                        }
                    }
                }

                return value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyMemoryCache{T}"/> class.
        /// </summary>
        public LazyMemoryCache(Func<T> valueFactory, IMemoryCache memoryCache, TimeSpan expiration, string cacheKey, ref object cacheLock)
        {
            ValueFactory = valueFactory;
            MemoryCache = memoryCache;
            Expiration = expiration;
            CacheKey = cacheKey;
            CacheLock = cacheLock;
        }

        /// <summary>
        /// Resets the value by removing the cache object with the key <see cref="CacheKey"/> from cache. This forces the value to be read from the value factory again.
        /// </summary>
        public void Reset()
        {
            lock (CacheLock)
            {
                MemoryCache.Remove(CacheKey);
            }
        }
    }
}
