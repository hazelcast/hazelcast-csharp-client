using System;

namespace Hazelcast.Core
{
    [Serializable]
    public class HazelcastException : SystemException
    {
        public HazelcastException()
        {
        }

        public HazelcastException(string message) : base(message)
        {
        }

        public HazelcastException(string message, Exception cause) : base(message, cause)
        {
        }

        public HazelcastException(Exception cause) : base(cause.Message)
        {
        }
    }
}