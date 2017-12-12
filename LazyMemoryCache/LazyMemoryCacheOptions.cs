using System;
using System.ComponentModel.DataAnnotations;

namespace Mooski.Caching
{
    public class LazyMemoryCacheOptions
    {
        public TimeSpan Expiration { get; }

        [Required]
        public string Key { get; }

        [Required]
        public object Lock { get; }

        public LazyMemoryCacheOptions(TimeSpan expiration, string key, object @lock)
        {
            Expiration = expiration;
            Key = key;
            Lock = @lock;

            Validator.ValidateObject(this, new ValidationContext(this));
        }
    }
}
