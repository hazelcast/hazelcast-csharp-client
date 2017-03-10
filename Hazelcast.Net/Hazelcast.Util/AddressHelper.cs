// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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
using System.Net;
using System.Net.Sockets;
using Hazelcast.IO;
using Hazelcast.Logging;

namespace Hazelcast.Util
{
    internal static class AddressHelper
    {
        private const int MaxPortTries = 3;

        public static ICollection<IPEndPoint> GetPossibleSocketAddresses(IPAddress ipAddress, int port,
            string scopedAddress)
        {
            var portTryCount = 1;
            if (port == -1)
            {
                portTryCount = MaxPortTries;
                port = 5701;
            }
            ICollection<IPEndPoint> socketAddresses = new List<IPEndPoint>();
            if (ipAddress == null)
            {
                for (var i = 0; i < portTryCount; i++)
                {
                    IPAddress addr;
                    if (IPAddress.TryParse(scopedAddress, out addr))
                    {
                        socketAddresses.Add(new IPEndPoint(addr, port + i));
                    }
                }
            }
            else
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    for (var i = 0; i < portTryCount; i++)
                    {
                        socketAddresses.Add(new IPEndPoint(ipAddress, port + i));
                    }
                }
                else
                {
                    var addresses = AddressUtil.GetPossibleInetAddressesFor(ipAddress);
                    foreach (var ipa in addresses)
                    {
                        for (var i = 0; i < portTryCount; i++)
                        {
                            socketAddresses.Add(new IPEndPoint(ipa, port + i));
                        }
                    }
                }
            }
            return socketAddresses;
        }

        public static ICollection<IPEndPoint> GetSocketAddresses(string address)
        {
            var addressHolder = AddressUtil.GetAddressHolder(address, -1);
            var scopedAddress = addressHolder.ScopeId != null
                ? addressHolder.Address + "%" + addressHolder.ScopeId
                : addressHolder.Address;
            IPAddress inetAddress = null;
            try
            {
                inetAddress = Address.GetAddressByName(scopedAddress);
            }
            catch (Exception)
            {
                Logger.GetLogger(typeof (AddressHelper)).Finest("Address not available");
            }
            return GetPossibleSocketAddresses(inetAddress, addressHolder.Port, scopedAddress);
        }
    }
}