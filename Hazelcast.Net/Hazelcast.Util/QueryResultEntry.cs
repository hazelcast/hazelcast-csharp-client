using Hazelcast.IO.Serialization;
using Hazelcast.Util;


namespace Hazelcast.Util
{
	public interface QueryResultEntry
	{
		Data GetKeyData();

		Data GetValueData();

		Data GetIndexKey();
	}
}
