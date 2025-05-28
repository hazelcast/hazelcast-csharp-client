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
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Testing.Remote;
using NuGet.Versioning;

namespace Hazelcast.Testing
{
    /// <summary>
    /// Detects the version of the server on the cluster.
    /// </summary>
    internal static class ServerVersionDetector
    {
        private static NuGetVersion _version;
        private static bool _enterprise;
        private static bool _forced;

        /// <summary>
        /// Gets the detected version of the server on the cluster.
        /// </summary>
        public static NuGetVersion DetectedServerVersion
        {
            get
            {
                if (_version != null || _forced) return _version;
                RunDetection();
                return _version;
            }
        }

        /// <summary>
        /// Whether the detected server on the cluster runs an enterprise server.
        /// </summary>
        /// <remarks>
        /// <para>This property being <c>true</c> does not automatically imply that a valid enterprise license has been provided.</para>
        /// </remarks>
        public static bool DetectedEnterprise
        {
            get
            {
                if (_version != null || _forced) return _enterprise;
                RunDetection();
                return _enterprise;
            }
        }

        private static void RunDetection()
        {
            // in the rare occasion where we haven't connected to the remote controller
            // even once, and yet someone want the version, we have to detect it here.
            // bearing in mind that that "someone" may be an attribute that CANNOT do
            // an async call - hence this property HAS to remain a synchronous thing.
            // and this is why we end up with the ugly thing below :(

            try
            {
                (_version, _enterprise) = DetectServerVersionAsync().GetAwaiter().GetResult(); // yes - see above
            }
            catch (AggregateException ae)
            {
                // this weird thing here is to avoid breaking the NUnit test runner
                if (ae.InnerExceptions.Count != 1) throw;
                ExceptionDispatchInfo.Capture(ae.InnerExceptions[0]).Throw();
            }
        }

        /// <summary>
        /// (for tests only, non thread-safe) Overrides the detected version with a <c>null</c> value..
        /// </summary>
        /// <returns>An <see cref="IDisposable"/> object that must be disposed to restore the original detected version.</returns>
        public static IDisposable ForceNoVersion()
            => ForceVersion((NuGetVersion)null);

        /// <summary>
        /// (for tests only, non thread-safe) Overrides the detected version.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <returns>An <see cref="IDisposable"/> object that must be disposed to restore the original detected version.</returns>
        public static IDisposable ForceVersion(string version)
            => ForceVersion(version == null ? null : NuGetVersion.Parse(version));

        /// <summary>
        /// (for tests only, non thread-safe) Overrides the detected version.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <returns>An <see cref="IDisposable"/> object that must be disposed to restore the original detected version.</returns>
        private static IDisposable ForceVersion(NuGetVersion version)
        {
            if (_forced) throw new InvalidOperationException("Already forcing.");
            var preserve = _version;
            _version = version;
            _forced = true;

            return new DisposeAction(() =>
            {
                _version = preserve;
                _forced = false;
            });
        }

        // this method is invoked synchronously (!) by the DetectedServerVersion property above
        private static async Task<(NuGetVersion, bool)> DetectServerVersionAsync()
        {
            IRemoteControllerClient client = null;
            try
            {
                client = await RemoteControllerClient.CreateAsync().CfAwait();
                var (ossVersion, enterpriseVersion) = await client.DetectServerVersionAsync().CfAwait();
                return (ossVersion, enterpriseVersion != null);
            }
            finally
            {
                try
                {
                    if (client != null) await client.ExitAsync().CfAwait();
                }
                catch { /* running out of options */ }
            }
        }

        // this method is invoked asynchronously by RemoteTestBase.ConnectToRemoteControllerAsync
        public static ValueTask InitializeServerVersionAsync(IRemoteControllerClient client)
        {
            static async ValueTask SetVersionAsync(IRemoteControllerClient client)
            {
                var (ossVersion, enterpriseVersion) = await client.DetectServerVersionAsync().CfAwait();
                _version = ossVersion;
                _enterprise = enterpriseVersion != null;
            }

            return _version != null || _forced
                ? default
                : SetVersionAsync(client);
        }
    }
}
