using System;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods for <see cref="Enum"/>.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Determines whether one or more bit fields are set in the current instance.
        /// </summary>
        /// <typeparam name="T">The type of the enumeration.</typeparam>
        /// <param name="value">This instance value.</param>
        /// <param name="flags">An enumeration value.</param>
        /// <returns><c>true</c> if all the bit field or bit fields that are set in flag are also set in the current instance; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// <para>This extension methods works for enumerations backed by an <see cref="int"/> value, or any smaller value.
        /// No test is performed on <c>value.GetTypeCode()</c> and therefore results for enumerations backed, by example,
        /// by a <see cref="long"/> are unspecified.</para>
        /// <para>This is a convenient replacement for <see cref="Enum.HasFlag"/> which is way slower.</para>
        /// </remarks>
        public static bool HasAll<T>(this T value, T flags) where T : Enum
            => ((int) (IConvertible) value & (int) (IConvertible) flags) == (int) (IConvertible) flags;

        /// <summary>
        /// Determines whether one or more bit fields are set in the current instance.
        /// </summary>
        /// <typeparam name="T">The type of the enumeration.</typeparam>
        /// <param name="value">This instance value.</param>
        /// <param name="flags">An enumeration value.</param>
        /// <returns><c>true</c> if any of the bit field or bit fields that are set in flag are also set in the current instance; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// <para>This extension methods works for enumerations backed by an <see cref="int"/> value, or any smaller value.
        /// No test is performed on <c>value.GetTypeCode()</c> and therefore results for enumerations backed, by example,
        /// by a <see cref="long"/> are unspecified.</para>
        /// </remarks>
        public static bool HasAny<T>(this T value, T flags) where T : Enum
            => ((int) (IConvertible) value & (int) (IConvertible) flags) > 0;
    }
}
