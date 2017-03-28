namespace Hazelcast.Client.Protocol.Codec
{
    internal enum SemaphoreMessageType
    {
        SemaphoreInit = 0x0d01,
        SemaphoreAcquire = 0x0d02,
        SemaphoreAvailablePermits = 0x0d03,
        SemaphoreDrainPermits = 0x0d04,
        SemaphoreReducePermits = 0x0d05,
        SemaphoreRelease = 0x0d06,
        SemaphoreTryAcquire = 0x0d07
    }
}