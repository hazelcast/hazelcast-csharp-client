using Hazelcast.Core;

namespace Hazelcast.Partition.Strategy
{
    internal class StringPartitioningStrategy : IPartitioningStrategy
    {
        public virtual object GetPartitionKey(object key)
        {
            if (key is string)
            {
                return GetPartitionKey((string) key);
            }
            return null;
        }

        public static string GetPartitionKey(string key)
        {
            if (key == null)
            {
                return null;
            }
            int firstIndexOf = key.IndexOf('@');
            if (firstIndexOf > -1)
            {
                key = key.Substring(firstIndexOf + 1);
            }
            return key;
        }
    }
}