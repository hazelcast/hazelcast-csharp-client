namespace Hazelcast.Client.Protocol.Codec
{
    internal enum TransactionalQueueMessageType
    {
        TransactionalQueueOffer = 0x1401,
        TransactionalQueueTake = 0x1402,
        TransactionalQueuePoll = 0x1403,
        TransactionalQueuePeek = 0x1404,
        TransactionalQueueSize = 0x1405
    }
}