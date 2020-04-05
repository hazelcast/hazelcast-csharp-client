using System;
using NuGet.Versioning;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace AsyncTests1.HazelcastServerVersion
{
    /// <summary>
    /// Marks a class or a method as depending on the server version being within a specific versions range.
    /// </summary>
    /// <remarks>
    /// <para>If the server version condition is not met, i.e. if the specified range
    /// does not contain the current server version, then the test fixture or test
    /// method is ignored.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ServerConditionAttribute : Attribute, IApplyToTest
    {
        private static NuGetVersion _serverVersion;
        private readonly VersionRange _range;

        /// <summary>
        /// Marks the class or method as depending on the server version being within a specific versions range.
        /// </summary>
        /// <param name="range">A versions range.</param>
        public ServerConditionAttribute(string range)
        {
            if (!VersionRange.TryParse(range, out _range))
                throw new ArgumentException("Invalid range.", nameof(range));
        }

        // semver ranges specification
        // as used by NuGet
        //
        // 1.0          x >= 1.0
        // (1.0,)       x > 1.0
        // [1.0]        x == 1.0
        // (,1.0]       x ≤ 1.0
        // (,1.0)       x< 1.0
        // [1.0, 2.0    1.0 ≤ x ≤ 2.0
        // (1.0,2.0)    1.0 < x< 2.0
        // [1.0, 2.0)   1.0 ≤ x < 2.0
        // (1.0)        invalid

        /// <inheritdoc />
        public void ApplyToTest(Test test)
        {
            if (test.RunState == RunState.NotRunnable)
                return;

            var serverVersion = HazelcastServerVersionAttribute.ServerVersion;

            if (_range.Satisfies(serverVersion))
                return;

            test.RunState = RunState.Ignored;
            var reason = $"Server version {serverVersion} outside range {_range}.";
            test.Properties.Set(PropertyNames.SkipReason, reason);
        }
    }
}