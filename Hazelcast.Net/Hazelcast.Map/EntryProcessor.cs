using System.Collections.Generic;

namespace Hazelcast.Map
{
    public interface EntryProcessor<K, V>
    {
        /// <summary>Process the entry without worrying about concurrency.</summary>
        /// <remarks>
        ///     Process the entry without worrying about concurrency.
        ///     <p />
        /// </remarks>
        /// <param name="entry">entry to be processes</param>
        /// <returns>result of the process</returns>
        object Process(KeyValuePair<K, V> entry);

        /// <summary>Get the entry processor to be applied to backup entries.</summary>
        /// <remarks>
        ///     Get the entry processor to be applied to backup entries.
        ///     <p />
        /// </remarks>
        /// <returns>back up processor</returns>
        EntryBackupProcessor<K, V> GetBackupProcessor();
    }
}