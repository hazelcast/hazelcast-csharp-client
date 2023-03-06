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

using System.Linq;
using NuGet.Versioning;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Hazelcast.Testing.Conditions
{
    /// <summary>
    /// Provides constants for managing the server version during tests.
    /// </summary>
    public static class ServerVersion
    {
        // ReSharper disable once InconsistentNaming
        private const string DefaultVersionString = "0.0";

        /// <summary>
        /// Gets the default server version.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static readonly NuGetVersion DefaultVersion = NuGetVersion.Parse(DefaultVersionString);

        /// <summary>
        /// Gets the detected server version, or the specified default server version.
        /// </summary>
        /// <param name="defaultVersion">The optional default version.</param>
        /// <returns>The server version.</returns>
        public static NuGetVersion GetVersion(string defaultVersion)
            => GetVersion(defaultVersion == null ? null : NuGetVersion.Parse(defaultVersion));

        /// <summary>
        /// Gets the detected server version, or the specified default server version, or the default 0.0 version.
        /// </summary>
        /// <param name="defaultVersion">The optional default version.</param>
        /// <returns>The server version.</returns>
        public static NuGetVersion GetVersion(NuGetVersion defaultVersion = null)
        {
            // order is:
            // 1. use the detector, which will return non-null if it *can* detect the version
            // 2. use the supplied default version
            // 3. fallback to the hard-coded default version (but, really?)

            var detectedVersion = ServerVersionDetector.DetectedServerVersion;
            if (detectedVersion != null) return detectedVersion;

            return defaultVersion ?? DefaultVersion;
        }

        /// <summary>
        /// Determines whether the detected server is enterprise code.
        /// </summary>
        /// <returns><c>true</c> if a server is detected and reports it is an enterprise server; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>This property being <c>true</c> does not automatically imply that a valid enterprise license has been provided.</para>
        /// </remarks>
        public static bool IsEnterprise()
        {
            var detectedVersion = ServerVersionDetector.DetectedServerVersion;
            if (detectedVersion == null) return false;

            return ServerVersionDetector.DetectedEnterprise;
        }

        /// <summary>
        /// Gets the server version indicated by test attributes, or the detected server version, or the default 0.0 version.
        /// </summary>
        /// <param name="test"></param>
        /// <returns>The server version.</returns>
        public static NuGetVersion GetVersion(Test test)
        {
            var methodInfo = test.Method;
            var fixtureInfo = test.TypeInfo;

            NuGetVersion serverVersion = null;

            // check if server version is forced by an attribute on the test or the fixture
            if (methodInfo != null)
                serverVersion = methodInfo.GetCustomAttributes<ServerVersionAttribute>(true).FirstOrDefault()?.Version;
            if (serverVersion == null && fixtureInfo != null)
                serverVersion = fixtureInfo.GetCustomAttributes<ServerVersionAttribute>(true).FirstOrDefault()?.Version;

            // otherwise, use the default mechanism
            serverVersion ??= GetVersion();

            return serverVersion;
        }

        /// <summary>
        /// Gets the test context version, or the detected server version, or the default 0.0 version.
        /// </summary>
        /// <param name="context">The test context.</param>
        /// <returns>The server version.</returns>
        public static NuGetVersion GetVersion(TestContext context)
        {
            var testProperties = context.Test.Properties[ServerVersionAttribute.PropertyName];
            var version = testProperties?.FirstOrDefault() as NuGetVersion;
            return version ?? GetVersion();
        }
    }
}
