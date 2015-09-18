namespace Hazelcast.Client.Protocol.Codec
{
    internal enum SetMessageType
    {

        SetSize = 0x0601,
        SetContains = 0x0602,
        SetContainsAll = 0x0603,
        SetAdd = 0x0604,
        SetRemove = 0x0605,
        SetAddAll = 0x0606,
        SetCompareAndRemoveAll = 0x0607,
        SetCompareAndRetainAll = 0x0608,
        SetClear = 0x0609,
        SetGetAll = 0x060a,
        SetAddListener = 0x060b,
        SetRemoveListener = 0x060c,
        SetIsEmpty = 0x060d

    }

}


