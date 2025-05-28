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
        private static string _clientMajorMinorPatchVersion;

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
        /// Returns the SemVer-compliant version without the build metadata.
        /// Ex: 5.1.2-preview.0+ab16ec43 -> 5.1.2-preview.0
        /// </summary>
        /// /// <param name="version">The SemVer-compliant version.</param>
        internal static string GetSemVerWithoutBuildingMetadata(string version)
        {
            var pos = version.IndexOf('+', StringComparison.OrdinalIgnoreCase);
            return pos > 0 ? version[..pos] : version;
        }
        
        /// <summary>
        /// Returns the SemVer-compliant version without the build metadata.
        /// </summary>
        internal static string GetSemVerWithoutBuildingMetadata()
        {
            return GetSemVerWithoutBuildingMetadata(Version);
        }
        
        /// <summary>
        /// (for tests only)
        /// Gets the major.minor version of a SemVer-compliant version.
        /// Outside of tests, prefer the <see cref="MajorMinorPatchMajorMinorPatchVersion"/> property.
        /// </summary>
        /// <param name="version">The SemVer-compliant version.</param>
        /// <returns>The "pure" version corresponding to the specified <paramref name="version"/>.</returns>
        internal static string GetMajorMinorPatchVersion(string version)
        {
            // one single dot = major.minor[+whatever]
            // remove the +whatever part if any
            var pos = version.IndexOf('+', StringComparison.OrdinalIgnoreCase);
            if (pos >= 0) version = version[..pos];
            
            var pos0 = version.IndexOf('.', StringComparison.OrdinalIgnoreCase);
            var pos1 = version.IndexOf('.', pos0 + 1);
            var pos2 = -1;
            
            // Check the patch version segment, expecting max 2 digits
            if (pos1 + 1 < version.Length && int.TryParse(version[pos1 + 1] + "", out _))
            {
                pos2 = pos1 + 1; 

                if (pos2 + 1 < version.Length && int.TryParse(version[pos2 + 1] + "", out _))
                {
                    pos2 = pos2 + 1;
                }

                pos2++; // because splitting excludes the last index
            }

            if (pos2 >= 0)
            {
                // two dots = major.minor.patch
                // take everything until the second '.' = major.minor.patch
                version = version[..pos2];
            }
            else if (pos1 >= 0)
            {
                // major.minor
                version = version[..pos1];
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
        internal static string MajorMinorPatchVersion
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_clientMajorMinorPatchVersion)) return _clientMajorMinorPatchVersion;
                _clientMajorMinorPatchVersion = GetMajorMinorPatchVersion(GetSemVerWithoutBuildingMetadata());
                return _clientMajorMinorPatchVersion;
            }
        }
    }
}
