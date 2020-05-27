using System;
using System.Runtime.Serialization;

namespace Hazelcast.Exceptions
{
    /// <summary>
    /// Represents the exception that is thrown when the Hazelcast client is invoked but is not connected.
    /// </summary>
    [Serializable]
    public class HazelcastClientNotConnectedException : InvalidOperationException
    {
        // ReSharper disable once InconsistentNaming
        private const string DefaultMessage = "Hazelcast client is not connected.";

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastClientNotConnectedException"/> class.
        /// </summary>
        public HazelcastClientNotConnectedException()
            : base(DefaultMessage)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastClientNotConnectedException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public HazelcastClientNotConnectedException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastClientNotConnectedException"/> class with a reference to
        /// the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null
        /// reference if no inner exception is specified.</param>
        public HazelcastClientNotConnectedException(Exception innerException)
            : base(DefaultMessage, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastClientNotConnectedException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null
        /// reference if no inner exception is specified.</param>
        public HazelcastClientNotConnectedException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastClientNotConnectedException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data
        /// about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        public HazelcastClientNotConnectedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
