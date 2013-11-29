using System;

namespace Hazelcast.Util
{
    public sealed class ExceptionUtil
    {
        private const string ExceptionSeparator = "------ End remote and begin local stack-trace ------";

        public static Exception Rethrow(Exception t)
        {
            throw t;
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

        public static T SneakyThrow<T>(Exception t) where T : Exception
        {
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