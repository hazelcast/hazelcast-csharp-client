namespace Hazelcast.Client.Protocol.Codec
{
    internal enum LockMessageType
    {
        LockIsLocked = 0x0701,
        LockIsLockedByCurrentThread = 0x0702,
        LockGetLockCount = 0x0703,
        LockGetRemainingLeaseTime = 0x0704,
        LockLock = 0x0705,
        LockUnlock = 0x0706,
        LockForceUnlock = 0x0707,
        LockTryLock = 0x0708
    }
}