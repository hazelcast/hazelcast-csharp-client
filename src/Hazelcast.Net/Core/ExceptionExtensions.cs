// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Runtime.ExceptionServices;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods for exceptions.
    /// </summary>
    internal static class ExceptionExtensions
    {
        /// <summary>
        /// Throws and catches an exception, thus forcing it to have a stack trace.
        /// </summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <param name="exception">The exception.</param>
        /// <returns>The exception after it has been thrown and caught.</returns>
        /// <remarks>
        /// <para>This can be useful when creating, but not immediately throwing, an exception, such
        /// as with <code>taskContinuationSource.TrySetException(new Exception())</code> in order
        /// to force the new exception to have a stack trace that corresponds to its creation.</para>
        /// </remarks>
        public static TException Thrown<TException>(this TException exception)
            where TException : Exception
        {
            try { throw exception; } catch { /*nothing*/ }
            return exception;
        }

        /// <summary>
        /// Captures an exception.
        /// </summary>
        /// <param name="e">The exception to capture.</param>
        /// <returns>The new <see cref="ExceptionDispatchInfo"/> which captures the exception.</returns>
        public static ExceptionDispatchInfo Capture(this Exception e)
            => ExceptionDispatchInfo.Capture(e);
    }
}
