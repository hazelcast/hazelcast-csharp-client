// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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

// This code file is heavily inspired from the .NET Runtime code, which
// is licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// ReSharper disable once CheckNamespace
namespace System.Buffers
{
    /// <summary>
    /// Provides extension methods for the <see cref="ReadOnlySequence{T}"/> struct.
    /// </summary>
    internal static class ReadOnlySequenceExtensions
    {
        /// <summary>
        /// Gets the first span of a sequence.
        /// </summary>
        /// <typeparam name="T">The type of the objects in the sequence.</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <returns>The first span of the sequence.</returns>
        /// <remarks>
        /// <para>Built-in sequence.FirstSpan property is introduced in netstandard2.1.</para>
        /// </remarks>
        public static ReadOnlySpan<T> FirstSpan<T>(this ReadOnlySequence<T> sequence)
        {
#if NET462 || NETSTANDARD2_0
            return sequence.First.Span;
#else
            return sequence.FirstSpan;
#endif
        }
    }
}
