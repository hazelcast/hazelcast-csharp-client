// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
    internal interface IDns
    {
        /// <summary>
        /// Gets the host name of the local computer.
        /// </summary>
        /// <returns>A string that contains the DNS host name of the local computer.</returns>
        string GetHostName();

        /// <summary>
        /// Resolves a host name or IP address to an <see cref="IPHostEntry"/> instance.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
        /// <returns>An <see cref="IPHostEntry"/> that contains address information about the host specified in <paramref name="hostNameOrAddress"/>.</returns>
        IPHostEntry GetHostEntry(string hostNameOrAddress);

        /// <summary>
        /// Resolves an IP address to an <see cref="IPHostEntry"/> instance.
        /// </summary>
        /// <param name="address">The IP address to resolve.</param>
        /// <returns>An <see cref="IPHostEntry"/> that contains address information about the host specified in <paramref name="address"/>.</returns>
        IPHostEntry GetHostEntry(IPAddress address);

        /// <summary>
        /// Returns the IP addresses for the specified host.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
        /// <returns>An array of <see cref="IPAddress"/> that contains the IP addresses for the host specified in <paramref name="hostNameOrAddress"/>.</returns>
        IPAddress[] GetHostAddresses(string hostNameOrAddress);
    }
}
