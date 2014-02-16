using System;
using System.IO;
using System.Threading;
using Hazelcast.Client;
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
    public sealed class ExceptionUtil
    {
        private const string ExceptionSeparator = "------ End remote and begin local stack-trace ------";

        public static Exception Rethrow(Exception t)
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
            if (t is GenericError)
            {
                var genericError = ((GenericError) t);
                var name = genericError.Name.Substring(genericError.Name.LastIndexOf(".")+1);
                switch (name)
                {
                    case "IllegalStateException":
                        throw new InvalidOperationException(genericError.Message);
                    case "IllegalMonitorStateException":
                        throw new SynchronizationLockException(genericError.Message);
                    case "IOException":
                        throw new IOException(genericError.Message);
                    case "TransactionException":
                        throw new TransactionException(genericError.Message);
                    case "TransactionNotActiveException":
                        throw new TransactionNotActiveException(genericError.Message);
                    case "HazelcastException":
                        throw new HazelcastException(genericError.Message);
                    case "NullPointerException":
                        throw new NullReferenceException(genericError.Message);
                    case "QueryException":
                        throw new QueryException(genericError.Message);
                    default:
                        throw t;
                }
            }

            Logger.GetLogger(typeof(HazelcastException)).Finest(t);
            throw new HazelcastException(t);
            //if (t. is Error)
            //{
            //    if (t is OutOfMemoryException)
            //    {
            //        OutOfMemoryErrorDispatcher.OnOutOfMemory((OutOfMemoryException)t);
            //    }
            //    throw (Error)t;
            //}
            //else
            //{
            //    if (t is RuntimeException)
            //    {
            //        throw (RuntimeException)t;
            //    }
            //    else
            //    {
            //        if (t is ExecutionException)
            //        {
            //            Exception cause = t.InnerException;
            //            if (cause != null)
            //            {
            //                throw Rethrow(cause);
            //            }
            //            else
            //            {
            //                throw new HazelcastException(t);
            //            }
            //        }
            //        else
            //        {
            //            throw new HazelcastException(t);
            //        }
            //    }
            //}
        }

        /// <exception cref="T"></exception>
        public static Exception Rethrow<T>(Exception t) where T : Exception
        {
            return Rethrow<Exception>(t);
            //System.Type allowedType = typeof(T);
            //if (t is Error)
            //{
            //    if (t is OutOfMemoryException)
            //    {
            //        OutOfMemoryErrorDispatcher.OnOutOfMemory((OutOfMemoryException)t);
            //    }
            //    throw (Error)t;
            //}
            //else
            //{
            //    if (t is RuntimeException)
            //    {
            //        throw (RuntimeException)t;
            //    }
            //    else
            //    {
            //        if (t is ExecutionException)
            //        {
            //            Exception cause = t.InnerException;
            //            if (cause != null)
            //            {
            //                throw Rethrow(cause, allowedType);
            //            }
            //            else
            //            {
            //                throw new HazelcastException(t);
            //            }
            //        }
            //        else
            //        {
            //            if (allowedType.IsAssignableFrom(t.GetFieldType()))
            //            {
            //                throw (T)t;
            //            }
            //            else
            //            {
            //                throw new HazelcastException(t);
            //            }
            //        }
            //    }
            //}
        }

        /// <exception cref="System.Exception"></exception>
        public static Exception RethrowAllowInterrupted(Exception t)
        {
            return Rethrow<Exception>(t);
        }

        //public static T SneakyThrow<T>(Exception t) where T : Exception
        //{
        //    throw t;
        //}

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