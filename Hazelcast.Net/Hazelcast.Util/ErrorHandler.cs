using System;
using System.IO;
using Hazelcast.Client.Request.Base;
using Hazelcast.Core;

namespace Hazelcast.Util
{
    public class ErrorHandler
    {
        //public static T ReturnResultOrThrowException<T>(ClientResponse result)
        //{
        //    var error = result.Error;
        //    if (error != null)
        //    {
        //        throw new Exception(error.Message);
        //    }
        //    return result.Response != null ? (T)result.Response : default(T);
        //}

        public static bool IsRetryable(Exception e)
        {
            return e is IOException || e is HazelcastInstanceNotActiveException;
        }
    }
}