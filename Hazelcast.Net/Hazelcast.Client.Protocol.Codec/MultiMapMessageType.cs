namespace Hazelcast.Client.Protocol.Codec
{
    internal enum MultiMapMessageType
    {

        MultiMapPut = 0x0201,
        MultiMapGet = 0x0202,
        MultiMapRemove = 0x0203,
        MultiMapKeySet = 0x0204,
        MultiMapValues = 0x0205,
        MultiMapEntrySet = 0x0206,
        MultiMapContainsKey = 0x0207,
        MultiMapContainsValue = 0x0208,
        MultiMapContainsEntry = 0x0209,
        MultiMapSize = 0x020a,
        MultiMapClear = 0x020b,
        MultiMapValueCount = 0x020c,
        MultiMapAddEntryListenerToKey = 0x020d,
        MultiMapAddEntryListener = 0x020e,
        MultiMapRemoveEntryListener = 0x020f,
        MultiMapLock = 0x0210,
        MultiMapTryLock = 0x0211,
        MultiMapIsLocked = 0x0212,
        MultiMapUnlock = 0x0213,
        MultiMapForceUnlock = 0x0214,
        MultiMapRemoveEntry = 0x0215,
    }

}


