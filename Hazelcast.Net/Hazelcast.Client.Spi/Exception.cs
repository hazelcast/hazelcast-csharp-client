using System;
using Hazelcast.Core;
using Hazelcast.IO;

namespace Hazelcast.Client.Spi
{
    public class RetryableHazelcastException : HazelcastException
    {
        public RetryableHazelcastException()
        {
        }

        public RetryableHazelcastException(String message) : base(message)
        {
        }
    }

    public class TargetNotMemberException : RetryableHazelcastException
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

    public class TargetDisconnectedException : RetryableHazelcastException
    {
        public TargetDisconnectedException()
        {
        }

        public TargetDisconnectedException(Address address) : base("Target[" + address + "] disconnected.")
        {
        }
    }
}