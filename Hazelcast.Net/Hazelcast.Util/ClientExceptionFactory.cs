using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;
using Hazelcast.Client;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.Transaction;

namespace Hazelcast.Util
{
    internal class ClientExceptionFactory
    {
        private delegate Exception ExceptionFactoryDelegate(string message, Exception cause);

        private readonly IDictionary<ClientProtocolErrorCodes, ExceptionFactoryDelegate> errorCodeToException =
            new Dictionary<ClientProtocolErrorCodes, ExceptionFactoryDelegate>
            {
                {ClientProtocolErrorCodes.ArrayIndexOutOfBounds, (m, c) => new IndexOutOfRangeException(m)},
                {ClientProtocolErrorCodes.ArrayStore, (m, c) => new ArrayTypeMismatchException(m)},
                {ClientProtocolErrorCodes.Authentication, (m, c) => new AuthenticationException(m)},
                {ClientProtocolErrorCodes.Cancellation, (m, c) => new OperationCanceledException(m)},
                {ClientProtocolErrorCodes.ClassCast, (m, c) => new InvalidCastException(m)},
                {ClientProtocolErrorCodes.ClassNotFound, (m, c) => new TypeLoadException(m, c)},
                {ClientProtocolErrorCodes.ConcurrentModification, (m, c) => new InvalidOperationException(m)},
                {ClientProtocolErrorCodes.Configuration, (m, c) => new ConfigurationException(m)},
                {ClientProtocolErrorCodes.DistributedObjectDestroyed, (m, c) => new DistributedObjectDestroyedException(m)},
                {ClientProtocolErrorCodes.Eof, (m, c) => new IOException(m)},
                {ClientProtocolErrorCodes.Hazelcast, (m, c) => new HazelcastException(m, c)},
                {ClientProtocolErrorCodes.HazelcastInstanceNotActive, (m, c) => new HazelcastInstanceNotActiveException()},
                {ClientProtocolErrorCodes.HazelcastOverload, (m, c) => new HazelcastException(m)},
                {ClientProtocolErrorCodes.HazelcastSerialization, (m, c) => new HazelcastException(m, c)},
                {ClientProtocolErrorCodes.Io, (m, c) => new IOException(m, c)},
                {ClientProtocolErrorCodes.IllegalArgument, (m, c) => new ArgumentException(m, c)},
                {ClientProtocolErrorCodes.IllegalMonitorState, (m, c) => new SynchronizationLockException(m)},
                {ClientProtocolErrorCodes.IllegalState, (m, c) => new SynchronizationLockException(m)},
                {ClientProtocolErrorCodes.IllegalThreadState, (m, c) => new ThreadStateException(m)},
                {ClientProtocolErrorCodes.Interrupted, (m, c) => new ThreadInterruptedException(m)},
                {ClientProtocolErrorCodes.InvalidAddress, (m, c) => new AddressUtil.InvalidAddressException(m)},
                {ClientProtocolErrorCodes.NegativeArraySize, (m, c) => new ArgumentOutOfRangeException(m)},
                {ClientProtocolErrorCodes.NoSuchElement, (m, c) => new ArgumentException(m)},
                {ClientProtocolErrorCodes.NotSerializable, (m, c) => new SerializationException(m)},
                {ClientProtocolErrorCodes.NullPointer, (m, c) => new NullReferenceException(m)},
                {ClientProtocolErrorCodes.OperationTimeout, (m, c) => new TimeoutException(m)},
                {ClientProtocolErrorCodes.Query, (m, c) => new QueryException(m, c)},
                {ClientProtocolErrorCodes.QueryResultSizeExceeded, (m, c) => new SynchronizationLockException(m)},
                {ClientProtocolErrorCodes.Security, (m, c) => new SecurityException(m)},
                {ClientProtocolErrorCodes.Socket, (m, c) => new IOException(m)},
                {ClientProtocolErrorCodes.TargetDisconnected, (m, c) => new TargetDisconnectedException(m)},
                {ClientProtocolErrorCodes.TargetNotMember, (m, c) => new SynchronizationLockException(m)},
                {ClientProtocolErrorCodes.Timeout, (m, c) => new TimeoutException(m)},
                {ClientProtocolErrorCodes.Transaction, (m, c) => new TransactionException(m)},
                {ClientProtocolErrorCodes.TransactionNotActive, (m, c) => new TransactionNotActiveException(m)},
                {ClientProtocolErrorCodes.TransactionTimedOut, (m, c) => new TransactionTimedOutException(m)},
                {ClientProtocolErrorCodes.UriSyntax, (m, c) => new UriFormatException(m)},
                {ClientProtocolErrorCodes.UtfDataFormat, (m, c) => new InvalidDataException(m)},
                {ClientProtocolErrorCodes.UnsupportedOperation, (m, c) => new NotSupportedException(m)}
            };

        public Exception CreateException(Error error)
        {
            return CreateException(error.ErrorCode, error.ClassName, error.Message, error.StackTrace,
                error.CauseErrorCode, error.CauseClassName);
        }

        public Exception CreateException(int errorCode, string className, string message, StackTraceElement[] stackTrace
            ,int? causeErrorCode, string causeClassName)
        {
            Exception cause = null;
            if (causeClassName != null && causeErrorCode.HasValue)
            {
                cause = CreateException(causeErrorCode.Value, causeClassName, null, null);
            }

            var exception = CreateException(errorCode, className, message, cause);

            return exception;
        }

        private Exception CreateException(int errorCode, string className, string message, Exception cause)
        {
            ExceptionFactoryDelegate factory;
            if (Enum.IsDefined(typeof(ClientProtocolErrorCodes), errorCode) &&
                errorCodeToException.TryGetValue((ClientProtocolErrorCodes)errorCode, out factory))
            {
                return factory.Invoke(message, cause);
            }
            return new HazelcastException(message, cause);
        }
    }
}