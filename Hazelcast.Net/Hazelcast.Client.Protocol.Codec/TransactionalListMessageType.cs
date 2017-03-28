namespace Hazelcast.Client.Protocol.Codec
{
    internal enum TransactionalListMessageType
    {
        TransactionalListAdd = 0x1301,
        TransactionalListRemove = 0x1302,
        TransactionalListSize = 0x1303
    }
}