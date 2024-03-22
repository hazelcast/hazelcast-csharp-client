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

// This code file is heavily inspired from the .NET Runtime code, which
// is licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;

// ReSharper disable once CheckNamespace
namespace System.Net
{
    // ReSharper disable once InconsistentNaming
    internal static class IPEndPointEx
    {
        // this code is directly copied from .NET Core runtime, with minor adjustments
        // ReSharper disable all

        public static bool TryParse(string s, /*[NotNullWhen(true)]*/ out IPEndPoint result)
        {
            return TryParse(s.AsSpan(), out result);
        }

        public static bool TryParse(ReadOnlySpan<char> s, /*[NotNullWhen(true)]*/ out IPEndPoint result)
        {
            int addressLength = s.Length;  // If there's no port then send the entire string to the address parser
            int lastColonPos = s.LastIndexOf(':');

            // Look to see if this is an IPv6 address with a port.
            if (lastColonPos > 0)
            {
                if (s[lastColonPos - 1] == ']')
                {
                    addressLength = lastColonPos;
                }
                // Look to see if this is IPv4 with a port (IPv6 will have another colon)
                else if (s.Slice(0, lastColonPos).LastIndexOf(':') == -1)
                {
                    addressLength = lastColonPos;
                }
            }

#if NETSTANDARD2_1
                if (IPAddress.TryParse(s.Slice(0, addressLength), out IPAddress address))
                {
                    uint port = 0;
                    if (addressLength == s.Length ||
                        (uint.TryParse(s.Slice(addressLength + 1), NumberStyles.None, CultureInfo.InvariantCulture, out port) && port <= System.Net.IPEndPoint.MaxPort))

                    {
                        result = new IPEndPoint(address, (int)port);
                        return true;
                    }
                }
#else
            if (IPAddress.TryParse(s.Slice(0, addressLength).ToString(), out IPAddress address))
            {
                uint port = 0;
                if (addressLength == s.Length ||
                    (uint.TryParse(s.Slice(addressLength + 1).ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out port) && port <= System.Net.IPEndPoint.MaxPort))

                {
                    result = new System.Net.IPEndPoint(address, (int)port);
                    return true;
                }
            }
#endif

            result = null;
            return false;
        }

        /// <summary>
        /// Converts an IP network endpoint (address and port) represented as a string to an IPEndPoint instance.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <returns>An IP network endpoint.</returns>
        public static IPEndPoint Parse(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            return Parse(s.AsSpan());
        }

        public static IPEndPoint Parse(ReadOnlySpan<char> s)
        {
            if (TryParse(s, out IPEndPoint result))
            {
                return result;
            }

            throw new FormatException("Invalid format.");
        }

        // ReSharper restore all
    }
}
