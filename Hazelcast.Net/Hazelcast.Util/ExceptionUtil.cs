using System;
using Hazelcast.Client;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Core;

namespace Hazelcast.Util
{
    internal sealed class ExceptionUtil
    {

        private static readonly ClientExceptionFactory ExceptionFactory = new ClientExceptionFactory();

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
            if (allowedType != null && allowedType.IsInstanceOfType(t))
            {
                throw t;
            }
            throw new HazelcastException(t);
        }

        public static Exception Rethrow(Error error)
        {
            return Rethrow(ExceptionFactory.CreateException(error));
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