using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Mooski.Caching
{
    public class LazyMemoryCacheTests
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ISystemClock> _systemClockMock;
        private readonly IMemoryCache _memoryCache;

        public LazyMemoryCacheTests(ITestOutputHelper output)
        {
            _output = output;
            _systemClockMock = new Mock<ISystemClock>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions { Clock = _systemClockMock.Object });

            _systemClockMock.Setup(c => c.UtcNow).Returns(new DateTime(2000, 1, 1, 0, 0, 0));
        }

        [Fact]
        public void ValueIsCorrect()
        {
            var lazyMemoryCache = new LazyMemoryCache<string>(
                () =>
                {
                    return "Value";
                },
                _memoryCache, TimeSpan.FromMinutes(1));

            Assert.Equal("Value", lazyMemoryCache.Value);
        }

        [Fact]
        public void ValueFactoryIsOnlyCalledWhenCacheExpires()
        {
            var factoryCallCount = 0;

            var lazyMemoryCache = new LazyMemoryCache<int>(
                () =>
                {
                    Interlocked.Increment(ref factoryCallCount);

                    return 0;
                },
                _memoryCache, TimeSpan.FromMinutes(1));

            Assert.Equal(0, factoryCallCount);

            // Access the value for the first time:
            var value = lazyMemoryCache.Value;

            Assert.Equal(1, factoryCallCount);

            // Access the value again - this time we should get the cached value:
            value = lazyMemoryCache.Value;

            Assert.Equal(1, factoryCallCount);

            // Fast-forward the time by 59 seconds:
            _systemClockMock.Setup(c => c.UtcNow).Returns(new DateTime(2000, 1, 1, 0, 0, 59));

            // Access the value again - we should get the cached value again:
            value = lazyMemoryCache.Value;

            Assert.Equal(1, factoryCallCount);

            // Fast-forward the time by 2 more seconds (1 minute and 1 second since the test started):
            _systemClockMock.Setup(c => c.UtcNow).Returns(new DateTime(2000, 1, 1, 0, 1, 1));

            // Access the value again - this time the cache should have expired so the value factory method should get called again:
            value = lazyMemoryCache.Value;

            Assert.Equal(2, factoryCallCount);
        }

        [Fact]
        public void IsValueCreatedWorksCorrectly()
        {
            var lazyMemoryCache = new LazyMemoryCache<int>(
                () =>
                {
                    return 0;
                },
                _memoryCache, TimeSpan.FromMinutes(1));

            Assert.False(lazyMemoryCache.IsValueCreated);

            var value = lazyMemoryCache.Value;

            Assert.True(lazyMemoryCache.IsValueCreated);
        }

        [Fact]
        public void ResetWorksCorrectly()
        {
            var factoryCallCount = 0;

            var lazyMemoryCache = new LazyMemoryCache<int>(
                () =>
                {
                    Interlocked.Increment(ref factoryCallCount);

                    return 0;
                },
                _memoryCache, TimeSpan.FromMinutes(1));

            Assert.Equal(0, factoryCallCount);

            // Access the value for the first time:
            var value = lazyMemoryCache.Value;

            Assert.Equal(1, factoryCallCount);

            lazyMemoryCache.Reset();

            // Access the value again - the cache has been reset so the value factory method should get called again:
            value = lazyMemoryCache.Value;

            Assert.Equal(2, factoryCallCount);
        }

        [Fact]
        public void AsyncCallsWorkCorrectly()
        {
            var factoryCallCount = 0;

            var lazyMemoryCache = new LazyMemoryCache<int>(
                () =>
                {
                    Interlocked.Increment(ref factoryCallCount);

                    return 0;
                },
                _memoryCache, TimeSpan.FromMinutes(1));

            Assert.Equal(0, factoryCallCount);

            var task1 = new Task(() => { var value = lazyMemoryCache.Value; });
            var task2 = new Task(() => { var value = lazyMemoryCache.Value; });
            var task3 = new Task(() => { var value = lazyMemoryCache.Value; });

            task1.Start();
            task2.Start();
            task3.Start();

            Task.WaitAll(task1, task2, task3);

            Assert.Equal(1, factoryCallCount);
        }
    }
}
