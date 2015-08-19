namespace Hazelcast.Client.Protocol.Codec
{
    internal enum CountDownLatchMessageType
    {

        CountDownLatchAwait = 0x0c01,
        CountDownLatchCountDown = 0x0c02,
        CountDownLatchGetCount = 0x0c03,
        CountDownLatchTrySetCount = 0x0c04

    }

}


