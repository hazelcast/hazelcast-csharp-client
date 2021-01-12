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
        // NOTE: do *not* use the .CAF() extension method in this class, as this
        // creates a cyclic dependency from System.* to Hazelcast.* which we'd
        // rather avoid - stick with .ConfigureAwait(false).

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

            byte[] bytes;
            int offset;
            int count;
            bool rentedBytes;

            if (MemoryMarshal.TryGetArray(memory, out ArraySegment<byte> array))
            {
                bytes = array.Array; // directly get the underlying array
                rentedBytes = false;

                offset = array.Offset;
                count = array.Count;
            }
            else
            {
                bytes = ArrayPool<byte>.Shared.Rent(memory.Length); // rent an array
                rentedBytes = true;

                offset = 0;
                count = memory.Length;
            }

            CancellationTokenRegistration reg;

            try
            {
                // stream.ReadAsync for network streams *ignores* the cancellation token
                // see https://github.com/dotnet/runtime/issues/24093
                // so, we wait on two tasks, including one that will complete when the cancellation
                // token is canceled - beware, do NOT use Task.Delay(-1, cancellationToken) as it
                // would stay around forever (leak) - instead, use a completion source

                var completion = new TaskCompletionSource<int>();
                reg = cancellationToken.Register(() => completion.TrySetCanceled());

                var reading = stream.ReadAsync(bytes, offset, count, cancellationToken);
                var completed = await Task.WhenAny(reading, completion.Task).CfAwait();

                if (completed != reading)
                {
                    _ = reading.ObserveException();
                    throw new TaskCanceledException();
                }

                var result = await reading.ConfigureAwait(false);

                // copy the rented array
                if (rentedBytes) new Span<byte>(bytes, 0, result).CopyTo(memory.Span);

                return result;
            }
            finally
            {
                reg.Dispose(); // don't leak the registration
                if (rentedBytes) ArrayPool<byte>.Shared.Return(bytes); // return the rented array
            }
        }
    }
}

#endif
