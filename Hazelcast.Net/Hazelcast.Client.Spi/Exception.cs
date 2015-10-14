using System;
using Hazelcast.Core;
using Hazelcast.IO;

namespace Hazelcast.Client.Spi
{
    internal class RetryableHazelcastException : HazelcastException
    {
        public RetryableHazelcastException()
        {
        }

        public RetryableHazelcastException(String message) : base(message)
        {
        }
    }

    internal class TargetNotMemberException : RetryableHazelcastException
    {
        public TargetNotMemberException(String message) : base(message)
        {
        }

    }

    internal class TargetDisconnectedException : RetryableHazelcastException
    {
        public TargetDisconnectedException(Address address) : base("Target[" + address + "] disconnected.")
        {
        }

        public TargetDisconnectedException(String msg) : base(msg)
        {
        }
    }
}