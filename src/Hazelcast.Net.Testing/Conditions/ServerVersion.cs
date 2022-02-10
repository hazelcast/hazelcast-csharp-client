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
        // defines the default server version used by tests.
        // - this is overriden by the HAZELCAST_SERVER_VERSION environment variable
        // - this can be overriden on each test fixture and each test method,
        //   with a [ServerVersion(...)] attribute.
        // so, this is always ignored when running tests with hz.ps1 which sets the
        // environment variable - it only applies to, for instance, VS or Rider

        // ReSharper disable once InconsistentNaming
        private const string DefaultVersionString = "5.1";

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
        /// <returns>The server version.</returns>
        public static NuGetVersion GetVersion()
        {
            var env = Environment.GetEnvironmentVariable(EnvironmentVariableName);
            if (NuGetVersion.TryParse(env, out var envVersion)) return envVersion;

            var detectedVersion = ServerVersionDetector.DetectedServerVersion;
            if (detectedVersion != null) return detectedVersion;

            return DefaultVersion;
        }

        /// <summary>
        /// Gets the server version indicated by the environment variable, or the default server version.
        /// </summary>
        /// <param name="defaultVersion">The optional default version.</param>
        /// <returns>The server version.</returns>
        public static NuGetVersion GetVersion(NuGetVersion defaultVersion)
        {
            var env = Environment.GetEnvironmentVariable(EnvironmentVariableName);
            if (NuGetVersion.TryParse(env, out var envVersion)) return envVersion;

            var detectedVersion = ServerVersionDetector.DetectedServerVersion;
            if (detectedVersion != null) return detectedVersion;

            return defaultVersion ?? DefaultVersion;
        }

        /// <summary>
        /// Gets the server version indicated by the environment variable, or the default server version.
        /// </summary>
        /// <param name="defaultVersion">The optional default version.</param>
        /// <returns>The server version.</returns>
        public static NuGetVersion GetVersion(string defaultVersion)
        {
            var env = Environment.GetEnvironmentVariable(EnvironmentVariableName);
            if (NuGetVersion.TryParse(env, out var envVersion)) return envVersion;

            var detectedVersion = ServerVersionDetector.DetectedServerVersion;
            if (detectedVersion != null) return detectedVersion;

            return defaultVersion != null
                ? NuGetVersion.Parse(defaultVersion)
                : DefaultVersion;
        }
    }
}
