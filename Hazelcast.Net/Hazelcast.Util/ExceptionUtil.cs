using System;
using System.IO;
using System.Threading;
using Hazelcast.Client;
using Hazelcast.Client.Spi;
using Hazelcast.Core;
using Hazelcast.Logging;
using Hazelcast.Transaction;

namespace Hazelcast.Util
{


/*
All Possioble Exceptions that are thrown at hazelcast-core:

ConcurrentModificationException
DuplicateInstanceNameException
HazelcastException
HazelcastSerializationException
IllegalArgumentException
IllegalMonitorStateException
IllegalStateException
IllegalThreadStateException
IOException
NegativeArraySizeException
NoSuchElementException
NullPointerException
PartitionMigratingException
QueryException
ReflectionException
RejectedExecutionException
ResponseAlreadySentException
RetryableHazelcastException
RuntimeException
TargetNotMemberException
TimeoutException
TransactionException
TransactionNotActiveException
UnsupportedOperationException
UTFDataFormatException
WrongTargetException
*/
    /// </summary>
    internal sealed class ExceptionUtil
    {
        public static Exception Rethrow(Exception t, Type allowedType)
        {
            if (t is NotImplementedException)
            {
                throw t;
            }
            if (t is AggregateException)
            {
                var readOnlyCollection = ((AggregateException) t).InnerExceptions;
                foreach (var exception in readOnlyCollection)
                {
                    Rethrow(exception);
                }
            }
            var e = t is GenericError ? ConvertGenericError((GenericError)t) : t;
            if (allowedType != null && allowedType.IsInstanceOfType(e))
            {
                throw e;
            }
            throw new HazelcastException(t);
        }

        public static Exception ConvertGenericError(GenericError genericError)
        {
            var name = genericError.Name.Substring(genericError.Name.LastIndexOf(".") + 1);
            switch (name)
            {
                case "IllegalStateException":
                    return new InvalidOperationException(genericError.Message);
                case "IllegalMonitorStateException":
                    return new SynchronizationLockException(genericError.Message);
                case "IOException":
                    return new IOException(genericError.Message);
                case "TransactionException":
                    return new TransactionException(genericError.Message);
                case "TransactionNotActiveException":
                    return new TransactionNotActiveException(genericError.Message);
                case "HazelcastException":
                    return new HazelcastException(genericError.Message);
                case "NullPointerException":
                    return new NullReferenceException(genericError.Message);
                case "QueryException":
                    return new QueryException(genericError.Message);
                case "HazelcastInstanceNotActiveException":
                    return new HazelcastInstanceNotActiveException();
                case "TargetDisconnectedException":
                    return new TargetDisconnectedException(genericError.Message);
                default:
                    return new HazelcastException(genericError.Message);
            }
        }

        public static Exception Rethrow(Exception t)
        {
            if (t is NotImplementedException)
            {
                throw t;
            }
            if (t is AggregateException)
            {
                var readOnlyCollection = ((AggregateException)t).InnerExceptions;
                foreach (var exception in readOnlyCollection)
                {
                    Rethrow(exception);
                }
            }
            if (t is GenericError)
            {

                throw ConvertGenericError(t as GenericError);
            }
            throw t;
        }


        //public static void FixRemoteStackTrace(Exception remoteCause, StackTraceElement[] localSideStackTrace)
        //{
        //    StackTraceElement[] remoteStackTrace = remoteCause.GetStackTrace();
        //    StackTraceElement[] newStackTrace = new StackTraceElement[localSideStackTrace.Length + remoteStackTrace.Length];
        //    System.Array.Copy(remoteStackTrace, 0, newStackTrace, 0, remoteStackTrace.Length);
        //    newStackTrace[remoteStackTrace.Length] = new StackTraceElement(ExceptionSeparator, string.Empty, null, -1);
        //    System.Array.Copy(localSideStackTrace, 1, newStackTrace, remoteStackTrace.Length + 1, localSideStackTrace.Length - 1);
        //    remoteCause.SetStackTrace(newStackTrace);
        //}
    }
}