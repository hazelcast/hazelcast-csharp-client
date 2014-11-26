using System;
using System.Collections;
using Hazelcast.Logging;

namespace Hazelcast.IO.Serialization
{
	internal sealed class FactoryIdHelper
	{
        public const string SpiDsFactory = "hazelcast.serialization.ds.spi";
		public const string PartitionDsFactory = "hazelcast.serialization.ds.partition";
		public const string ClientDsFactory = "hazelcast.serialization.ds.client";
		public const string MapDsFactory = "hazelcast.serialization.ds.map";
		public const string QueueDsFactory = "hazelcast.serialization.ds.queue";
		public const string MultimapDsFactory = "hazelcast.serialization.ds.multimap";
		public const string CollectionDsFactory = "hazelcast.serialization.ds.collection";
		public const string ExecutorDsFactory = "hazelcast.serialization.ds.executor";
		public const string TopicDsFactory = "hazelcast.serialization.ds.topic";
		public const string LockDsFactory = "hazelcast.serialization.ds.lock";
		public const string SemaphoreDsFactory = "hazelcast.serialization.ds.semaphore";
		public const string AtomicLongDsFactory = "hazelcast.serialization.ds.atomic_long";
		public const string CdlDsFactory = "hazelcast.serialization.ds.cdl";
		public const string AtomicReferenceDsFactory = "hazelcast.serialization.ds.atomic_reference";
		public const string ReplicatedMapDsFactory = "hazelcast.serialization.ds.replicated_map";
		public const string AggregationsDsFactory = "hazelcast.serialization.ds.aggregations";
		public const string MapReduceDsFactory = "hazelcast.serialization.ds.map_reduce";
		public const string WebDsFactory = "hazelcast.serialization.ds.web";
		public const string CacheDsFactory = "hazelcast.serialization.ds.cache";
		public const string EnterpriseCacheDsFactory = "hazelcast.serialization.ds.enterprise.cache";
		public const string SpiPortableFactory = "hazelcast.serialization.portable.spi";
		public const string PartitionPortableFactory = "hazelcast.serialization.portable.partition";
		public const string ClientPortableFactory = "hazelcast.serialization.portable.client";
		public const string ClientTxnPortableFactory = "hazelcast.serialization.portable.client.txn";
		public const string MapPortableFactory = "hazelcast.serialization.portable.map";
		public const string QueuePortableFactory = "hazelcast.serialization.portable.queue";
		public const string MultimapPortableFactory = "hazelcast.serialization.portable.multimap";
		public const string CollectionPortableFactory = "hazelcast.serialization.portable.collection";
		public const string ExecutorPortableFactory = "hazelcast.serialization.portable.executor";
		public const string TopicPortableFactory = "hazelcast.serialization.portable.topic";
		public const string LockPortableFactory = "hazelcast.serialization.portable.lock";
		public const string SemaphorePortableFactory = "hazelcast.serialization.portable.semaphore";
		public const string AtomicLongPortableFactory = "hazelcast.serialization.portable.atomic_long";
		public const string AtomicReferencePortableFactory = "hazelcast.serialization.portable.atomic_reference";
		public const string CdlPortableFactory = "hazelcast.serialization.portable.cdl";
		public const string ReplicatedPortableFactory = "hazelcast.serialization.portable.replicated_map";
		public const string AggregationsPortableFactory = "hazelcast.serialization.portable.aggregations";
		public const string MapReducePortableFactory = "hazelcast.serialization.portable.map_reduce";
		public const string WebPortableFactory = "hazelcast.serialization.portable.web";
		public const string CachePortableFactory = "hazelcast.serialization.portable.cache";

		public FactoryIdHelper()
		{
		}

		// factory id 0 is reserved for Cluster objects (Data, Address, Member etc)...
		public static int GetFactoryId(string prop, int defaultId)
		{
            IDictionary env = Environment.GetEnvironmentVariables();
            object value = env[prop];
			if (value != null)
			{
				try
				{
                    return Convert.ToInt32(value);
				}
                catch (FormatException e)
				{
					Logger.GetLogger(typeof(FactoryIdHelper)).Finest("Parameter for property prop could not be parsed", e);
				}
			}
			return defaultId;
		}
	}
}
