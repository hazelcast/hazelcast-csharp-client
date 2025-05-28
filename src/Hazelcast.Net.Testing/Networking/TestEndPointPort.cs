// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;

namespace Hazelcast.Testing.Networking
{
    /// <summary>
    /// Produces unique test endpoint ports.
    /// </summary>
    public static class TestEndPointPort
    {
        private const int PortBase = 11000;
        private static int _offset;

        /// <summary>
        /// Returns the next test endpoint port.
        /// </summary>
        /// <returns>The next test endpoint port.</returns>
        public static int GetNext()
        {
            while (true)
            {
                var port = PortBase + Interlocked.Increment(ref _offset);
                if (!IsListening(port)) return port;
            }
        }

        private static bool IsListening(int port)
        {
            var ipProperties = IPGlobalProperties.GetIPGlobalProperties();

            //var activeConnections = ipProperties.GetActiveTcpConnections();
            var listeners = ipProperties.GetActiveTcpListeners();
            return listeners.Any(x => x.Port == port);
        }
    }
}
