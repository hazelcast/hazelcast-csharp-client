/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using Hazelcast.Client;
using Hazelcast.Client.Protocol.Codec;
using Hazelcast.Core;

namespace Hazelcast.Util
{
    internal sealed class ExceptionUtil
    {

        private static readonly ClientExceptionFactory ExceptionFactory = new ClientExceptionFactory();

        public static Exception Rethrow(Exception t, params Type[] allowedTypes)
        {
            if (t is NotImplementedException)
            {
                return t;
            }
            if (t is AggregateException)
            {
                var readOnlyCollection = ((AggregateException) t).InnerExceptions;
                foreach (var exception in readOnlyCollection)
                {
                    return Rethrow(exception);
                }
            }

            foreach (var allowedType in allowedTypes)
            {
                if (allowedType != null && allowedType.IsInstanceOfType(t))
                {
                    return t;
                }
            }
            return new HazelcastException(t);
        }

        public static Exception ToException(Error error)
        {
            return ExceptionFactory.CreateException(error);
        }

        public static Exception Rethrow(Error error)
        {
            return Rethrow(ToException(error));
        }

        public static Exception Rethrow(Exception t)
        {
            if (t is NotImplementedException)
            {
                return t;
            }
            if (t is AggregateException)
            {
                var readOnlyCollection = ((AggregateException)t).InnerExceptions;
                foreach (var exception in readOnlyCollection)
                {
                    return Rethrow(exception);
                }
            }
            return t;
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