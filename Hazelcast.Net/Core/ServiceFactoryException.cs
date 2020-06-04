using System;
using System.Runtime.Serialization;
using Hazelcast.Exceptions;

namespace Hazelcast.Core
{
    [Serializable]
    public class ServiceFactoryException : HazelcastException
    {
        // ReSharper disable once InconsistentNaming
        private const string DefaultMessage = "Failed to create an instance.";

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceFactoryException"/> class.
        /// </summary>
        public ServiceFactoryException()
            : base(DefaultMessage)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceFactoryException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ServiceFactoryException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceFactoryException"/> class with a reference to
        /// the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null
        /// reference if no inner exception is specified.</param>
        public ServiceFactoryException(Exception innerException)
            : base(DefaultMessage, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceFactoryException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null
        /// reference if no inner exception is specified.</param>
        public ServiceFactoryException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceFactoryException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data
        /// about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        public ServiceFactoryException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
