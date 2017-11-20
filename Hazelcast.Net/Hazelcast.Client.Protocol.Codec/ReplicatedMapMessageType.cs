namespace Hazelcast.Client.Protocol.Codec
{
    internal enum ReplicatedMapMessageType
    {
        ReplicatedMapPut = 0x0e01,
        ReplicatedMapSize = 0x0e02,
        ReplicatedMapIsEmpty = 0x0e03,
        ReplicatedMapContainsKey = 0x0e04,
        ReplicatedMapContainsValue = 0x0e05,
        ReplicatedMapGet = 0x0e06,
        ReplicatedMapRemove = 0x0e07,
        ReplicatedMapPutAll = 0x0e08,
        ReplicatedMapClear = 0x0e09,
        ReplicatedMapAddEntryListenerToKeyWithPredicate = 0x0e0a,
        ReplicatedMapAddEntryListenerWithPredicate = 0x0e0b,
        ReplicatedMapAddEntryListenerToKey = 0x0e0c,
        ReplicatedMapAddEntryListener = 0x0e0d,
        ReplicatedMapRemoveEntryListener = 0x0e0e,
        ReplicatedMapKeySet = 0x0e0f,
        ReplicatedMapValues = 0x0e10,
        ReplicatedMapEntrySet = 0x0e11,
        ReplicatedMapAddNearCacheEntryListener = 0x0e12
    }
}