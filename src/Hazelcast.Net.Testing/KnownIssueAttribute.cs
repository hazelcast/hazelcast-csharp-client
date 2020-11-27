// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
