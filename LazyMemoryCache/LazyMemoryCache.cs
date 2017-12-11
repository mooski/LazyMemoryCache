using System;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;

namespace Mooski.Caching
{
    /// <summary>
    /// Provides support for lazy in-memory caching.
    /// </summary>
    public class LazyMemoryCache<T>
    {
        private int _isValueCreated = 0;

        private Func<T> ValueFactory { get; }

        private IMemoryCache MemoryCache { get; }

        private TimeSpan Expiration { get; }

        private object CacheLock { get; }

        private string CacheKey { get; }

        /// <summary>
        /// Gets the lazily initialized value of this <see cref="LazyMemoryCache{T}"/> instance from cache if it exists, or from the value factory if not.
        /// </summary>
        public T Value
        {
            get
            {
                Interlocked.Exchange(ref _isValueCreated, 1);

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
        /// Gets a value that indicates whether a value has ever been created for this <see cref="LazyMemoryCache{T}"/> instance.
        /// </summary>
        public bool IsValueCreated
        {
            get
            {
                return _isValueCreated == 1;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyMemoryCache{T}"/> class.
        /// </summary>
        public LazyMemoryCache(Func<T> valueFactory, IMemoryCache memoryCache, TimeSpan expiration)
        {
            ValueFactory = valueFactory;
            MemoryCache = memoryCache;
            Expiration = expiration;
            CacheLock = new object();
            CacheKey = $"LazyMemoryCache.{Guid.NewGuid()}";
        }

        /// <summary>
        /// Resets the value of this <see cref="LazyMemoryCache{T}"/> instance, forcing the value to be read from the value factory again.
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
