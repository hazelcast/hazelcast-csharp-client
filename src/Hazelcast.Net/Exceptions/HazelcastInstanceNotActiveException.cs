using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Hazelcast.Exceptions
{
    [Serializable]
    public class HazelcastInstanceNotActiveException : HazelcastException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastInstanceNotActiveException"/> class.
        /// </summary>
        public HazelcastInstanceNotActiveException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastInstanceNotActiveException"/> class.
        /// </summary>
        /// <param name="message"></param>
        public HazelcastInstanceNotActiveException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastInstanceNotActiveException"/> class.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public HazelcastInstanceNotActiveException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected HazelcastInstanceNotActiveException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
