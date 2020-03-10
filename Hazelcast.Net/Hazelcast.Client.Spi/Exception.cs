// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.IO;
using System.Threading;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Transaction;

namespace Hazelcast.Client.Spi
{
    /// <summary>
    /// A <see cref="HazelcastException"/> that is thrown when client can not use this cluster.
    /// </summary>
    public class ClientNotAllowedInClusterException : HazelcastException
    {
        public ClientNotAllowedInClusterException()
        {
        }

        public ClientNotAllowedInClusterException(string message) : base(message)
        {
        }

        public ClientNotAllowedInClusterException(string message, Exception cause) : base(message, cause)
        {
        }
    }
    /// <summary>
    ///  Marker interface for exceptions to indicate that an operation can be retried. E.g. a map.get send to a machine
    ///  where the partition has just moved to another machine.
    /// </summary>
    internal interface IRetryableException
    {
    }

    /// <summary>
    /// Marker interface for exceptions to indicate that an operation can be retried. 
    /// E.g. a map.get sent to a machine where the partition has just moved to another machine.
    /// </summary>
    [Serializable]
    public class RetryableHazelcastException : HazelcastException, IRetryableException
    {
        /// <inheritdoc />
        public RetryableHazelcastException()
        {
        }

        /// <inheritdoc />
        public RetryableHazelcastException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public RetryableHazelcastException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// A <see cref="RetryableHazelcastException"/> that indicates operation is sent to a machine that isn't member of the cluster
    /// </summary>
    [Serializable]
    public class TargetNotMemberException : RetryableHazelcastException
    {
        /// <inheritdoc />
        public TargetNotMemberException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Thrown when a client invocation is failed because related target is disconnected, and
    /// whether the invocation runs or not is indeterminate
    /// </summary>
    [Serializable]
    public class TargetDisconnectedException : HazelcastException
    {
        /// <summary>
        /// Constructor version with an Address instance to assign to
        /// </summary>
        /// <param name="address">is an Adress instance to assign to</param>
        public TargetDisconnectedException(Address address) : base("Target[" + address + "] disconnected.")
        {
        }

        /// <summary>
        /// Constructor version with an Address instance and message to assign to
        /// </summary>
        /// <param name="address">is an Adress instance to assign to</param>
        /// <param name="message">is a message to assign to</param>
        public TargetDisconnectedException(Address address, string message) : base(
            "Target[" + address + "] disconnected, " + message)
        {
        }

        /// <inheritdoc />
        public TargetDisconnectedException(string msg) : base(msg)
        {
        }

        public TargetDisconnectedException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>Thrown when IHazelcastInstance is not active during an invocation.</summary>
    [Serializable]
    public class HazelcastInstanceNotActiveException : InvalidOperationException
    {
        public HazelcastInstanceNotActiveException() : base("Hazelcast instance is not active!")
        {
        }

        public HazelcastInstanceNotActiveException(string message) : base(message)
        {
        }

        public HazelcastInstanceNotActiveException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>Thrown when Hazelcast client is not active during an invocation.</summary>
    [Serializable]
    public class HazelcastClientNotActiveException : InvalidOperationException
    {
        public HazelcastClientNotActiveException() : base("Client is not active!")
        {
        }

        public HazelcastClientNotActiveException(string message) : base(message)
        {
        }

        public HazelcastClientNotActiveException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// A <see cref="RetryableHazelcastException"/> that indicates that an operation was sent by a machine which isn't member in the cluster
    /// when the operation is executed.
    /// </summary>
    [Serializable]
    public class CallerNotMemberException : RetryableHazelcastException
    {
        public CallerNotMemberException()
        {
        }

        public CallerNotMemberException(string message) : base(message)
        {
        }

        public CallerNotMemberException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// This exception is thrown when an exception that is coming from server is not recognized by the protocol.
    /// The original exception is included in the exception
    /// </summary>
    public class UndefinedErrorCodeException : HazelcastException
    {
        public UndefinedErrorCodeException()
        {
        }

        public UndefinedErrorCodeException(string message) : base(message)
        {
        }

        public UndefinedErrorCodeException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// Indicates that the version of a joining member is not compatible with the cluster version.
    /// </summary>
    internal class VersionMismatchException : HazelcastException
    {
        public VersionMismatchException()
        {
        }

        public VersionMismatchException(string message) : base(message)
        {
        }

        public VersionMismatchException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// A <see cref="CPSubsystemException"/> which is thrown when a leader-only request is received by a non-leader member.
    /// </summary>
    internal class NotLeaderException : CPSubsystemException
    {
        public NotLeaderException(string message) : base(message)
        {
        }

        public NotLeaderException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// Base exception for failures in CP Subsystem
    /// </summary>
    internal class CPSubsystemException : HazelcastException
    {
        public CPSubsystemException()
        {
        }

        public CPSubsystemException(string message) : base(message)
        {
        }

        public CPSubsystemException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// A <see cref="CPSubsystemException"/> which is thrown when a Raft leader node
    /// appends an entry to its local Raft log, but demotes to the follower role
    /// before learning the commit status of the entry. In this case, this node
    /// cannot decide if the operation is committed or not.
    /// </summary>
    internal class StaleAppendRequestException : CPSubsystemException
    {
        public StaleAppendRequestException()
        {
        }

        public StaleAppendRequestException(string message) : base(message)
        {
        }

        public StaleAppendRequestException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// A <see cref="CPSubsystemException"/> which is thrown when  an appended but not-committed entry is truncated by the new leader.
    /// </summary>
    internal class LeaderDemotedException : CPSubsystemException
    {
        public LeaderDemotedException()
        {
        }

        public LeaderDemotedException(string message) : base(message)
        {
        }

        public LeaderDemotedException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// A <see cref="CPSubsystemException"/> which is thrown when an entry cannot be replicated
    /// </summary>
    internal class CannotReplicateException : CPSubsystemException, IRetryableException
    {
        public CannotReplicateException()
        {
        }

        public CannotReplicateException(string message) : base(message)
        {
        }

        public CannotReplicateException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// A <see cref="CPSubsystemException"/> which is thrown when a request is sent to a destroyed CP group.
    /// </summary>
    internal class CPGroupDestroyedException : CPSubsystemException
    {
        public CPGroupDestroyedException()
        {
        }

        public CPGroupDestroyedException(string message) : base(message)
        {
        }

        public CPGroupDestroyedException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// Thrown when an endpoint (either a Hazelcast server or a client) interacts with a FencedLock instance after its CP session
    /// is closed in the underlying CP group and its lock ownership is cancelled.
    /// </summary>
    internal class LockOwnershipLostException : SynchronizationLockException
    {
        public LockOwnershipLostException()
        {
        }

        public LockOwnershipLostException(string message) : base(message)
        {
        }

        public LockOwnershipLostException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Thrown when the current lock holder could not acquired the lock reentrantly because the configured lock acquire limit is reached.
    /// </summary>
    internal class LockAcquireLimitReachedException : HazelcastException
    {
        public LockAcquireLimitReachedException()
        {
        }

        public LockAcquireLimitReachedException(string message) : base(message)
        {
        }

        public LockAcquireLimitReachedException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// Thrown when a wait key is cancelled and means that the corresponding operation has not succeeded
    /// </summary>
    internal class WaitKeyCancelledException : HazelcastException
    {
        public WaitKeyCancelledException()
        {
        }

        public WaitKeyCancelledException(string message) : base(message)
        {
        }

        public WaitKeyCancelledException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// Thrown when an operation is attached to a Raft session is no longer active
    /// </summary>
    internal class SessionExpiredException : HazelcastException
    {
        public SessionExpiredException()
        {
        }

        public SessionExpiredException(string message) : base(message)
        {
        }

        public SessionExpiredException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// Exception that indicates that the state found on this replica disallows mutation. 
    /// </summary>
    internal class MutationDisallowedException : HazelcastException
    {
        public MutationDisallowedException()
        {
        }

        public MutationDisallowedException(string message) : base(message)
        {
        }

        public MutationDisallowedException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// Exception that indicates that the receiver of a CRDT operation is not a CRDT replica.
    /// </summary>
    internal class TargetNotReplicaException : RetryableHazelcastException
    {
        public TargetNotReplicaException()
        {
        }

        public TargetNotReplicaException(string message) : base(message)
        {
        }

        public TargetNotReplicaException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// Exception thrown from member if that member is not able to generate IDs using Flake ID generator because its node ID is too big.
    /// </summary>
    internal class NodeIdOutOfRangeException : HazelcastException
    {
        public NodeIdOutOfRangeException()
        {
        }

        public NodeIdOutOfRangeException(string message) : base(message)
        {
        }

        public NodeIdOutOfRangeException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// IndeterminateOperationStateException is thrown when result of an invocation becomes indecisive.
    /// </summary>
    internal class IndeterminateOperationStateException : HazelcastException
    {
        public IndeterminateOperationStateException()
        {
        }

        public IndeterminateOperationStateException(string message) : base(message)
        {
        }

        public IndeterminateOperationStateException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// An exception provided to <see cref="MemberLeftException"/> as a cause when the local member is resetting itself
    /// </summary>
    internal class LocalMemberResetException : HazelcastException
    {
        public LocalMemberResetException()
        {
        }

        public LocalMemberResetException(string message) : base(message)
        {
        }

        public LocalMemberResetException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// Exception thrown during any operation on a stale (=previously destroyed) task.
    /// </summary>
    internal class StaleTaskException : HazelcastException
    {
        public StaleTaskException()
        {
        }

        public StaleTaskException(string message) : base(message)
        {
        }

        public StaleTaskException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// An exception thrown when a task's name is already used before for another (or the same, if re-attempted) schedule. 
    /// </summary>
    internal class DuplicateTaskException : HazelcastException
    {
        public DuplicateTaskException()
        {
        }

        public DuplicateTaskException(string message) : base(message)
        {
        }

        public DuplicateTaskException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// thrown when retrieving the result of a task  if the result of the task is overwritten.
    /// This means the task is executed but the result isn't available anymore
    /// </summary>
    internal class StaleTaskIdException : HazelcastException
    {
        public StaleTaskIdException()
        {
        }

        public StaleTaskIdException(string message) : base(message)
        {
        }

        public StaleTaskIdException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// indicates that a requested service is not exist.
    /// </summary>
    internal class ServiceNotFoundException : HazelcastException
    {
        public ServiceNotFoundException()
        {
        }

        public ServiceNotFoundException(string message) : base(message)
        {
        }

        public ServiceNotFoundException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// Thrown when Hazelcast cannot allocate required native memory.
    /// </summary>
    internal class NativeOutOfMemoryException : Exception
    {
        public NativeOutOfMemoryException()
        {
        }

        public NativeOutOfMemoryException(string message) : base(message)
        {
        }

        public NativeOutOfMemoryException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Thrown to indicate that an assertion has failed.
    /// </summary>
    internal class AssertException : Exception
    {
        public AssertException()
        {
        }

        public AssertException(string message) : base(message)
        {
        }

        public AssertException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// thrown when the wan replication queues are full
    /// </summary>
    internal class WanQueueFullException : HazelcastException
    {
        public WanQueueFullException()
        {
        }

        public WanQueueFullException(string message) : base(message)
        {
        }

        public WanQueueFullException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// Thrown when client message size exceeds <see cref="int.MaxValue"/>
    /// </summary>
    internal class MaxMessageSizeExceeded : HazelcastException
    {
        public MaxMessageSizeExceeded()
        {
        }

        public MaxMessageSizeExceeded(string message) : base(message)
        {
        }

        public MaxMessageSizeExceeded(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// Thrown when <see cref="IHazelcastInstance.GetReplicatedMap{TKey,TValue}"/> is invoked on a lite member.
    /// </summary>
    internal class ReplicatedMapCantBeCreatedOnLiteMemberException : HazelcastException
    {
        public ReplicatedMapCantBeCreatedOnLiteMemberException()
        {
        }

        public ReplicatedMapCantBeCreatedOnLiteMemberException(string message) : base(message)
        {
        }

        public ReplicatedMapCantBeCreatedOnLiteMemberException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// A <see cref="RetryableHazelcastException"/> indicating that an operation is executed on a wrong member.
    /// </summary>
    internal class WrongTargetException : RetryableHazelcastException
    {
        public WrongTargetException()
        {
        }

        public WrongTargetException(string message) : base(message)
        {
        }

        public WrongTargetException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// Thrown when a publisher wants to write to a topic, but there is not sufficient storage to deal with the event.
    /// </summary>
    internal class TopicOverloadException : HazelcastException
    {
        public TopicOverloadException()
        {
        }

        public TopicOverloadException(string message) : base(message)
        {
        }

        public TopicOverloadException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// indicating that there was a IO failure, but it can be retried.
    /// </summary>
    internal class RetryableIOException : IOException, IRetryableException
    {
        public RetryableIOException()
        {
        }

        public RetryableIOException(string message) : base(message)
        {
        }

        public RetryableIOException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// A HazelcastException indicating that there is some kind of system error causing a response to be send multiple times for some operation.
    /// </summary>
    internal class ResponseAlreadySentException : HazelcastException
    {
        public ResponseAlreadySentException()
        {
        }

        public ResponseAlreadySentException(string message) : base(message)
        {
        }

        public ResponseAlreadySentException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// Exception thrown when a write-behind MapStore rejects to accept a new element.
    /// </summary>
    internal class ReachedMaxSizeException : Exception
    {
        public ReachedMaxSizeException()
        {
        }

        public ReachedMaxSizeException(string message) : base(message)
        {
        }

        public ReachedMaxSizeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// An exception thrown when the cluster size is below the defined threshold.
    /// </summary>
    internal class SplitBrainProtectionException : TransactionException
    {
        public SplitBrainProtectionException()
        {
        }

        public SplitBrainProtectionException(string message) : base(message)
        {
        }

        public SplitBrainProtectionException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// This exception is thrown when a query exceeds a configurable result size limit.
    /// </summary>
    internal class QueryResultSizeExceededException : HazelcastException
    {
        public QueryResultSizeExceededException()
        {
        }

        public QueryResultSizeExceededException(string message) : base(message)
        {
        }

        public QueryResultSizeExceededException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// A <see cref="RetryableHazelcastException"/> that is thrown when an operation is executed on a partition, but that
    /// partition is currently being moved around.
    /// </summary>
    internal class PartitionMigratingException : RetryableHazelcastException
    {
        public PartitionMigratingException()
        {
        }

        public PartitionMigratingException(string message) : base(message)
        {
        }

        public PartitionMigratingException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// A {@link ExecutionException} thrown when a member left during an invocation or execution.
    /// </summary>
    internal class MemberLeftException : AggregateException, IRetryableException
    {
        public MemberLeftException()
        {
        }

        public MemberLeftException(string message) : base(message)
        {
        }

        public MemberLeftException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// A <see cref="RetryableHazelcastException"/> that is thrown when the system won't handle more load due to an overload.
    /// </summary>
    internal class HazelcastOverloadException : HazelcastException
    {
        public HazelcastOverloadException()
        {
        }

        public HazelcastOverloadException(string message) : base(message)
        {
        }

        public HazelcastOverloadException(string message, Exception cause) : base(message, cause)
        {
        }
    }

    /// <summary>
    /// Exception thrown when 2 nodes want to join, but there configuration doesn't match
    /// </summary>
    internal class ConfigMismatchException : HazelcastException
    {
        public ConfigMismatchException()
        {
        }

        public ConfigMismatchException(string message) : base(message)
        {
        }

        public ConfigMismatchException(string message, Exception cause) : base(message, cause)
        {
        }
    }
}