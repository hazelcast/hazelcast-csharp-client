namespace Hazelcast.Client.Protocol.Codec
{
    internal enum QueueMessageType
    {
        QueueOffer = 0x0301,
        QueuePut = 0x0302,
        QueueSize = 0x0303,
        QueueRemove = 0x0304,
        QueuePoll = 0x0305,
        QueueTake = 0x0306,
        QueuePeek = 0x0307,
        QueueIterator = 0x0308,
        QueueDrainTo = 0x0309,
        QueueDrainToMaxSize = 0x030a,
        QueueContains = 0x030b,
        QueueContainsAll = 0x030c,
        QueueCompareAndRemoveAll = 0x030d,
        QueueCompareAndRetainAll = 0x030e,
        QueueClear = 0x030f,
        QueueAddAll = 0x0310,
        QueueAddListener = 0x0311,
        QueueRemoveListener = 0x0312,
        QueueRemainingCapacity = 0x0313,
        QueueIsEmpty = 0x0314
    }
}