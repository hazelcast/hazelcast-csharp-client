namespace Hazelcast.Core
{
    /// <summary>
    ///     Message listener for
    ///     <see cref="ITopic{E}">ITopic&lt;E&gt;</see>
    ///     .
    /// </summary>
    /// <?></?>
    public interface IMessageListener<E> : IEventListener
    {
        /// <summary>Invoked when a message is received for the added topic.</summary>
        /// <remarks>
        ///     Invoked when a message is received for the added topic. Note that topic guarantees message ordering.
        ///     Therefore there is only one thread invoking onMessage. The user shouldn't keep the thread busy and preferably
        ///     dispatch it via an Executor. This will increase the performance of the topic.
        /// </remarks>
        /// <param name="message">received message</param>
        void OnMessage(Message<E> message);
    }
}