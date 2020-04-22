using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Hazelcast.Exceptions;
using Hazelcast.Protocol.Data;
using Hazelcast.Serialization;
using errorCodes = Hazelcast.Protocol.ClientProtocolErrors;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// 
    /// </summary>
    /// TODO: document, deal with missing exceptions
    internal static class ClientProtocolExceptions
    {
        private static readonly IDictionary<int, ExceptionFactoryDelegate> _errorCodeToException =
            new Dictionary<int, ExceptionFactoryDelegate>
            {
                {errorCodes.ArrayIndexOutOfBounds, (m, c) => new IndexOutOfRangeException(m, c)},
                {errorCodes.ArrayStore, (m, c) => new ArrayTypeMismatchException(m, c)},
                {errorCodes.Authentication, (m, c) => new AuthenticationException(m)},
                {errorCodes.CallerNotMember, (m, c) => new Exception(m)}, //new CallerNotMemberException(m)},
                {errorCodes.Cancellation, (m, c) => new OperationCanceledException(m)},
                {errorCodes.ClassCast, (m, c) => new InvalidCastException(m)},
                {errorCodes.ClassNotFound, (m, c) => new TypeLoadException(m)},
                {errorCodes.ConcurrentModification, (m, c) => new InvalidOperationException(m)},
                {errorCodes.ConfigMismatch, (m, c) => new Exception(m)}, //new ConfigMismatchException(m)},
                {errorCodes.DistributedObjectDestroyed, (m, c) => new Exception(m)}, //new DistributedObjectDestroyedException(m)},
                {errorCodes.Eof, (m, c) => new EndOfStreamException(m)},
                {errorCodes.EntryProcessor, (m, c) => new NotSupportedException(m)},
                {errorCodes.Execution, (m, c) => new AggregateException(m)},
                {errorCodes.Hazelcast, (m, c) => new HazelcastException(m)},
                {errorCodes.HazelcastInstanceNotActive, (m, c) => new Exception(m)}, //new HazelcastInstanceNotActiveException(m)},
                {errorCodes.HazelcastOverload, (m, c) => new Exception(m)}, //new HazelcastOverloadException(m)},
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
                {errorCodes.InvalidAddress, (m, c) => new Exception(m)}, //new AddressUtil.InvalidAddressException(m)},
                {errorCodes.InvalidConfiguration, (m, c) => new Exception(m)}, //new InvalidConfigurationException(m)},
                {errorCodes.MemberLeft, (m, c) => new Exception(m)}, //new MemberLeftException(m)},
                {errorCodes.NegativeArraySize, (m, c) => new ArgumentOutOfRangeException(m)},
                {errorCodes.NoSuchElement, (m, c) => new InvalidOperationException(m)},
                {errorCodes.NotSerializable, (m, c) => new SerializationException(m)},
                {errorCodes.NullPointer, (m, c) => new NullReferenceException(m)},
                {errorCodes.OperationTimeout, (m, c) => new TimeoutException(m)},
                {errorCodes.PartitionMigrating, (m, c) => new Exception(m)}, //new PartitionMigratingException(m)},
                {errorCodes.Query, (m, c) => new Exception(m)}, //new QueryException(m)},
                {errorCodes.QueryResultSizeExceeded, (m, c) => new Exception(m)}, //new QueryResultSizeExceededException(m)},
                {errorCodes.SplitBrainProtection, (m, c) => new Exception(m)}, //new SplitBrainProtectionException(m)},
                {errorCodes.ReachedMaxSize, (m, c) => new Exception(m)}, //new ReachedMaxSizeException(m)},
                {errorCodes.RejectedExecution, (m, c) => new TaskSchedulerException(m)},
                {errorCodes.ResponseAlreadySent, (m, c) => new Exception(m)}, //new ResponseAlreadySentException(m)},
                {errorCodes.RetryableHazelcast, (m, c) => new Exception(m)}, //new RetryableHazelcastException(m)},
                {errorCodes.RetryableIO, (m, c) => new Exception(m)}, //new RetryableIOException(m)},
                {errorCodes.Runtime, (m, c) => new Exception(m, c)},
                {errorCodes.Security, (m, c) => new SecurityException(m, c)},
                {errorCodes.Socket, (m, c) => new IOException(m, c)},
                {errorCodes.StaleSequence, (m, c) => new Exception(m)}, //new StaleSequenceException(m)},
                {errorCodes.TargetDisconnected, (m, c) => new Exception(m)}, //new TargetDisconnectedException(m)},
                {errorCodes.TargetNotMember, (m, c) => new Exception(m)}, //new TargetNotMemberException(m)},
                {errorCodes.Timeout, (m, c) => new TimeoutException(m)},
                {errorCodes.TopicOverload, (m, c) => new Exception(m)}, //new TopicOverloadException(m)},
                {errorCodes.Transaction, (m, c) => new TransactionException(m)},
                {errorCodes.TransactionNotActive, (m, c) => new Exception(m)}, //new TransactionNotActiveException(m)},
                {errorCodes.TransactionTimedOut, (m, c) => new Exception(m)}, //new TransactionTimedOutException(m)},
                {errorCodes.UriSyntax, (m, c) => new UriFormatException(m)},
                {errorCodes.UTFDataFormat, (m, c) => new ArgumentException(m, c)},
                {errorCodes.UnsupportedOperation, (m, c) => new NotSupportedException(m)},
                {errorCodes.WrongTarget, (m, c) => new Exception(m)}, //new WrongTargetException(m)},
                {errorCodes.AccessControl, (m, c) => new SecurityException(m)},
                {errorCodes.Login, (m, c) => new AuthenticationException(m)},
                {errorCodes.UnsupportedCallback, (m, c) => new NotSupportedException(m)},
                {errorCodes.NoDataMember, (m, c) => new Exception(m)}, //new NoDataMemberInClusterException(m)},
                {errorCodes.ReplicatedMapCantBeCreated, (m, c) => new Exception(m)}, //new ReplicatedMapCantBeCreatedOnLiteMemberException(m)},
                {errorCodes.MaxMessageSizeExceeded, (m, c) => new Exception(m)}, //new MaxMessageSizeExceeded(m)},
                {errorCodes.WanReplicationQueueFull, (m, c) => new Exception(m)}, //new WanQueueFullException(m)},
                {errorCodes.AssertionError, (m, c) => new Exception(m)}, //new AssertException(m)},
                {errorCodes.OutOfMemoryError, (m, c) => new OutOfMemoryException(m)},
                {errorCodes.StackOverflowError, (m, c) => new StackOverflowException(m)},
                {errorCodes.NativeOutOfMemoryError, (m, c) => new Exception(m)}, //new NativeOutOfMemoryException(m)},
                {errorCodes.ServiceNotFound, (m, c) => new Exception(m)}, //new ServiceNotFoundException(m)},
                {errorCodes.StaleTaskId, (m, c) => new Exception(m)}, //new StaleTaskIdException(m)},
                {errorCodes.DuplicateTask, (m, c) => new Exception(m)}, //new DuplicateTaskException(m)},
                {errorCodes.StaleTask, (m, c) => new Exception(m)}, //new StaleTaskException(m)},
                {errorCodes.LocalMemberReset, (m, c) => new Exception(m)}, //new LocalMemberResetException(m)},
                {errorCodes.IndeterminateOperationState, (m, c) => new Exception(m)}, //new IndeterminateOperationStateException(m)},
                {errorCodes.FlakeIdNodeIdOutOfRangeException, (m, c) => new Exception(m)}, //new NodeIdOutOfRangeException(m)},
                {errorCodes.TargetNotReplicaException, (m, c) => new Exception(m)}, //new TargetNotReplicaException(m)},
                {errorCodes.MutationDisallowedException, (m, c) => new Exception(m)}, //new MutationDisallowedException(m)},
                {errorCodes.ConsistencyLostException, (m, c) => new Exception(m)}, //new ConsistencyLostException(m)},
                {errorCodes.SessionExpiredException, (m, c) => new Exception(m)}, //new SessionExpiredException(m)},
                {errorCodes.WaitKeyCancelledException, (m, c) => new Exception(m)}, //new WaitKeyCancelledException(m)},
                {errorCodes.LockAcquireLimitReachedException, (m, c) => new Exception(m)}, //new LockAcquireLimitReachedException(m)},
                {errorCodes.LockOwnershipLostException, (m, c) => new Exception(m)}, //new LockOwnershipLostException(m)},
                {errorCodes.CpGroupDestroyedException, (m, c) => new Exception(m)}, //new CpGroupDestroyedException(m)},
                {errorCodes.CannotReplicateException, (m, c) => new Exception(m)}, //new CannotReplicateException(m)},
                {errorCodes.LeaderDemotedException, (m, c) => new Exception(m)}, //new LeaderDemotedException(m)},
                {errorCodes.StaleAppendRequestException, (m, c) => new Exception(m)}, //new StaleAppendRequestException(m)},
                {errorCodes.NotLeaderException, (m, c) => new Exception(m)}, //new NotLeaderException(m)},
                {errorCodes.VersionMismatchException, (m, c) => new Exception(m)}, //new VersionMismatchException(m)}
            };

        internal static Exception CreateException(IEnumerator<ErrorHolder> errorHolders)
        {
            if (!errorHolders.MoveNext())
            {
                return null;
            }
            var errorHolder = errorHolders.Current;
            var cause = CreateException(errorHolders);
            var exception = _errorCodeToException.TryGetValue(errorHolder.ErrorCode, out var exceptionFactory)
                ? exceptionFactory.Invoke(errorHolder.Message, cause)
                : new Exception(errorHolder.Message, cause); //new UndefinedErrorCodeException(errorHolder.Message, cause);

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
