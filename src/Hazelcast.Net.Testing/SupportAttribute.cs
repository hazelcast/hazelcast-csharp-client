// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
    /// Marks a test feature or test method as explicit (see <see cref="ExplicitAttribute"/>) because it has been,
    /// or is, useful for troubleshooting (for instance, an issue) but does not need to run as part of the general
    /// tests suite.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SupportAttribute : ExplicitAttribute
    {
        /// <summary>
        /// Marks a test feature or test method as explicit (see <see cref="ExplicitAttribute"/>) because it has been,
        /// or is, useful for troubleshooting (for instance, an issue) but does not need to run as part of the general
        /// tests suite.
        /// </summary>
        public SupportAttribute()
            : base("Support test or feature, useful but explicit.")
        { }
    }
}