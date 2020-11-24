namespace Hazelcast.Data
{
    internal enum ProtocolType
    {
        // values MUST match ProtocolType.java

        Member = 0,
        Client = 1,
        Wan = 2,
        Rest = 3,
        MemCache = 4
    }
}
