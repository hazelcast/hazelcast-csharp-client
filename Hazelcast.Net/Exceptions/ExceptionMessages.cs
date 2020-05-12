namespace Hazelcast.Exceptions
{
    /// <summary>
    /// Defines common exception messages.
    /// </summary>
    public static class ExceptionMessages
    {
        /// <summary>
        /// Not enough bytes.
        /// </summary>
        public const string NotEnoughBytes = "Not enough bytes.";

        /// <summary>
        /// Invalid target.
        /// </summary>
        public const string InvalidTarget = "Invalid target.";

        /// <summary>
        /// Value cannot be null nor empty.
        /// </summary>
        public const string NullOrEmpty = "Value cannot be null nor empty.";

        /// <summary>
        /// Cached value is not of the expected type.
        /// </summary>
        public const string InvalidCacheCast = "Cached value is not of the expected type.";

        /// <summary>
        /// Property is now read only and cannot be modified.
        /// </summary>
        public const string PropertyIsNowReadOnly = "The property is now readonly and cannot be modified.";
    }
}
