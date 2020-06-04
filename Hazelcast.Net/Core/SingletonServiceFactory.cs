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

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents a singleton service factory.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    /// <remarks>
    /// <para>The <see cref="SingletonServiceFactory{TService}"/> class supports defining how
    /// a service should be created, via its <see cref="Creator"/> property, and then provides
    /// a unique instance of that service via its <see cref="Service"/> property.</para>
    /// </remarks>
    public class SingletonServiceFactory<TService>
        where TService : class
    {
        private Lazy<TService> _lazyService;
        private Func<TService> _creator;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonServiceFactory{TService}"/> class.
        /// </summary>
        public SingletonServiceFactory()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonServiceFactory{TService}"/> class.
        /// </summary>
        protected SingletonServiceFactory(SingletonServiceFactory<TService> other, bool shallow)
        {
            Creator = other.Creator;
            if (shallow)
            {
                _creator = other._creator;
                _lazyService = other._lazyService;
            }
            else
            {
                Creator = other.Creator;
            }
        }

        /// <summary>
        /// Gets or sets the service creator.
        /// </summary>
        /// <remarks>
        /// <para>Do not set the creator after the service has been created,
        /// as that could have unspecified consequences.</para>
        /// </remarks>
        public Func<TService> Creator
        {
            get => _creator;
            set
            {
                _creator = value;
                _lazyService = _creator == null ? null : new Lazy<TService>(_creator);
            }
        }

        /// <summary>
        /// Gets the singleton instance of the service.
        /// </summary>
        /// <returns>The singleton instance of the service, or null if no creator has been set.</returns>
        public TService Service => _lazyService?.Value;

        /// <summary>
        /// Clones this service factory.
        /// </summary>
        /// <remarks>
        /// <para>When cloning a singleton service factory, a shallow clones is performed
        /// by default, meaning that the (lazy) service instance is cloned too. In other
        /// words, the singleton remains a singleton across clones.</para>
        /// <para>If <paramref name="shallow"/> is set to false, a deep clone is created,
        /// which would create an entirely new singleton.</para>
        /// </remarks>
        /// <returns>A clone of the service factory.</returns>
        public SingletonServiceFactory<TService> Clone(bool shallow = true) => new SingletonServiceFactory<TService>(this, shallow);
    }
}