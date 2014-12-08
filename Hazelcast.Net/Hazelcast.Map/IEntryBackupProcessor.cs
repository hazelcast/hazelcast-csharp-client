using System.Collections.Generic;

namespace Hazelcast.Map
{
    public interface IEntryBackupProcessor<K, V>
    {
        void ProcessBackup(KeyValuePair<K, V> entry);
    }
}