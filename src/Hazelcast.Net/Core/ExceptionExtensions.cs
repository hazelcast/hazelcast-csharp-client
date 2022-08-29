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
using System.Diagnostics;
#if !NET6_0_OR_GREATER
using System.Linq.Expressions;
using System.Reflection;
#endif
using System.Runtime.ExceptionServices;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods for exceptions.
    /// </summary>
#if NET6_0_OR_GREATER
    [StackTraceHidden]
#endif
    internal static class ExceptionExtensions
    {
        /// <summary>Stores the current stack trace into the specified <typeparamref name="TException"/> exception instance.</summary>
        /// <typeparam name="TException">The type of the exception.</typeparam>
        /// <param name="exception">The unthrown <typeparamref name="TException"/> exception.</param>
        /// <returns>The <paramref name="exception"/> exception instance.</returns>
        /// <remarks>
        /// <para>Use this method to store a stack trace into an exception without having to throw it.</para>
        /// </remarks>
        public static TException SetCurrentStackTrace<TException>(this TException exception)
            where TException : Exception
        {
            // starting with .NET 5, ExceptionDispatchInfo can do what we want - but the StackTraceHidden
            // attribute is internal in .NET 5 so we cannot hide this SetCurrentStackTrace extension
            // method - therefore we use the new ExceptionDispatchInfo for .NET 6 and greater only
            //
            // for everything else, we have to force the stack trace through reflection, and to manually
            // remove the first line -  which will be this SetCurrentStackTrace extension.

#if NET6_0_OR_GREATER
            exception = (TException) ExceptionDispatchInfo.SetCurrentStackTrace(exception);
#else
            var stackTraceString = new StackTrace(fNeedFileInfo: true).ToString();
            var pos = stackTraceString.IndexOf('\n', StringComparison.OrdinalIgnoreCase);
            if (pos > 0) stackTraceString = stackTraceString.Substring(pos + 1);
            SetStackTraceField(exception, stackTraceString);
#endif
            return exception;
        }
#if !NET6_0_OR_GREATER
        // compiles a dynamic method that sets the Exception._stackTraceString internal field via reflection
        private static readonly Action<Exception, string> SetStackTraceField = new Func<Action<Exception, string>>(() =>
        {
            var target = Expression.Parameter(typeof(Exception));
            var stackTraceString = Expression.Parameter(typeof(string));
            var stackTraceStringField = typeof(Exception).GetField("_stackTraceString", BindingFlags.NonPublic | BindingFlags.Instance);
            var assign = Expression.Assign(Expression.Field(target, stackTraceStringField), stackTraceString);
            return Expression.Lambda<Action<Exception, string>>(assign, target, stackTraceString).Compile();
        })();
#endif
        /// <summary>
        /// Captures an exception.
        /// </summary>
        /// <param name="e">The exception to capture.</param>
        /// <returns>The new <see cref="ExceptionDispatchInfo"/> which captures the exception.</returns>
        public static ExceptionDispatchInfo Capture(this Exception e)
            => ExceptionDispatchInfo.Capture(e);
    }
}
