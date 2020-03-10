// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Client;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec.BuiltIn;
using Hazelcast.Client.Protocol.Codec.Custom;
using Hazelcast.Core;
using Hazelcast.Logging;

namespace Hazelcast.Util
{
    internal static class ExceptionUtil
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

        public static Exception Rethrow(Exception t)
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
            return t;
        }

        public static Exception Rethrow<Result>(Exception t, Func<Exception, Result> exceptionFactory) where Result : Exception
        {
            if (t is NotImplementedException)
            {
                return t;
            }
            if (t.InnerException != null)
            {
                return Rethrow(t.InnerException, exceptionFactory);
            }
            if (t is AggregateException)
            {
                var readOnlyCollection = ((AggregateException) t).InnerExceptions;
                foreach (var exception in readOnlyCollection)
                {
                    var cause =  Rethrow(exception);
                    return Rethrow(cause, exceptionFactory);
                }
            }
            return exceptionFactory.Invoke(t);
        }

        public static Exception ToException(this ClientMessage clientMessage)
        {
            var error = ErrorsCodec.Decode(clientMessage);
            return ExceptionFactory.CreateException(error.GetEnumerator());
        }
    }
}