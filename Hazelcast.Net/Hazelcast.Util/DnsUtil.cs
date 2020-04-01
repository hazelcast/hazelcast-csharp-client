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
using System.Net;

namespace Hazelcast.Util
{
    /// <summary>
    /// A simple Util class enabling fault injection for <see cref="Dns"/> method calls.
    /// </summary>
    static class DnsUtil
    {
        static Func<string> _getHostNameFunc = Dns.GetHostName;
        static Func<string, IPHostEntry> _getHostEntryFunc = Dns.GetHostEntry;
        static Func<string, IPAddress[]> _getHostAddressesFunc = Dns.GetHostAddresses;

        /// <summary>Gets the host name of the local computer.</summary>
        /// <returns>A string that contains the DNS host name of the local computer.</returns>
        /// <exception cref="T:System.Net.Sockets.SocketException">An error is encountered when resolving the local host name. </exception>
        public static string GetHostName()
        {
            return _getHostNameFunc();
        }

        /// <summary>Resolves a host name or IP address to an <see cref="T:System.Net.IPHostEntry" /> instance.</summary>
        /// <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
        /// <returns>An <see cref="T:System.Net.IPHostEntry" /> instance that contains address information about the host specified in <paramref name="hostNameOrAddress" />.</returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="hostNameOrAddress" /> parameter is <see langword="null" />. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The length of <paramref name="hostNameOrAddress" /> parameter is greater than 255 characters. </exception>
        /// <exception cref="T:System.Net.Sockets.SocketException">An error was encountered when resolving the <paramref name="hostNameOrAddress" /> parameter. </exception>
        /// <exception cref="T:System.ArgumentException">The <paramref name="hostNameOrAddress" /> parameter is an invalid IP address. </exception>
        public static IPHostEntry GetHostEntry(string hostNameOrAddress)
        {
            return _getHostEntryFunc(hostNameOrAddress);
        }

        /// <summary>Returns the Internet Protocol (IP) addresses for the specified host.</summary>
        /// <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
        /// <returns>An array of type <see cref="T:System.Net.IPAddress" /> that holds the IP addresses for the host that is specified by the <paramref name="hostNameOrAddress" /> parameter.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="hostNameOrAddress" /> is <see langword="null" />. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The length of <paramref name="hostNameOrAddress" /> is greater than 255 characters. </exception>
        /// <exception cref="T:System.Net.Sockets.SocketException">An error is encountered when resolving <paramref name="hostNameOrAddress" />. </exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="hostNameOrAddress" /> is an invalid IP address.</exception>
        public static IPAddress[] GetHostAddresses(string hostNameOrAddress)
        {
            return _getHostAddressesFunc(hostNameOrAddress);
        }
    }
}