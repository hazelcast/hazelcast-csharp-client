namespace Hazelcast.Data
{
    public enum ProtocolType
    {
        // order must match ProtocolType.java

        Member = 0,
        Client,
        Wan,
        Rest,
        MemCache
    }
}
