// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Linq;
using System.Reflection;
using NuGet.Versioning;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Hazelcast.Testing.Conditions
{
    /// <summary>
    /// Marks a class or a method as depending on the server version being within a specific versions range.
    /// </summary>
    /// <remarks>
    /// <para>If the server version condition is not met, i.e. if the specified range
    /// does not contain the current server version, then the test fixture or test
    /// method is ignored. The server version for tests is specified by the <c>HAZELCAST_SERVER_VERSION</c>
    /// environment variable, else by a <see cref="ServerVersionAttribute"/> set on (in this order)
    /// the test method, else the test fixture, else the test assembly itself.</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ServerConditionAttribute : Attribute, IApplyToTest
    {
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
        // [1.0, 2.0]   1.0 ≤ x ≤ 2.0
        // (1.0,2.0)    1.0 < x< 2.0
        // [1.0, 2.0)   1.0 ≤ x < 2.0
        // (1.0)        invalid

        /// <inheritdoc />
        public void ApplyToTest(Test test)
        {
            if (test.RunState == RunState.NotRunnable)
                return;

            var methodInfo = test.Method;
            var fixtureInfo = test.TypeInfo;

            NuGetVersion serverVersion = null;

            // check if server version is forced by an attribute on the test or the fixture
            if (methodInfo != null)
                serverVersion = methodInfo.GetCustomAttributes<ServerVersionAttribute>(true).FirstOrDefault()?.Version;
            else if (fixtureInfo != null)
                serverVersion = fixtureInfo.GetCustomAttributes<ServerVersionAttribute>(true).FirstOrDefault()?.Version;

            // otherwise, use the default mechanism
            serverVersion ??= ServerVersion.GetVersion();

            // test the range
            if (_range.Satisfies(serverVersion))
                return;

            // ignore the test if out-of-range
            test.RunState = RunState.Ignored;
            var reason = $"Server version {serverVersion} outside range {_range}.";
            test.Properties.Set(PropertyNames.SkipReason, reason);
        }
    }
}
