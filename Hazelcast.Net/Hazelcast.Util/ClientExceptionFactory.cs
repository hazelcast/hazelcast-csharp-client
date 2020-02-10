// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Client;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Spi;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.IO.Serialization;
using Hazelcast.Transaction;
using errorCodes = Hazelcast.Client.Protocol.ClientProtocolErrorCodes;

namespace Hazelcast.Util
{
    internal class ClientExceptionFactory
    {
        private readonly IDictionary<int, ExceptionFactoryDelegate> _errorCodeToException =
            new Dictionary<int, ExceptionFactoryDelegate>
            {
                {errorCodes.ArrayIndexOutOfBounds, (m, c) => new IndexOutOfRangeException(m, c)},
                {errorCodes.ArrayStore, (m, c) => new ArrayTypeMismatchException(m, c)},
                {errorCodes.Authentication, (m, c) => new AuthenticationException(m)},
                {errorCodes.CallerNotMember, (m, c) => new CallerNotMemberException(m)},
                {errorCodes.Cancellation, (m, c) => new OperationCanceledException(m)},
                {errorCodes.ClassCast, (m, c) => new InvalidCastException(m)},
                {errorCodes.ClassNotFound, (m, c) => new TypeLoadException(m)},
                {errorCodes.ConcurrentModification, (m, c) => new InvalidOperationException(m)},
                {errorCodes.ConfigMismatch, (m, c) => new ConfigMismatchException(m)},
                {errorCodes.DistributedObjectDestroyed, (m, c) => new DistributedObjectDestroyedException(m)},
                {errorCodes.Eof, (m, c) => new EndOfStreamException(m)},
                {errorCodes.EntryProcessor, (m, c) => new NotSupportedException(m)},
                {errorCodes.Execution, (m, c) => new AggregateException(m)},
                {errorCodes.Hazelcast, (m, c) => new HazelcastException(m)},
                {errorCodes.HazelcastInstanceNotActive, (m, c) => new HazelcastInstanceNotActiveException(m)},
                {errorCodes.HazelcastOverload, (m, c) => new HazelcastOverloadException(m)},
                {errorCodes.HazelcastSerialization, (m, c) => new HazelcastSerializationException(m)},
                {errorCodes.IO, (m, c) => new IOException(m)},
                {errorCodes.IllegalArgument, (m, c) => new ArgumentException(m)},
                {errorCodes.IllegalAccessException, (m, c) => new MemberAccessException(m)},
                {errorCodes.IllegalAccessError, (m, c) => new AccessViolationException(m)},
                {errorCodes.IllegalMonitorState, (m, c) => new SynchronizationLockException(m)},
                {errorCodes.IllegalState, (m, c) => new InvalidOperationException(m)},
                {errorCodes.IllegalThreadState, (m, c) => new ThreadStateException(m)},
                {errorCodes.IndexOutOfBounds, (m, c) => new IndexOutOfRangeException(m)},
                {errorCodes.Interrupted, (m, c) => new ThreadInterruptedException(m)},
                {errorCodes.InvalidAddress, (m, c) => new AddressUtil.InvalidAddressException(m)},
                {errorCodes.InvalidConfiguration, (m, c) => new InvalidConfigurationException(m)},
                {errorCodes.MemberLeft, (m, c) => new MemberLeftException(m)},
                {errorCodes.NegativeArraySize, (m, c) => new ArgumentOutOfRangeException(m)},
                {errorCodes.NoSuchElement, (m, c) => new InvalidOperationException(m)},
                {errorCodes.NotSerializable, (m, c) => new SerializationException(m)},
                {errorCodes.NullPointer, (m, c) => new NullReferenceException(m)},
                {errorCodes.OperationTimeout, (m, c) => new TimeoutException(m)},
                {errorCodes.PartitionMigrating, (m, c) => new PartitionMigratingException(m)},
                {errorCodes.Query, (m, c) => new QueryException(m)},
                {errorCodes.QueryResultSizeExceeded, (m, c) => new QueryResultSizeExceededException(m)},
                {errorCodes.SplitBrainProtection, (m, c) => new SplitBrainProtectionException(m)},
                {errorCodes.ReachedMaxSize, (m, c) => new ReachedMaxSizeException(m)},
                {errorCodes.RejectedExecution, (m, c) => new TaskSchedulerException(m)},
                {errorCodes.ResponseAlreadySent, (m, c) => new ResponseAlreadySentException(m)},
                {errorCodes.RetryableHazelcast, (m, c) => new RetryableHazelcastException(m)},
                {errorCodes.RetryableIO, (m, c) => new RetryableIOException(m)},
                {errorCodes.Runtime, (m, c) => new Exception(m, c)},
                {errorCodes.Security, (m, c) => new SecurityException(m, c)},
                {errorCodes.Socket, (m, c) => new IOException(m, c)},
                {errorCodes.StaleSequence, (m, c) => new StaleSequenceException(m)},
                {errorCodes.TargetDisconnected, (m, c) => new TargetDisconnectedException(m)},
                {errorCodes.TargetNotMember, (m, c) => new TargetNotMemberException(m)},
                {errorCodes.Timeout, (m, c) => new TimeoutException(m)},
                {errorCodes.TopicOverload, (m, c) => new TopicOverloadException(m)},
                {errorCodes.Transaction, (m, c) => new TransactionException(m)},
                {errorCodes.TransactionNotActive, (m, c) => new TransactionNotActiveException(m)},
                {errorCodes.TransactionTimedOut, (m, c) => new TransactionTimedOutException(m)},
                {errorCodes.UriSyntax, (m, c) => new UriFormatException(m)},
                {errorCodes.UTFDataFormat, (m, c) => new ArgumentException(m, c)},
                {errorCodes.UnsupportedOperation, (m, c) => new NotSupportedException(m)},
                {errorCodes.WrongTarget, (m, c) => new WrongTargetException(m)},
                {errorCodes.AccessControl, (m, c) => new SecurityException(m)},
                {errorCodes.Login, (m, c) => new AuthenticationException(m)},
                {errorCodes.UnsupportedCallback, (m, c) => new NotSupportedException(m)},
                {errorCodes.NoDataMember, (m, c) => new NoDataMemberInClusterException(m)},
                {errorCodes.ReplicatedMapCantBeCreated, (m, c) => new ReplicatedMapCantBeCreatedOnLiteMemberException(m)},
                {errorCodes.MaxMessageSizeExceeded, (m, c) => new MaxMessageSizeExceeded(m)},
                {errorCodes.WanReplicationQueueFull, (m, c) => new WanQueueFullException(m)},
                {errorCodes.AssertionError, (m, c) => new AssertException(m)},
                {errorCodes.OutOfMemoryError, (m, c) => new OutOfMemoryException(m)},
                {errorCodes.StackOverflowError, (m, c) => new StackOverflowException(m)},
                {errorCodes.NativeOutOfMemoryError, (m, c) => new NativeOutOfMemoryException(m)},
                {errorCodes.ServiceNotFound, (m, c) => new ServiceNotFoundException(m)},
                {errorCodes.StaleTaskId, (m, c) => new StaleTaskIdException(m)},
                {errorCodes.DuplicateTask, (m, c) => new DuplicateTaskException(m)},
                {errorCodes.StaleTask, (m, c) => new StaleTaskException(m)},
                {errorCodes.LocalMemberReset, (m, c) => new LocalMemberResetException(m)},
                {errorCodes.IndeterminateOperationState, (m, c) => new IndeterminateOperationStateException(m)},
                {errorCodes.FlakeIdNodeIdOutOfRangeException, (m, c) => new NodeIdOutOfRangeException(m)},
                {errorCodes.TargetNotReplicaException, (m, c) => new TargetNotReplicaException(m)},
                {errorCodes.MutationDisallowedException, (m, c) => new MutationDisallowedException(m)},
                {errorCodes.ConsistencyLostException, (m, c) => new ConsistencyLostException(m)},
                {errorCodes.SessionExpiredException, (m, c) => new SessionExpiredException(m)},
                {errorCodes.WaitKeyCancelledException, (m, c) => new WaitKeyCancelledException(m)},
                {errorCodes.LockAcquireLimitReachedException, (m, c) => new LockAcquireLimitReachedException(m)},
                {errorCodes.LockOwnershipLostException, (m, c) => new LockOwnershipLostException(m)},
                {errorCodes.CpGroupDestroyedException, (m, c) => new CPGroupDestroyedException(m)},
                {errorCodes.CannotReplicateException, (m, c) => new CannotReplicateException(m)},
                {errorCodes.LeaderDemotedException, (m, c) => new LeaderDemotedException(m)},
                {errorCodes.StaleAppendRequestException, (m, c) => new StaleAppendRequestException(m)},
                {errorCodes.NotLeaderException, (m, c) => new NotLeaderException(m)},
                {errorCodes.VersionMismatchException, (m, c) => new VersionMismatchException(m)}
            };

        internal Exception CreateException(IEnumerator<ErrorHolder> errorHolders)
        {
            if (!errorHolders.MoveNext())
            {
                return null;
            }
            var errorHolder = errorHolders.Current;
            var cause = CreateException(errorHolders);
            var exception = _errorCodeToException.TryGetValue(errorHolder.ErrorCode, out var exceptionFactory)
                ? exceptionFactory.Invoke(errorHolder.Message, cause)
                : new UndefinedErrorCodeException(errorHolder.Message, cause);
            
            var sb = new StringBuilder();
            foreach (var stackTraceElement in errorHolder.StackTraceElements)
            {
                sb.Append("\tat ").AppendLine(stackTraceElement.ToString());
            }

            exception.Data.Add(sb.ToString(), "");
            return exception;
        }

        private delegate Exception ExceptionFactoryDelegate(string message, Exception cause);
    }
}