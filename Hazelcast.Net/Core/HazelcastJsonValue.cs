using System;

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents a JSON formatted string.
    /// </summary>
    /// <remarks>
    /// <para>It is preferred to store HazelcastJsonValue instead of String for JSON formatted strings.
    /// Users can then run predicates and aggregations and use indexes on the attributes of the underlying
    /// JSON content.</para>
    /// <para>Note that the string is not validated and may be invalid JSON.</para>
    /// </remarks>
    public sealed class HazelcastJsonValue
    {
        private readonly string _json;

        /// <summary>
        /// Initializes a new instance of the <see cref="HazelcastJsonValue"/> with a string containing JSON.
        /// </summary>
        /// <param name="json">The string containing JSON.</param>
        public HazelcastJsonValue(string json)
        {
            _json = json ?? throw new ArgumentNullException(nameof(json));
        }

        /// <inheritdoc />
        public override string ToString() => _json;

        /// <inheritdoc />
        public override int GetHashCode() => _json.GetHashCode();

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            return obj is HazelcastJsonValue other && Equals(this, other);
        }

        /// <summary>
        /// Compares two instances of the <see cref="HazelcastJsonValue"/> for equality.
        /// </summary>
        /// <param name="x1">The first instance.</param>
        /// <param name="x2">The second instance.</param>
        /// <returns>true if the two instances are equal; otherwise false.</returns>
        private static bool Equals(HazelcastJsonValue x1, HazelcastJsonValue x2)
            => x1._json == x2._json;
    }
}
