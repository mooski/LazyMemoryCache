using System;
#if NETSTANDARD2_0
using Microsoft.Extensions.Caching.Memory;
#elif NET45
using System.Runtime.Caching;
#endif
using Mooski.Caching.Extensions;

namespace Mooski.Caching
{
    /// <summary>
    /// Provides support for lazy in-memory caching.
    /// </summary>
    public class LazyMemoryCache<T>
    {
        private Func<T> ValueFactory { get; }

#if NETSTANDARD2_0
        private IMemoryCache MemoryCache { get; }
#elif NET45
        private MemoryCache MemoryCache { get; }
#endif

        private LazyMemoryCacheOptions Options { get; }

        /// <summary>
        /// Gets the lazily initialized value. This value will come from the cache object with the provided key if it exists, or from the value factory if not.
        /// </summary>
        public T Value
        {
            get
            {
                var isInCache = MemoryCache.TryGetCacheValue<T>(Options.Key, out var value);

                if (!isInCache)
                {
                    lock (Options.Lock)
                    {
                        isInCache = MemoryCache.TryGetCacheValue<T>(Options.Key, out value);

                        if (!isInCache)
                        {
                            value = ValueFactory();
                            MemoryCache.Set(Options.Key, value, Options.Clock.UtcNow.Add(Options.Expiration));
                        }
                    }
                }

                return value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyMemoryCache{T}"/> class.
        /// </summary>
        public LazyMemoryCache(
            Func<T> valueFactory,
#if NETSTANDARD2_0
            IMemoryCache memoryCache,
#elif NET45
            MemoryCache memoryCache,
#endif
            LazyMemoryCacheOptions options)
        {
            ValueFactory = valueFactory;
            MemoryCache = memoryCache;
            Options = options;
        }

        /// <summary>
        /// Resets the value by removing the cache object with the provided key from cache. This forces the value to be read from the value factory again.
        /// </summary>
        public void Reset()
        {
            lock (Options.Lock)
            {
                MemoryCache.Remove(Options.Key);
            }
        }
    }
}
