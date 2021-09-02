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

using System;
using NuGet.Versioning;

namespace Hazelcast.Testing.Conditions
{
    /// <summary>
    /// Provides constants for managing the server version during tests.
    /// </summary>
    public static class ServerVersion
    {
        // ReSharper disable once InconsistentNaming
        private const string DefaultVersionString = "5.0";

        /// <summary>
        /// Gets the default server version.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static readonly NuGetVersion DefaultVersion = NuGetVersion.Parse(DefaultVersionString);

        /// <summary>
        /// Gets the name of the environment variable that can be used to override the default version.
        /// </summary>
        public const string EnvironmentVariableName = "HAZELCAST_SERVER_VERSION";

        /// <summary>
        /// Gets the server version indicated by the environment variable, or the default server version.
        /// </summary>
        /// <param name="defaultVersion">The optional default version.</param>
        /// <returns>The server version.</returns>
        public static NuGetVersion GetVersion(NuGetVersion defaultVersion = null)
        {
            var env = Environment.GetEnvironmentVariable(EnvironmentVariableName);
            return NuGetVersion.TryParse(env, out var serverVersion)
                ? serverVersion
                : defaultVersion ?? DefaultVersion;
        }
    }
}
