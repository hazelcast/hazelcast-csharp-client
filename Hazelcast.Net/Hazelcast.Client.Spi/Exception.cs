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

        public TargetNotMemberException(Address target, int partitionId, String operationName, String serviceName)
            : base(
                "Not Member! target:" + target + ", partitionId: " + partitionId + ", operation: " + operationName +
                ", service: " + serviceName)
        {
        }
    }

    internal class TargetDisconnectedException : RetryableHazelcastException
    {
        public TargetDisconnectedException()
        {
        }

        public TargetDisconnectedException(Address address) : base("Target[" + address + "] disconnected.")
        {
        }

        public TargetDisconnectedException(String msg) : base(msg)
        {
        }
    }
}