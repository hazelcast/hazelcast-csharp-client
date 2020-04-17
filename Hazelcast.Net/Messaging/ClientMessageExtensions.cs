namespace Hazelcast.Messaging
{
    /// <summary>
    /// Provides extension methods to the <see cref="ClientMessage"/> class.
    /// </summary>
    public static class ClientMessageExtensions
    {
        /// <summary>
        /// Clones the message with a new correlation identifier.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="correlationId">The new correlation identifier.</param>
        /// <returns>A clone of the original message with a new correlation identifier.</returns>
        /// <remarks>
        /// <para>The first frame of the original message is deep-cloned because it carries the correlation
        /// identifier and therefore is modified. Other frames are shallow-cloned because they are not
        /// modified.</para>
        /// </remarks>
        public static ClientMessage CloneWithNewCorrelationId(this ClientMessage message, long correlationId)
        {
            var clone = new ClientMessage();
            var first = true;

            foreach (var frame in message)
            {
                if (first)
                {
                    // deep-clone the first frame, we're going to modify it with the new correlation id
                    clone.Append(frame.DeepClone());
                    first = false;
                }
                else
                {
                    // shallow-clone the other frames, we're not modifying them
                    clone.Append(frame.ShallowClone());
                }
            }

            clone.CorrelationId = correlationId;

            return clone;
        }
    }
}
