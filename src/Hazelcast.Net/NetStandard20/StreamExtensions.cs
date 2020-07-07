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

#if NET462 || NETSTANDARD2_0
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;

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
        /// <param name="memory">The memory to read into.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The number of bytes that were read.</returns>
        /// <remarks>
        /// <para>Built-in method is introduced in netstandard2.1.</para>
        /// <para>This can theoretically fail </para>
        /// </remarks>
        public static async Task<int> ReadAsync(this Stream stream, Memory<byte> memory, CancellationToken cancellationToken)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            byte[] bytes = null;
            try
            {
                // TODO: optimize
                // of course this is sub-optimal :(
                bytes = ArrayPool<byte>.Shared.Rent(memory.Length);

                // stream.ReadAsync for network streams *ignores* the cancellation token
                // see https://github.com/dotnet/runtime/issues/24093
                // hence this... workaround
                //
                var reading = stream.ReadAsync(bytes, 0, memory.Length, cancellationToken);
                var completed = await Task.WhenAny(reading, Task.Delay(-1, cancellationToken)).ConfigureAwait(false);

                if (completed != reading)
                    throw new TaskCanceledException();

                var count = await reading.ConfigureAwait(false);

                new ReadOnlySpan<byte>(bytes).Slice(0, count).CopyTo(memory.Span);
                return count;
            }
            finally
            {
                if (bytes != null)
                    ArrayPool<byte>.Shared.Return(bytes);
            }

            // see https://stackoverflow.com/questions/50078640/spant-and-streams
            // but... memory.GetUnderlyingArray().Array is for readonly memory only
            // cannot work in our case?
        }
    }
}

#endif
