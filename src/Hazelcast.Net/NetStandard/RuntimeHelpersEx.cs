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
//
// This code file is heavily inspired from the .NET Runtime code, which
// is licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if NETSTANDARD2_0 ||  NETCOREAPP2_0 ||  NETCOREAPP2_1 ||  NETCOREAPP2_2 || NET45 || NET451 || NET452 || NET6 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48

// brings C# 8 index & range to netstandard 2.0
// see https://www.meziantou.net/how-to-use-nullable-reference-types-in-dotnet-standard-2-0-and-dotnet-.htm
//
// https://github.com/dotnet/corefx/blob/1597b894a2e9cac668ce6e484506eca778a85197/src/Common/src/CoreLib/System/Index.cs
// https://github.com/dotnet/corefx/blob/1597b894a2e9cac668ce6e484506eca778a85197/src/Common/src/CoreLib/System/Range.cs

using System.Diagnostics.CodeAnalysis;

#nullable enable

// ReSharper disable UnusedMember.Global

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    internal static class RuntimeHelpersEx
    {
        /// <summary>
        /// Slices the specified array using the specified range.
        /// </summary>
        [SuppressMessage("NDepend", "ND1400:AvoidNamespacesMutuallyDependent", Justification = "Imported code.")]
        [ExcludeFromCodeCoverage] // not covering MS code
        public static T[] GetSubArray<T>(T[] array, Range range)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            var (offset, length) = range.GetOffsetAndLength(array.Length);

            if (default(T) != null || typeof(T[]) == array.GetType())
            {
                // We know the type of the array to be exactly T[].

                if (length == 0)
                {
                    return Array.Empty<T>();
                }

                var dest = new T[length];
                Array.Copy(array, offset, dest, 0, length);
                return dest;
            }
            else
            {
                // The array is actually a U[] where U:T.
                var dest = (T[])Array.CreateInstance(array.GetType().GetElementType(), length);
                Array.Copy(array, offset, dest, 0, length);
                return dest;
            }
        }
    }
}

#endif
