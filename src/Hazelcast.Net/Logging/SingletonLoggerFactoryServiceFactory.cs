// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Core;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Logging
{
    /// <summary>
    /// Represents a singleton <see cref="ILoggerFactory"/> service factory.
    /// </summary>
    /// <remarks></remarks>
    public class SingletonLoggerFactoryServiceFactory : SingletonServiceFactory<ILoggerFactory>, ILoggerFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonLoggerFactoryServiceFactory"/> class.
        /// </summary>
        internal SingletonLoggerFactoryServiceFactory() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonLoggerFactoryServiceFactory"/> class.
        /// </summary>
        /// <param name="other">Another factory.</param>
        /// <param name="shallow">Whether to shallow- or deep-clone the factory.</param>
        private SingletonLoggerFactoryServiceFactory(SingletonLoggerFactoryServiceFactory other, bool shallow) : base(other, shallow) { }

        /// <inheritdoc />
        public ILogger CreateLogger(string categoryName) => Service.CreateLogger(categoryName);

        /// <inheritdoc />
        public void AddProvider(ILoggerProvider provider) => Service.AddProvider(provider);

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
        internal new SingletonLoggerFactoryServiceFactory Clone(bool shallow = true) => new SingletonLoggerFactoryServiceFactory(this, shallow);
    }
}