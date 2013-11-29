using System.Collections.Generic;

namespace Hazelcast.Map
{
    public interface EntryBackupProcessor<K, V>
    {
        void ProcessBackup(KeyValuePair<K, V> entry);
    }
}