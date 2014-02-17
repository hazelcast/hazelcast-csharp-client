using System;
using Hazelcast.Core;

namespace Hazelcast.Client
{
    [Serializable]
    internal class AuthenticationException : HazelcastException
    {
        public AuthenticationException() : base("Wrong group name and password.")
        {
        }

        public AuthenticationException(string message) : base(message)
        {
        }
    }
}