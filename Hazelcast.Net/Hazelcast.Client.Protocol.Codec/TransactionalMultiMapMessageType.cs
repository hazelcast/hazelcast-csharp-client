namespace Hazelcast.Client.Protocol.Codec
{
    internal enum TransactionalMultiMapMessageType
    {
        TransactionalMultiMapPut = 0x1101,
        TransactionalMultiMapGet = 0x1102,
        TransactionalMultiMapRemove = 0x1103,
        TransactionalMultiMapRemoveEntry = 0x1104,
        TransactionalMultiMapValueCount = 0x1105,
        TransactionalMultiMapSize = 0x1106
    }
}