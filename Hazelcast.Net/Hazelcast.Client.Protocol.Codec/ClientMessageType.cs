namespace Hazelcast.Client.Protocol.Codec
{
    internal enum ClientMessageType
    {
        ClientAuthentication = 0x0002,
        ClientAuthenticationCustom = 0x0003,
        ClientAddMembershipListener = 0x0004,
        ClientCreateProxy = 0x0005,
        ClientDestroyProxy = 0x0006,
        ClientGetPartitions = 0x0008,
        ClientRemoveAllListeners = 0x0009,
        ClientAddPartitionLostListener = 0x000a,
        ClientRemovePartitionLostListener = 0x000b,
        ClientGetDistributedObjects = 0x000c,
        ClientAddDistributedObjectListener = 0x000d,
        ClientRemoveDistributedObjectListener = 0x000e,
        ClientPing = 0x000f
    }
}