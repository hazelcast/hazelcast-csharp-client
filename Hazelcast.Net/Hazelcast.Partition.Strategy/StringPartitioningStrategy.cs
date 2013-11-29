using Hazelcast.Core;

namespace Hazelcast.Partition.Strategy
{
    public class StringPartitioningStrategy : IPartitioningStrategy
    {
        public virtual object GetPartitionKey(object key)
        {
            if (key is string)
            {
                return GetPartitionKey((string) key);
            }
            return null;
        }

        public static string GetBaseName(string name)
        {
            if (name == null)
            {
                return null;
            }
            int indexOf = name.IndexOf('@');
            if (indexOf == -1)
            {
                return name;
            }
            return name.Substring(0, indexOf);
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