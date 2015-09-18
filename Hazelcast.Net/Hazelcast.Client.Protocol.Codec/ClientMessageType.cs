namespace Hazelcast.Client.Protocol.Codec
{
    internal enum ClientMessageType
    {

        ClientAuthentication = 0x2,
        ClientAuthenticationCustom = 0x3,
        ClientMembershipListener = 0x4,
        ClientCreateProxy = 0x5,
        ClientDestroyProxy = 0x6,
        ClientGetPartitions = 0x8,
        ClientRemoveAllListeners = 0x9,
        ClientAddPartitionLostListener = 0xa,
        ClientRemovePartitionLostListener = 0xb,
        ClientGetDistributedObject = 0xc,
        ClientAddDistributedObjectListener = 0xd,
        ClientRemoveDistributedObjectListener = 0xe,
        ClientPing = 0xf
    }

}


