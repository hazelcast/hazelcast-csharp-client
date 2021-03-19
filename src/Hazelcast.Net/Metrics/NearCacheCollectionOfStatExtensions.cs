using System.Collections.Generic;
using Hazelcast.NearCaching;

namespace Hazelcast.Metrics
{
    // publishes metrics to the cluster

    internal static class NearCacheCollectionOfStatExtensions
    {
        public static void AddStats(this ICollection<IStat> stats, NearCache nearCache)
        {
            var prefix = "nc" + CollectionOfStatExtensions.NameSeparator + nearCache.Name.TrimStart('/');
            var ncstats = nearCache.Statistics;

            // these are the stats currently sent by the Java v4 client

            stats.AddStat(prefix, "creationTime", ncstats.CreationTime);
            stats.AddStat(prefix, "evictions", ncstats.Evictions);
            stats.AddStat(prefix, "hits", ncstats.Hits);

            stats.AddEmptyStat(prefix, "lastPersistenceDuration");
            stats.AddEmptyStat(prefix, "lastPersistenceKeyCount");
            stats.AddEmptyStat(prefix, "lastPersistenceTime");
            stats.AddEmptyStat(prefix, "lastPersistenceWrittenBytes");

            stats.AddStat(prefix, "misses", ncstats.Misses);
            stats.AddStat(prefix, "ownedEntryCount", ncstats.EntryCount);
            stats.AddStat(prefix, "expirations", ncstats.Expirations);

            stats.AddEmptyStat(prefix, "invalidations");
            stats.AddEmptyStat(prefix, "invalidationRequests");

            stats.AddEmptyStat(prefix, "ownedEntryMemoryCost");
            //stats.AddEmptyStat(prefix, "lastPersistenceFailure"); // "if"...
        }
    }
}
