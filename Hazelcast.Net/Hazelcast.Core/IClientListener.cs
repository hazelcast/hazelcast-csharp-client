namespace Hazelcast.Core
{
    /// <summary>
    ///     IClientListener allows to get notified when a
    ///     <see cref="IClient">IClient</see>
    ///     is connected to
    ///     or disconnected from cluster.
    /// </summary>
    /// <seealso cref="IClient">IClient</seealso>
    /// <seealso cref="IClientService.AddClientListener(IClientListener)">IClientService.AddClientListener(IClientListener)</seealso>
    public interface IClientListener : IEventListener
    {
        /// <summary>Invoked when a new client is connected.</summary>
        /// <remarks>Invoked when a new client is connected.</remarks>
        /// <param name="client">IClient instance</param>
        void ClientConnected(IClient client);

        /// <summary>Invoked when a new client is disconnected.</summary>
        /// <remarks>Invoked when a new client is disconnected.</remarks>
        /// <param name="client">IClient instance</param>
        void ClientDisconnected(IClient client);
    }

    public interface IEventListener
    {
    }
}