// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

using System.Reflection;

#if NETSTANDARD2_1
using System;
#endif

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents the version of this client.
    /// </summary>
    internal static class ClientVersion
    {
        private static string _clientVersion;

        internal static string Version // FIXME should this be a string?!
        {
            get
            {
                if (_clientVersion != null) return _clientVersion;

                var type = typeof (ClientVersion);
                var assembly = type.Assembly;
                var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                if (attribute != null)
                {
                    var version = attribute.InformationalVersion;
#if NETSTANDARD2_1
                    var pos = version.IndexOf('+', StringComparison.OrdinalIgnoreCase);
#else
                    var pos = version.IndexOf('+');
#endif
                    if (pos > 0 && version.Length > pos + 7)
                        version = version.Substring(0, pos + 7);
                    _clientVersion = version;
                }
                else
                {
                    var v = assembly.GetCustomAttribute<AssemblyVersionAttribute>();
                    _clientVersion = v != null ? v.Version : "0.0.0";
                }

                return _clientVersion;
            }
        }
    }
}
