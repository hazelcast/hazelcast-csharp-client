namespace Hazelcast.Client.Connection
{
    internal interface IConnectionListener
    {
        void ConnectionAdded(ClientConnection connection);
        void ConnectionRemoved(ClientConnection connection);
    }
}