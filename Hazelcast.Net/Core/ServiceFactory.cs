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
    /// Represents a service factory.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    public class ServiceFactory<TService>
        where TService : class
    {
        private readonly Func<TService> _defaultFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceFactory{TService}"/> class.
        /// </summary>
        public ServiceFactory()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceFactory{TService}"/> class.
        /// </summary>
        public ServiceFactory(Func<TService> defaultFactory)
        {
            _defaultFactory = defaultFactory;
        }

        /// <summary>
        /// Gets or sets the service creator.
        /// </summary>
        public Func<TService> Creator { get; set; }

        /// <summary>
        /// Creates an instance of the service.
        /// </summary>
        /// <returns>An instance of the service.</returns>
        public TService Create() => Creator?.Invoke() ??
                                    _defaultFactory?.Invoke() ??
                                    throw new InvalidOperationException($"Cannot create an instance of {typeof(TService)}.");
    }
}