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
using System.Net;

namespace Hazelcast.Networking
{
    /// <summary>
    /// Provides simple domain name resolution functionality.
    /// </summary>
    /// <remarks>
    /// <para>This class is just a wrapper around <see cref="Dns"/> with entry
    /// points that allow for altering its behavior for tests (exclusively).</para>
    /// </remarks>
    internal static class HDns
    {
        private static IDns _implementation = new SystemNetDns();

        /// <summary>
        /// (internal for tests only)
        /// Overrides the default static implementation with a temporary alternate implementation.
        /// </summary>
        /// <param name="dns">The alternate <see cref="IDns"/> implementation.</param>
        /// <returns>An object that must be disposed in order to restore the original static implementation.</returns>
        internal static IDisposable Override(IDns dns)
        {
            var od = new OverrideDisposable();
            _implementation = dns;
            return od;
        }

        private class OverrideDisposable : IDisposable
        {
            private readonly IDns _captured;

            public OverrideDisposable()
            {
                _captured = _implementation;
            }

            public void Dispose()
            {
                _implementation = _captured;
            }
        }

        /// <summary>
        /// Gets the host name of the local computer.
        /// </summary>
        /// <returns></returns>
        public static string GetHostName() => _implementation.GetHostName();

        /// <summary>
        /// Gets the DNS host entry for a host.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or IP address.</param>
        /// <returns>The DNS host entry for the specified host.</returns>
        public static IPHostEntry GetHostEntry(string hostNameOrAddress) => _implementation.GetHostEntry(hostNameOrAddress);

        /// <summary>
        /// Gets the DNS host entry for an IP address.
        /// </summary>
        /// <param name="address">The IP address.</param>
        /// <returns>The DNS host entry for the specified IP address.</returns>
        public static IPHostEntry GetHostEntry(IPAddress address) => _implementation.GetHostEntry(address);

        /// <summary>
        /// Returns the IP addresses for a host.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
        /// <returns>The IP addresses for the specified host.</returns>
        public static IPAddress[] GetHostAddresses(string hostNameOrAddress) => _implementation.GetHostAddresses(hostNameOrAddress);
    }
}
