namespace Hazelcast.Tests.Testing
{
    /// <summary>
    /// Provides extension methods to the <see cref="string"/> class.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Converts all cr/lf to lf.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>The converted string.</returns>
        public static string ToLf(this string s)
        {
            return s.Replace("\r\n", "\n").Replace("\r", "\n");
        }
    }
}
