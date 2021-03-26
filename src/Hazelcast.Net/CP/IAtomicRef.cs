using System.Threading.Tasks;

namespace Hazelcast.CP
{
    public interface IAtomicRef<T>: ICPDistributedObject
        where T: class
    {
        /// <summary>
        /// Compares the value for equality and, if equal, replaces the current value.
        /// </summary>
        /// <param name="comparand">The value that is compared to the current value.</param>
        /// <param name="value">The value that replaces the current value if the comparison results in equality.</param>
        /// <returns>The updated value.</returns>
        Task<bool> CompareAndSetAsync(T comparand, T value);

        /// <summary>
        /// Gets the current value.
        /// </summary>
        /// <returns>The current value.</returns>
        Task<T> GetAsync();

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        Task SetAsync(T value);

        /// <summary>
        /// Sets the current value, and returns the original value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>The original value.</returns>
        Task<T> GetAndSetAsync(T value);

        /// <summary>
        /// Checks if the stored reference is <c>null</c>.
        /// </summary>
        /// <returns><c>true</c> if <c>null</c>, <c>false</c> otherwise</returns>
        Task<bool> IsNullAsync();

        /// <summary>
        /// Clears current stored reference, so it becomes <c>null</c>.
        /// </summary>
        Task ClearAsync();

        /// <summary>
        /// Checks if the reference contains the value.
        /// </summary>
        /// <param name="value">The value to check (is allowed to be <c>null</c>).</param>
        /// <returns>Value the value to check (is allowed to be <c>null</c>).</returns>
        /// <remarks><c>true</c> if the value is found, <c>false</c> otherwise.</remarks>
        Task<bool> ContainsAsync(T value);
    }
}
