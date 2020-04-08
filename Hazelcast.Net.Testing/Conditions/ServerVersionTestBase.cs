using System;
using System.Linq;
using NuGet.Versioning;
using NUnit.Framework;

namespace Hazelcast.Testing.Conditions
{
    /// <summary>
    /// Provides a base class for tests that want to behave differently depending on the server version.
    /// </summary>
    public abstract class ServerVersionTestBase
    {
        private TestContext _fixtureContext;

        /// <summary>
        /// Sets the fixture up.
        /// </summary>
        [OneTimeSetUp]
        public void OneTimeSetUpBase()
        {
            _fixtureContext = TestContext.CurrentContext;
        }

        /// <summary>
        /// Gets the server version.
        /// </summary>
        protected NuGetVersion ServerVersion
        {
            get
            {
                var testProperties = TestContext.CurrentContext.Test.Properties[ServerVersionAttribute.PropertyName];
                var version = testProperties?.FirstOrDefault() as NuGetVersion;
                if (version != null) return version;

                var fixtureProperties = _fixtureContext.Test.Properties[ServerVersionAttribute.PropertyName];
                version = fixtureProperties?.FirstOrDefault() as NuGetVersion;
                if (version != null) return version;

                return Conditions.ServerVersion.GetVersion();
            }
        }

        /// <summary>
        /// Executes some test code if the server version is within a specific range.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <param name="codeInRange">The code to execute if the server version is within the specified range.</param>
        public void IfServerVersionIn(string range, TestDelegate codeInRange)
        {
            if (!VersionRange.TryParse(range, out var r))
                throw new ArgumentException("Invalid range.", nameof(range));

            if (r.Satisfies(ServerVersion))
                codeInRange();
        }

        /// <summary>
        /// Executes some test code depending on whether the server version is within a specific range or not.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <param name="codeInRange">The code to execute if the server version is within the specified range.</param>
        /// <param name="codeNotInRange">The code to execute if the server version is not within the specified range.</param>
        public void IfServerVersionIn(string range, TestDelegate codeInRange, TestDelegate codeNotInRange)
        {
            if (!VersionRange.TryParse(range, out var r))
                throw new ArgumentException("Invalid range.", nameof(range));

            if (r.Satisfies(ServerVersion))
                codeInRange();
            else
                codeNotInRange();
        }

        /// <summary>
        /// Executes some test code if the server version is not within a specific range.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <param name="codeNotInRange">The code to execute  if the server version is not within the specified range.</param>
        public void IfServerNotIn(string range, TestDelegate codeNotInRange)
        {
            if (!VersionRange.TryParse(range, out var r))
                throw new ArgumentException("Invalid range.", nameof(range));

            if (!r.Satisfies(ServerVersion))
                codeNotInRange();
        }
    }
}
