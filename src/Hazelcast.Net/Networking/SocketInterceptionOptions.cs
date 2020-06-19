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

using System.Text;
using Hazelcast.Configuration.Binding;
using Hazelcast.Core;

namespace Hazelcast.Networking
{
    /// <summary>
    /// Contains the configuration for interceptor socket.
    /// </summary>
    public class SocketInterceptionOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SocketInterceptionOptions"/> class.
        /// </summary>
        public SocketInterceptionOptions()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketInterceptionOptions"/> class.
        /// </summary>
        private SocketInterceptionOptions(SocketInterceptionOptions other)
        {
            Enabled = other.Enabled;
            Interceptor = other.Interceptor.Clone();
        }

        /// <summary>
        /// Whether socket interception is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets the interceptor service factory.
        /// </summary>
        [BinderIgnore]
        public SingletonServiceFactory<ISocketInterceptor> Interceptor { get; } = new SingletonServiceFactory<ISocketInterceptor>();

        [BinderName("interceptor")]
        [BinderIgnore(false)]
#pragma warning disable IDE0051 // Remove unused private members
        // ReSharper disable once UnusedMember.Local
        private InjectionOptions InterceptorBinder
#pragma warning restore IDE0051 // Remove unused private members
        {
            get => default;
            set => Interceptor.Creator = () => ServiceFactory.CreateInstance<ISocketInterceptor>(value.TypeName, value.Args);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var text = new StringBuilder();
            text.Append("SocketInterceptorConfig");
            return text.ToString();
        }

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal SocketInterceptionOptions Clone() => new SocketInterceptionOptions(this);
    }
}
