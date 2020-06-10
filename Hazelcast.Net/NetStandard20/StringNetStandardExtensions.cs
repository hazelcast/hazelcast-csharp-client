

// ReSharper disable once CheckNamespace
namespace System
{
    internal static class StringNetStandardExtensions
    {
#if NETSTANDARD2_0
#pragma warning disable CA1801 // Review unused parameters - we need them
#pragma warning disable IDE0060 // Remove unused parameter

        public static int GetHashCode(this string s, StringComparison comparison)
            => s.GetHashCode();

        public static int IndexOf(this string s, char c, StringComparison comparison)
            => s.IndexOf(c);

        public static string Replace(this string s, string o, string r, StringComparison comparison)
            => s.Replace(o, r);

#pragma warning restore CA1801
#pragma warning restore IDE0060
#endif
    }
}
