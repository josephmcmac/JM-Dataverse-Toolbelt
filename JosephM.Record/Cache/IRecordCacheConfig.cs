using System.Collections.Generic;

namespace JosephM.Record.Cache
{
    /// <summary>
    ///     Interface For Configuring A RecordCacheProvider
    /// </summary>
    public interface IRecordCacheConfig
    {
        /// <summary>
        ///     Determines If The Cache Will Load All Records Of The Type Into Cache Or Just The Queried Record When A Record Of A
        ///     Specific Type Is Queried Through The Cache
        /// </summary>
        bool CacheAll { get; }

        /// <summary>
        ///     Defines Specific Recrrd Types To Exclude From The CacheAll Property Rule And Only Load The Queried Record
        ///     Use If A Large Volume Of Records For The types
        /// </summary>
        IEnumerable<string> CacheAllExclude { get; }
    }
}