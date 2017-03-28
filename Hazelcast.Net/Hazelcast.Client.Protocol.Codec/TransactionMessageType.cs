namespace Hazelcast.Client.Protocol.Codec
{
    internal enum TransactionMessageType
    {
        TransactionCommit = 0x1701,
        TransactionCreate = 0x1702,
        TransactionRollback = 0x1703
    }
}