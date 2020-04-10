using System;
using System.Runtime.Serialization;

namespace Hazelcast.Exceptions
{
    /// <summary>
    /// Represents the generic exception that is throw when Hazelcast goes south.
    /// </summary>
    [Serializable]
    public class HazelcastException : Exception
    {
        // ReSharper disable once InconsistentNaming
        private const string DefaultMessage = "Hazelcast gone south.";

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastException"/> class.
        /// </summary>
        public HazelcastException()
            : base(DefaultMessage)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public HazelcastException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastException"/> class with a reference to
        /// the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null
        /// reference if no inner exception is specified.</param>
        public HazelcastException(Exception innerException)
            : base(DefaultMessage, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null
        /// reference if no inner exception is specified.</param>
        public HazelcastException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data
        /// about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        public HazelcastException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
