using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast
{
    /// <summary>
    /// Provides workarounds for missing capabilities in netstandard2.0.
    /// </summary>
    public static class NetStandardCompatibility
    {
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
            (key, value) = keyValuePair;
        }

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
            return sequence.First.Span;
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
            return task.IsCompleted && !(task.IsFaulted || task.IsCanceled);
        }

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
                // TODO optimize?
                // of course this is sub-optimal :(
                bytes = ArrayPool<byte>.Shared.Rent(memory.Length);
                var count = await stream.ReadAsync(bytes, 0, bytes.Length, cancellationToken);
                bytes.CopyTo(memory);
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

        private static ArraySegment<byte> GetUnderlyingArray(this Memory<byte> bytes) => GetUnderlyingArray((ReadOnlyMemory<byte>)bytes);

        private static ArraySegment<byte> GetUnderlyingArray(this ReadOnlyMemory<byte> bytes)
        {
            if (!MemoryMarshal.TryGetArray(bytes, out var arraySegment)) throw new NotSupportedException("This Memory does not support exposing the underlying array.");
            return arraySegment;
        }
    }
}
