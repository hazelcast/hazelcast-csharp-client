using Hazelcast.Core;

namespace Hazelcast.Partition.Strategy
{
    internal class DefaultPartitioningStrategy : IPartitioningStrategy
    {
        public virtual object GetPartitionKey(object key)
        {
            var aware = key as IPartitionAware<object>;
            if (aware != null)
            {
                return aware.GetPartitionKey();
            }
            return null;
        }
    }
}