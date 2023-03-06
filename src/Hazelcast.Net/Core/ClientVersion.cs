// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using System.Reflection;

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
        /// (for tests only)
        /// Gets the version of this assembly.
        /// Outside of testes, prefer the <see cref="Version"/> property.
        /// </summary>
        /// <param name="informationalVersionAttribute">The <see cref="AssemblyInformationalVersionAttribute"/> for this assembly.</param>
        /// <param name="versionAttribute">The <see cref="AssemblyVersionAttribute"/> for this assembly.</param>
        /// <returns></returns>
        internal static string GetVersion(AssemblyInformationalVersionAttribute informationalVersionAttribute, AssemblyVersionAttribute versionAttribute)
        {
            if (informationalVersionAttribute != null)
            {
                // AssemblyInformationalVersion attribute should contain a SemVer-compliant version.
                // the version is assumed to be compliant (not checked).
                var version = informationalVersionAttribute.InformationalVersion;
                // just make sure that the git SHA, if present, is shortened to 6 chars
                var pos = version.IndexOf('+', StringComparison.OrdinalIgnoreCase);
                if (pos > 0 && version.Length > pos + 7)
                    version = version[..(pos + 7)];
                return version;
            }
            else
            {
                // AssemblyVersion attribute should exist and contain a major.minor.patch version.
                // the version is assumed to be major.minor.patch (not checked).
                // if the attribute does not exist, go with "0.0.0".
                var version = versionAttribute?.Version;
                return string.IsNullOrWhiteSpace(version) ? "0.0.0" : version;
            }
        }

        /// <summary>
        /// (for tests only)
        /// Gets the major.minor version of a SemVer-compliant version.
        /// Outside of tests, prefer the <see cref="MajorMinorVersion"/> property.
        /// </summary>
        /// <param name="version">The SemVer-compliant version.</param>
        /// <returns>The "pure" version corresponding to the specified <paramref name="version"/>.</returns>
        internal static string GetMajorMinorVersion(string version)
        {
            var pos0 = version.IndexOf('.', StringComparison.OrdinalIgnoreCase);
            var pos1 = version.IndexOf('.', pos0 + 1);
            if (pos1 >= 0)
            {
                // two dots = major.minor.whatever
                // take everything until the second '.' = major.minor
                version = version[..pos1];
            }
            else
            {
                // one single dot = major.minor[+whatever]
                // remove the +whatever part if any
                var pos = version.IndexOf('+', StringComparison.OrdinalIgnoreCase);
                if (pos >= 0) version = version[..pos];
            }
            return version;
        }

        /// <summary>
        /// Gets the version of this assembly.
        /// </summary>
        /// <remarks>
        /// <para>Returns the informational, SemVer compliant version ie it can be 5.1.2-preview.0+ab16ec43.</para>
        /// </remarks>
        internal static string Version
        {
            get
            {
                if (_clientVersion != null) return _clientVersion;

                var type = typeof(ClientVersion);
                var assembly = type.Assembly;

                return _clientVersion = GetVersion(
                    assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>(),
                    assembly.GetCustomAttribute<AssemblyVersionAttribute>());
            }
        }

        /// <summary>
        /// Gets the major.minor version of this assembly.
        /// </summary>
        /// <remarks>
        /// <para>Returns the pure version, ie major.minor.</para>
        /// </remarks>
        internal static string MajorMinorVersion => _clientVersionPure ??= GetMajorMinorVersion(Version);
    }
}
