namespace Hazelcast.Client.Protocol.Codec
{
    internal enum TransactionalMapMessageType
    {
        TransactionalMapContainsKey = 0x1001,
        TransactionalMapGet = 0x1002,
        TransactionalMapGetForUpdate = 0x1003,
        TransactionalMapSize = 0x1004,
        TransactionalMapIsEmpty = 0x1005,
        TransactionalMapPut = 0x1006,
        TransactionalMapSet = 0x1007,
        TransactionalMapPutIfAbsent = 0x1008,
        TransactionalMapReplace = 0x1009,
        TransactionalMapReplaceIfSame = 0x100a,
        TransactionalMapRemove = 0x100b,
        TransactionalMapDelete = 0x100c,
        TransactionalMapRemoveIfSame = 0x100d,
        TransactionalMapKeySet = 0x100e,
        TransactionalMapKeySetWithPredicate = 0x100f,
        TransactionalMapValues = 0x1010,
        TransactionalMapValuesWithPredicate = 0x1011
    }
}