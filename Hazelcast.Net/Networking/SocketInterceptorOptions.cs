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
    public class SocketInterceptorOptions
    {
        /// <summary>
        /// Whether socket interception is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        [BinderIgnore]
        public ServiceFactory<ISocketInterceptor> SocketInterceptor { get; private set; } = new ServiceFactory<ISocketInterceptor>();

        [BinderIgnore(false)]
        private string InterceptorType
        {
            get => default;
            set => SocketInterceptor.Creator = () => Services.CreateInstance<ISocketInterceptor>(value, this);
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
        public SocketInterceptorOptions Clone()
        {
            return new SocketInterceptorOptions
            {
                IsEnabled = IsEnabled,
                SocketInterceptor = SocketInterceptor.Clone()
            };
        }
    }
}
