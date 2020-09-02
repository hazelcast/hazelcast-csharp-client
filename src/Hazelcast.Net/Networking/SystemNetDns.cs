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

using System.Net;

namespace Hazelcast.Networking
{
    internal sealed class SystemNetDns : IDns
    {
        public string GetHostName() => Dns.GetHostName();

        public IPHostEntry GetHostEntry(string hostNameOrAddress) => Dns.GetHostEntry(hostNameOrAddress);

        public IPHostEntry GetHostEntry(IPAddress address) => Dns.GetHostEntry(address);

        public IPAddress[] GetHostAddresses(string hostNameOrAddress) => Dns.GetHostAddresses(hostNameOrAddress);
    }
}