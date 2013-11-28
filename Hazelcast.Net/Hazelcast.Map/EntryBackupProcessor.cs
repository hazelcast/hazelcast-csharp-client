using System.Collections.Generic;
using Hazelcast.Map;


namespace Hazelcast.Map
{
	public interface EntryBackupProcessor<K, V>
	{
		void ProcessBackup(KeyValuePair<K, V> entry);
	}
}
