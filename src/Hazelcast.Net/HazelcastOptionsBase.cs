// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast
{
    /// <summary>
    /// Provides a base class for both <see cref="HazelcastOptions"/> and <see cref="HazelcastFailoverOptions"/>.
    /// </summary>
    public abstract class HazelcastOptionsBase
    {
        /// <summary>
        /// Gets the configuration section name.
        /// </summary>
        internal abstract string SectionName { get; }

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/>.
        /// </summary>
        /// <remarks>
        /// <para>In dependency-injection scenario the service provider may be available,
        /// so that service factories can return injected services. In non-dependency-injection
        /// scenario, this returns <c>null</c>.</para>
        /// </remarks>
        /// <returns>The service provider.</returns>
        public virtual IServiceProvider ServiceProvider { get; internal set; }
    }
}
