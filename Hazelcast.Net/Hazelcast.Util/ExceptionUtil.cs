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
using Hazelcast.Client;
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
                var readOnlyCollection = ((AggregateException) t).InnerExceptions;
                foreach (var exception in readOnlyCollection)
                {
                    return Rethrow(exception);
                }
            }
            return t;
        }

        public static Exception ToException(Error error)
        {
            return ExceptionFactory.CreateException(error);
        }
    }
}