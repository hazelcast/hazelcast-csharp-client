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

// This code file is heavily inspired from the .NET Runtime code, which
// is licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if NET462 || NETSTANDARD2_0
using System.Buffers;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;

// ReSharper disable once CheckNamespace
namespace System.IO
{
    /// <summary>
    /// Provides extension methods for the <see cref="Stream"/> class.
    /// </summary>
    internal static class StreamExtensions
    {
        /// <summary>
        /// Reads from a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="memory">The region of memory to write the data into.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The number of bytes that were read.</returns>
        /// <remarks>
        /// <para>Built-in method is introduced in netstandard2.1.</para>
        /// <para>This can theoretically fail.</para>
        /// </remarks>
        public static async ValueTask<int> ReadAsync(this Stream stream, Memory<byte> memory, CancellationToken cancellationToken)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (memory.Length == 0) throw new ArgumentException("Empty memory.", nameof(memory));

            // this is based upon what 2.1 does

            if (MemoryMarshal.TryGetArray(memory, out ArraySegment<byte> array))
            {
                //return await stream.ReadAsync(array.Array, array.Offset, array.Count, cancellationToken).CAF();

                // see below, cancellation issue
                var reading = stream.ReadAsync(array.Array, array.Offset, array.Count, cancellationToken);
                var completed = await Task.WhenAny(reading, Task.Delay(-1, cancellationToken)).ConfigureAwait(false);

                if (completed != reading)
                {
                    _ = reading.ObserveException();
                    throw new TaskCanceledException();
                }

                var result = await reading.CAF();
                return result;
            }

            var bytes = ArrayPool<byte>.Shared.Rent(memory.Length);
            try
            {
                //var result = await stream.ReadAsync(bytes, 0, memory.Length, cancellationToken).CAF();

                // stream.ReadAsync for network streams *ignores* the cancellation token
                // see https://github.com/dotnet/runtime/issues/24093
                // hence this... workaround
                //
                var reading = stream.ReadAsync(bytes, 0, memory.Length, cancellationToken);
                var completed = await Task.WhenAny(reading, Task.Delay(-1, cancellationToken)).ConfigureAwait(false);

                if (completed != reading)
                {
                    _ = reading.ObserveException();
                    throw new TaskCanceledException();
                }

                var result = await reading.CAF();

                new Span<byte>(bytes, 0, result).CopyTo(memory.Span);
                return result;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(bytes);
            }
        }
    }
}

#endif
