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
#if NETSTANDARD2_0
            return sequence.First.Span;
#endif
#if NETSTANDARD2_1
            return sequence.FirstSpan;
#endif
        }
    }
}
