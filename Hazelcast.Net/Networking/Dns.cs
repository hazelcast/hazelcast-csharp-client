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
    /// <summary>
    /// Provides simple domain name resolution functionality.
    /// </summary>
    /// <remarks>
    /// <para>This class is just a wrapper around <see cref="System.Net.Dns"/> with entry
    /// points that allow for altering its behavior for tests (exclusively).</para>
    /// </remarks>
    public class Dns
    {
        // TODO for tests, allow overriding these methods

        public static string GetHostName() => System.Net.Dns.GetHostName();

        public static IPHostEntry GetHostEntry(string hostNameOrAddress) => System.Net.Dns.GetHostEntry(hostNameOrAddress);

        public static IPHostEntry GetHostEntry(IPAddress address) => System.Net.Dns.GetHostEntry(address);

        public static IPAddress[] GetHostAddresses(string hostNameOrAddress) => System.Net.Dns.GetHostAddresses(hostNameOrAddress);
    }
}