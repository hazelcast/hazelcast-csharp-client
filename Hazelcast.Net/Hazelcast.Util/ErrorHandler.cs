using System;
using System.IO;
using Hazelcast.Core;

namespace Hazelcast.Util
{
    public class ErrorHandler
    {
        public static T ReturnResultOrThrowException<T>(object result)
        {
            var exception = result as Exception;
            if (exception != null)
            {
                throw exception;
                //ExceptionUtil.FixRemoteStackTrace((Exception)result, Thread.CurrentThread().GetStackTrace());
                //ExceptionUtil.SneakyThrow((Exception)result);
                //return null;
            }
            return result != null ? (T) result : default(T);
        }

        public static bool IsRetryable(Exception e)
        {
            return e is IOException || e is HazelcastInstanceNotActiveException;
        }
    }
}