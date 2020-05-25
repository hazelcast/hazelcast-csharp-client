using System.Buffers;

namespace Hazelcast.Core
{
    /// <summary>
    /// Defines a reference to a buffer.
    /// </summary>
    /// <typeparam name="T">The type of the buffer.</typeparam>
    /// <remarks>
    /// <para>This interface is meant to allow to pass references to buffers that
    /// may be struct, such as <see cref="ReadOnlySequence{T}"/>, which cannot be
    /// passed as 'ref' parameters in asynchronous calls.</para>
    /// </remarks>
    public interface IBufferReference<T>
    {
        /// <summary>
        /// Gets or sets the buffer.
        /// </summary>
        T Buffer { get; set; }
    }
}
