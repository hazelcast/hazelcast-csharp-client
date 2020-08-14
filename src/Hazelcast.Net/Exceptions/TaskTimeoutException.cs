using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Hazelcast.Core;

namespace Hazelcast.Exceptions
{
    /// <summary>
    /// Represents the exception that is throw when an async task times out.
    /// </summary>
    [Serializable] // CA2237
    public sealed class TaskTimeoutException : HazelcastException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskTimeoutException"/> class with a specified error message and the task that timed out.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="task">The task that timed out.</param>
        public TaskTimeoutException(string message, Task task)
            : base(string.IsNullOrWhiteSpace(message) ? ExceptionMessages.Timeout : message)
        {
            Task = task ?? throw new ArgumentNullException(nameof(task));
        }

        /// <summary>
        /// Gets the task that timed out (and may still be executing).
        /// </summary>
        public Task Task { get; }

        /// <summary>
        /// Observes the exceptions of the task that timed out.
        /// </summary>
        public void ObserveException()
        {
            Task.ObserveException();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskTimeoutException"/>.
        /// </summary>
        /// <remarks>
        /// <para>This constructor method is provided to comply with CA1032 and ensure that the
        /// exception class is a good .NET citizen. It is not meant to be used in code.</para>
        /// </remarks>
        public TaskTimeoutException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskTimeoutException"/> with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <remarks>
        /// <para>This constructor method is provided to comply with CA1032 and ensure that the
        /// exception class is a good .NET citizen. It is not meant to be used in code.</para>
        /// </remarks>
        public TaskTimeoutException(string message) : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskTimeoutException"/> with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null
        /// reference if no inner exception is specified.</param>
        /// <remarks>
        /// <para>This constructor method is provided to comply with CA1032 and ensure that the
        /// exception class is a good .NET citizen. It is not meant to be used in code.</para>
        /// </remarks>
        public TaskTimeoutException(string message, Exception innerException) : base(message, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data
        /// about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        /// <remarks>
        /// <para>This constructor method is provided to comply with CA2229 and ensure that the
        /// exception class is a good .NET citizen. It is not meant to be used in code.</para>
        /// </remarks>
        private TaskTimeoutException(SerializationInfo info, StreamingContext context)
        { }
    }
}
