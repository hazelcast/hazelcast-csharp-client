// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Reflection;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides helper methods to manipulate symbols.
    /// </summary>
    internal static class Symbols
    {
        /// <summary>
        /// Represents just any type.
        /// </summary>
        public class AnyType
        { }

        /// <summary>
        /// Gets a method <see cref="MethodInfo"/>.
        /// </summary>
        /// <param name="a">The method.</param>
        /// <returns>The corresponding <see cref="MethodInfo"/>.</returns>
        public static MethodInfo GetMethodInfo(Action a)
            => a.Method;

        /// <summary>
        /// Gets a method generic <see cref="MethodInfo"/>.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>The corresponding <see cref="MethodInfo"/>.</returns>
        public static MethodInfo GetGenericMethodInfo<TResult>(Func<TResult> method)
            => method.Method.GetGenericMethodDefinition();

        /// <summary>
        /// Gets a method generic <see cref="MethodInfo"/>.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>The corresponding <see cref="MethodInfo"/>.</returns>
        public static MethodInfo GetGenericMethodInfo<T1, TResult>(Func<T1, TResult> method)
            => method.Method.GetGenericMethodDefinition();

        public static TResult Invoke<T1, TResult>(Func<T1, TResult> f, Type[] types, object[] parameters)
        {
            return (TResult)f.Method.GetGenericMethodDefinition().MakeGenericMethod(types).Invoke(null, parameters);
        }

        public class StaticFuncSymbol<T1, TResult>
        {
            private readonly MethodInfo _methodInfo;
            public StaticFuncSymbol(Func<T1, TResult> method) => _methodInfo = method.Method.GetGenericMethodDefinition();
            public TResult Invoke(Type type, T1 param) => (TResult)_methodInfo.MakeGenericMethod(type).Invoke(null, new object[] { param });
        }
    }
}
