using System;
using NUnit.Framework;

namespace Hazelcast.Testing
{
    /// <summary>
    /// Marks a test feature or test method as temporarily explicit (see <see cref="ExplicitAttribute"/>)
    /// because it fails and breaks the build, but we are aware of it an a corresponding issue has
    /// been created.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Method)]
    public class KnownIssueAttribute : ExplicitAttribute
    {
        /// <summary>
        /// Marks a test feature or test method as temporarily explicit (see <see cref="ExplicitAttribute"/>)
        /// because it fails and breaks the build, but we are aware of it an a corresponding issue has
        /// been created.
        /// </summary>
        /// <param name="issue">The issue</param>
        /// <param name="reason"></param>
        public KnownIssueAttribute(int issue, string reason = null)
            : base($"Parked (see https://github.com/hazelcast/hazelcast-csharp-client/issues/{issue}{(reason == null ? "" : (" : " + reason))})")
        { }
    }
}
