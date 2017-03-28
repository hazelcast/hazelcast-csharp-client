namespace Hazelcast.Client.Protocol.Codec
{
    internal enum TransactionalSetMessageType
    {
        TransactionalSetAdd = 0x1201,
        TransactionalSetRemove = 0x1202,
        TransactionalSetSize = 0x1203
    }
}