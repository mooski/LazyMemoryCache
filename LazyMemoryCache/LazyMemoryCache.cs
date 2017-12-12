using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        private LazyMemoryCacheOptions Options { get; }

        /// <summary>
        /// Gets the lazily initialized value. This value will come from the cache object with the provided key if it exists, or from the value factory if not.
        /// </summary>
        public T Value
        {
            get
            {
                var isInCache = MemoryCache.TryGetValue<T>(Options.Key, out var value);

                if (!isInCache)
                {
                    lock (Options.Lock)
                    {
                        isInCache = MemoryCache.TryGetValue<T>(Options.Key, out value);

                        if (!isInCache)
                        {
                            value = ValueFactory();
                            MemoryCache.Set(Options.Key, value, Options.Expiration);
                        }
                    }
                }

                return value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyMemoryCache{T}"/> class.
        /// </summary>
        public LazyMemoryCache(Func<T> valueFactory, IMemoryCache memoryCache, LazyMemoryCacheOptions options)
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
