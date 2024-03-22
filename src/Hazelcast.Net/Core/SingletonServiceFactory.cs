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
using System.Threading;

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
    /// <para>In a configuration file, it supports a <c>typeName</c> property which is the
    /// name of the type, and a <c>args</c> property which is a dictionary of arguments for
    /// the type constructor. For instance:
    /// <code>"service":
    /// {
    ///   "typeName": "My.Service,My.dll",
    ///   "args":
    ///   {
    ///     "foo": 33
    ///   }
    /// }</code></para>
    /// </remarks>
    public class SingletonServiceFactory<TService> : IDisposable
        where TService : class
    {
        private Lazy<TService> _lazyService;
        private Func<TService> _creator;
        private IServiceProvider _serviceProvider;

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
            if (other == null) throw new ArgumentNullException(nameof(other));

            if (shallow)
            {
                _serviceProvider = other._serviceProvider;
                _creator = other._creator;
                _lazyService = other._lazyService; // one single lazy service, shared by clones
                OwnsService = false; // owned by the original factory, not by clones
            }
            else
            {
                // create a new lazy service
                if (other._serviceProvider != null) ServiceProvider = other._serviceProvider;
                else if (other._creator != null) Creator = other._creator;
                OwnsService = other.OwnsService; // can own the service
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
                _serviceProvider = null;
                _creator = value;
                OwnsService = true;
                _lazyService = _creator == null ? null : new Lazy<TService>(_creator);
            }
        }

        /// <summary>
        /// Determines whether this service factory has been configured and can create a service.
        /// </summary>
        public bool IsConfigured => _creator != null;

        /// <summary>
        /// Gets or sets the service provider.
        /// </summary>
        /// <remarks>
        /// <para>Do not set the service provider after the service has been created,
        /// as that could have unspecified consequences.</para>
        /// </remarks>
        public IServiceProvider ServiceProvider
        {
            get => _serviceProvider;
            set
            {
                _serviceProvider = value;
                _creator = null;
                OwnsService = false;
                _lazyService = new Lazy<TService>(() => (TService) _serviceProvider.GetService(typeof(TService)) ??
                                                        throw new InvalidOperationException($"There is no service of type {typeof(TService)}."));
            }
        }

        /// <summary>
        /// Whether the factory owns the service.
        /// </summary>
        /// <remarks>
        /// <para>By default, services created via <see cref="Creator"/> are owned by the factory while
        /// services created via <see cref="ServiceProvider"/> are not, but this property can be used
        /// to force a different behavior.</para>
        /// </remarks>
        public bool OwnsService { get; set; }

        /// <summary>
        /// Gets the singleton instance of the service.
        /// </summary>
        /// <returns>The singleton instance of the service, or null if this service factory has not been configured.</returns>
        // TODO: consider throwing instead of returning null
        public virtual TService Service => _lazyService?.Value;

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
        internal SingletonServiceFactory<TService> Clone(bool shallow = true) => new SingletonServiceFactory<TService>(this, shallow);

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Frees, releases or resets managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> when invoked from <see cref="Dispose"/>; otherwise <c>false</c>.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            var lazyService = Interlocked.Exchange(ref _lazyService, null);
            if (lazyService == null) return;

            if (!OwnsService || !lazyService.IsValueCreated)
                return;

            var value = lazyService.Value;
            if (value is IDisposable disposable)
                disposable.Dispose();
        }
    }
}
