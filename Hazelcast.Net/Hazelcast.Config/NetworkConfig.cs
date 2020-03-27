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
using System.Collections.Generic;
using System.Linq;

namespace Hazelcast.Config
{
    public class NetworkConfig
    {
        public IList<string> Addresses { get; set; } = new List<string>();

        public bool SmartRouting { get; set; } = true;

        public bool RedoOperation { get; set; } = true;

        public int ConnectionTimeout { get; set; } = 60000;

        public SocketOptions SocketOptions { get; set; } = new SocketOptions();

        public SSLConfig SslConfig { get; set; } = new SSLConfig();

        public ICollection<string> OutboundPorts { get; set; } = new HashSet<string>();

        public HazelcastCloudConfig HazelcastCloudConfig { get; set; } = new HazelcastCloudConfig();

        public NetworkConfig AddAddress(params string[] addresses)
        {
            foreach (var address in addresses)
            {
                Addresses.Add(address);
            }
            return this;
        }
        public NetworkConfig ConfigureAddresses(Action<IList<string>> configAction)
        {
            configAction(Addresses);
            return this;
        }

        public NetworkConfig ConfigureOutboundPorts(Action<ICollection<string>> configAction)
        {
            configAction(OutboundPorts);
            return this;
        }

        public NetworkConfig ConfigureSocketOptions(Action<SocketOptions> configAction)
        {
            configAction(SocketOptions);
            return this;
        }

        public NetworkConfig ConfigureSSL(Action<SSLConfig> configAction)
        {
            configAction(SslConfig);
            return this;
        }

        public NetworkConfig ConfigureHazelcastCloud(Action<HazelcastCloudConfig> configAction)
        {
            configAction(HazelcastCloudConfig);
            return this;
        }
    }
}