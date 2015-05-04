using System.Collections.Generic;

namespace JosephM.Record.Cache
{
    /// <summary>
    ///     General Use Implementation Of IRecordCacheConfig
    /// </summary>
    public class RecordCacheConfig : IRecordCacheConfig
    {
        public bool CacheAll { get; set; }

        public IEnumerable<string> CacheAllExclude { get; set; }
    }
}