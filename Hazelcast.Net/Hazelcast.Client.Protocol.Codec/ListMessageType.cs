namespace Hazelcast.Client.Protocol.Codec
{
    internal enum ListMessageType
    {
        ListSize = 0x0501,
        ListContains = 0x0502,
        ListContainsAll = 0x0503,
        ListAdd = 0x0504,
        ListRemove = 0x0505,
        ListAddAll = 0x0506,
        ListCompareAndRemoveAll = 0x0507,
        ListCompareAndRetainAll = 0x0508,
        ListClear = 0x0509,
        ListGetAll = 0x050a,
        ListAddListener = 0x050b,
        ListRemoveListener = 0x050c,
        ListIsEmpty = 0x050d,
        ListAddAllWithIndex = 0x050e,
        ListGet = 0x050f,
        ListSet = 0x0510,
        ListAddWithIndex = 0x0511,
        ListRemoveWithIndex = 0x0512,
        ListLastIndexOf = 0x0513,
        ListIndexOf = 0x0514,
        ListSub = 0x0515,
        ListIterator = 0x0516,
        ListListIterator = 0x0517
    }
}