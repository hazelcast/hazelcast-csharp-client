namespace Hazelcast.Client.Protocol.Codec
{
    internal enum RingbufferMessageType
    {
        RingbufferSize = 0x1901,
        RingbufferTailSequence = 0x1902,
        RingbufferHeadSequence = 0x1903,
        RingbufferCapacity = 0x1904,
        RingbufferRemainingCapacity = 0x1905,
        RingbufferAdd = 0x1906,
        RingbufferReadOne = 0x1908,
        RingbufferAddAll = 0x1909,
        RingbufferReadMany = 0x190a
    }
}