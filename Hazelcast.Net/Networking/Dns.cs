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
    public static class Dns
    {
        // TODO for tests, allow overriding these methods

        /// <summary>
        /// Gets the host name of the local computer.
        /// </summary>
        /// <returns></returns>
        public static string GetHostName() => System.Net.Dns.GetHostName();

        /// <summary>
        /// Gets the DNS host entry for a host.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or IP address.</param>
        /// <returns>The DNS host entry for the specified host.</returns>
        public static IPHostEntry GetHostEntry(string hostNameOrAddress) => System.Net.Dns.GetHostEntry(hostNameOrAddress);

        /// <summary>
        /// Gets the DNS host entry for an IP address.
        /// </summary>
        /// <param name="address">The IP address.</param>
        /// <returns>The DNS host entry for the specified IP address.</returns>
        public static IPHostEntry GetHostEntry(IPAddress address) => System.Net.Dns.GetHostEntry(address);

        /// <summary>
        /// Returns the IP addresses for a host.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
        /// <returns>The IP addresses for the specified host.</returns>
        public static IPAddress[] GetHostAddresses(string hostNameOrAddress) => System.Net.Dns.GetHostAddresses(hostNameOrAddress);
    }
}
