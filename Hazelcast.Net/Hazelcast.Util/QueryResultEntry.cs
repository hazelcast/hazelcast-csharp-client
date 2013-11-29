using Hazelcast.IO.Serialization;

namespace Hazelcast.Util
{
    public interface QueryResultEntry
    {
        Data GetKeyData();

        Data GetValueData();

        Data GetIndexKey();
    }
}