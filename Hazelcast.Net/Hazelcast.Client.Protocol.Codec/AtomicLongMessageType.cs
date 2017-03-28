namespace Hazelcast.Client.Protocol.Codec
{
    internal enum AtomicLongMessageType
    {
        AtomicLongApply = 0x0a01,
        AtomicLongAlter = 0x0a02,
        AtomicLongAlterAndGet = 0x0a03,
        AtomicLongGetAndAlter = 0x0a04,
        AtomicLongAddAndGet = 0x0a05,
        AtomicLongCompareAndSet = 0x0a06,
        AtomicLongDecrementAndGet = 0x0a07,
        AtomicLongGet = 0x0a08,
        AtomicLongGetAndAdd = 0x0a09,
        AtomicLongGetAndSet = 0x0a0a,
        AtomicLongIncrementAndGet = 0x0a0b,
        AtomicLongGetAndIncrement = 0x0a0c,
        AtomicLongSet = 0x0a0d
    }
}