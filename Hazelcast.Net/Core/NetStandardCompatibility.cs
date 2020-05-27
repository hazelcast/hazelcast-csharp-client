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
using System.Buffers;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
#if NETSTANDARD2_0
using System.Collections.Generic;
using System.IO;
using System.Threading;
#endif

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides workarounds for missing capabilities in netstandard2.0.
    /// </summary>
    public static class NetStandardCompatibility
    {
#if NETSTANDARD2_0
        /// <summary>
        /// Deconstructs a <see cref="KeyValuePair{TKey,TValue}"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="keyValuePair">The key-value pair.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <remarks>
        /// <para>Built-in deconstruction of key-value pairs is introduced in netstandard2.1.</para>
        /// </remarks>
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> keyValuePair, out TKey key, out TValue value)
        {
            key = keyValuePair.Key;
            value = keyValuePair.Value;
        }
#endif

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

        /// <summary>
        /// Determines whether the task ran to successful completion.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <returns>true if the task ran to successful completion, otherwise false.</returns>
        /// <remarks>
        /// <para>Built-in task.IsCompletedSuccessfully property is introduced in netstandard2.1.</para>
        /// </remarks>
        public static bool IsCompletedSuccessfully(this Task task)
        {
#if NETSTANDARD2_0
            return task.IsCompleted && !(task.IsFaulted || task.IsCanceled);
#endif
#if NETSTANDARD2_1
            return task.IsCompletedSuccessfully;
#endif
        }

#if NETSTANDARD2_0
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
            byte[] bytes = null;
            try
            {
                // TODO: optimize
                // of course this is sub-optimal :(
                bytes = ArrayPool<byte>.Shared.Rent(memory.Length);

                // stream.ReadAsync for network streams *ignores* the cancellation token
                // see https://github.com/dotnet/runtime/issues/24093
                // hences this... workaround
                //
                var reading = stream.ReadAsync(bytes, 0, memory.Length, cancellationToken);
                var completed = await Task.WhenAny(reading, Task.Delay(-1, cancellationToken)).CAF();

                if (completed != reading)
                    throw new TaskCanceledException();

                var count = await reading.CAF();

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
#endif

        /*
        private static ArraySegment<byte> GetUnderlyingArray(this Memory<byte> bytes) => GetUnderlyingArray((ReadOnlyMemory<byte>)bytes);

        private static ArraySegment<byte> GetUnderlyingArray(this ReadOnlyMemory<byte> bytes)
        {
            if (!MemoryMarshal.TryGetArray(bytes, out var arraySegment)) throw new NotSupportedException("This Memory does not support exposing the underlying array.");
            return arraySegment;
        }
        */

        // ReSharper disable once InconsistentNaming
        public static class IPEndPoint
        {
            // this code is directly copied from .NET Core runtime, with minor adjustments
            // ReSharper disable all

            public static bool TryParse(string s, /*[NotNullWhen(true)]*/ out System.Net.IPEndPoint result)
            {
                return TryParse(s.AsSpan(), out result);
            }

            public static bool TryParse(ReadOnlySpan<char> s, /*[NotNullWhen(true)]*/ out System.Net.IPEndPoint result)
            {
                int addressLength = s.Length;  // If there's no port then send the entire string to the address parser
                int lastColonPos = s.LastIndexOf(':');

                // Look to see if this is an IPv6 address with a port.
                if (lastColonPos > 0)
                {
                    if (s[lastColonPos - 1] == ']')
                    {
                        addressLength = lastColonPos;
                    }
                    // Look to see if this is IPv4 with a port (IPv6 will have another colon)
                    else if (s.Slice(0, lastColonPos).LastIndexOf(':') == -1)
                    {
                        addressLength = lastColonPos;
                    }
                }

#if NETSTANDARD2_0
                if (IPAddress.TryParse(s.Slice(0, addressLength).ToString(), out IPAddress address))
                {
                    uint port = 0;
                    if (addressLength == s.Length ||
                        (uint.TryParse(s.Slice(addressLength + 1).ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out port) && port <= System.Net.IPEndPoint.MaxPort))

                    {
                        result = new System.Net.IPEndPoint(address, (int)port);
                        return true;
                    }
                }
#endif
#if NETSTANDARD2_1
                if (IPAddress.TryParse(s.Slice(0, addressLength), out IPAddress address))
                {
                    uint port = 0;
                    if (addressLength == s.Length ||
                        (uint.TryParse(s.Slice(addressLength + 1), NumberStyles.None, CultureInfo.InvariantCulture, out port) && port <= System.Net.IPEndPoint.MaxPort))

                    {
                        result = new System.Net.IPEndPoint(address, (int)port);
                        return true;
                    }
                }
#endif

                result = null;
                return false;
            }

            /// <summary>
            /// Converts an IP network endpoint (address and port) represented as a string to an IPEndPoint instance.
            /// </summary>
            /// <param name="s">The string.</param>
            /// <returns>An IP network endpoint.</returns>
            public static System.Net.IPEndPoint Parse(string s)
            {
                if (s == null)
                {
                    throw new ArgumentNullException(nameof(s));
                }

                return Parse(s.AsSpan());
            }

            public static System.Net.IPEndPoint Parse(ReadOnlySpan<char> s)
            {
                if (TryParse(s, out System.Net.IPEndPoint result))
                {
                    return result;
                }

                throw new FormatException("Invalid format.");
            }

            // ReSharper restore all
        }

        #region RuntimeCode



        #endregion
    }
}
