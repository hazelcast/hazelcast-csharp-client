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

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents a service factory.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <remarks>
    /// <para>The <see cref="ServiceFactory{TService}"/> class supports defining how
    /// a service should be created, via its <see cref="Creator"/> property, and then creates
    /// new instances of that service via its <see cref="Create"/> method.</para>
    /// </remarks>
    public class ServiceFactory<TService>
        where TService : class
    {
        /// <summary>
        /// Gets or sets the service creator.
        /// </summary>
        /// <remarks>
        /// <para>Do not set the creator after a service has been created,
        /// as that could have unspecified consequences.</para>
        /// </remarks>
        public Func<TService> Creator { get; set; }

        /// <summary>
        /// Creates a new instance of the service.
        /// </summary>
        /// <returns>A new instance of the service, or null if no creator has been set.</returns>
        internal TService Create() => Creator?.Invoke();

        /// <summary>
        /// Clones this service factory.
        /// </summary>
        /// <returns>A clone of the service factory.</returns>
        internal ServiceFactory<TService> Clone()
            => new ServiceFactory<TService> { Creator = Creator };
    }
}
