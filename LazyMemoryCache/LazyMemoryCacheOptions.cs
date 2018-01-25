using System;
using System.ComponentModel.DataAnnotations;
#if NETCOREAPP2_0
using Microsoft.Extensions.Internal;
#elif NET45
using Microsoft.Owin.Infrastructure;
#endif

namespace Mooski.Caching
{
    public class LazyMemoryCacheOptions
    {
        public TimeSpan Expiration { get; }

        [Required]
        public string Key { get; }

        [Required]
        public object Lock { get; }

        public ISystemClock Clock { get; set; }

        public LazyMemoryCacheOptions(TimeSpan expiration, string key, object @lock)
        {
            Expiration = expiration;
            Key = key;
            Lock = @lock;
            Clock = new SystemClock();

            Validator.ValidateObject(this, new ValidationContext(this));
        }
    }
}
