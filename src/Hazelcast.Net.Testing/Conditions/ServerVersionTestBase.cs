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
using NuGet.Versioning;
using NUnit.Framework;

namespace Hazelcast.Testing.Conditions
{
    /// <summary>
    /// Provides a base class for tests that want to behave differently depending on the server version.
    /// </summary>
    public abstract class ServerVersionTestBase
    {
        /// <summary>
        /// Gets the server version.
        /// </summary>
        protected NuGetVersion ServerVersion => Conditions.ServerVersion.GetVersion(TestContext.CurrentContext);

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
