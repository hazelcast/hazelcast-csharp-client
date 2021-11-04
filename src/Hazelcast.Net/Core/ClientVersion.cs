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
using System;

namespace Hazelcast.Core
{
    /// <summary>
    /// Represents the version of this client.
    /// </summary>
    internal static class ClientVersion
    {
        private static string _clientVersion;
        private static string _clientVersionPure;

        /// <summary>
        /// Gets the version of this assembly.
        /// </summary>
        /// <remarks>
        /// <para>Returns the informational, SemVer compliant version ie it can be 5.1.2.preview-0+ab16ec43.</para>
        /// </remarks>
        internal static string Version
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
                    var pos = version.IndexOf('+', StringComparison.OrdinalIgnoreCase);
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

        /// <summary>
        /// Gets the "pure" version of this assembly.
        /// </summary>
        /// <remarks>
        /// <para>Returns the pure version, ie major.minor.patch.</para>
        /// </remarks>
        internal static string PureVersion
        {
            get
            {
                if (_clientVersionPure != null) return _clientVersionPure;
                var version = Version;
                var pos0 = version.IndexOf('.');
                var pos1 = version.IndexOf('.', pos0 + 1);
                if (pos1 >= 0)
                {
                    version = version[..pos1];
                }
                else
                {
                    var pos = version.IndexOf('+');
                    if (pos >= 0) version = version[..pos];
                }
                _clientVersionPure = version;
                return _clientVersionPure;
            }
        }
    }
}
