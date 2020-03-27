using System;
using NuGet.Versioning;
using NUnit.Framework;

namespace AsyncTests1
{
    /// <summary>
    /// Provides static methods to execute code depending on the Hazelcast server version.
    /// </summary>
    public static class ServerCondition
    {
        /// <summary>
        /// Executes some code if the server version is within a specific range.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <param name="codeInRange">The code to execute if the server version is within the specified range.</param>
        public static void InRange(string range, TestDelegate codeInRange)
        {
            if (!VersionRange.TryParse(range, out var r))
                throw new ArgumentException("Invalid range.", nameof(range));

            if (r.Satisfies(HazelcastServerVersionAttribute.ServerVersion))
                codeInRange();
        }

        /// <summary>
        /// Executes some code depending on whether the server version is within a specific range or not.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <param name="codeInRange">The code to execute if the server version is within the specified range.</param>
        /// <param name="codeNotInRange">The code to execute if the server version is not within the specified range.</param>
        public static void InRange(string range, TestDelegate codeInRange, TestDelegate codeNotInRange)
        {
            if (!VersionRange.TryParse(range, out var r))
                throw new ArgumentException("Invalid range.", nameof(range));

            if (r.Satisfies(HazelcastServerVersionAttribute.ServerVersion))
                codeInRange();
            else
                codeNotInRange();
        }

        /// <summary>
        /// Executes some code if the server version is not within a specific range.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <param name="codeNotInRange">The code to execute  if the server version is not within the specified range.</param>
        public static void NotInRange(string range, TestDelegate codeNotInRange)
        {
            if (!VersionRange.TryParse(range, out var r))
                throw new ArgumentException("Invalid range.", nameof(range));

            if (!r.Satisfies(HazelcastServerVersionAttribute.ServerVersion))
                codeNotInRange();
        }
    }
}