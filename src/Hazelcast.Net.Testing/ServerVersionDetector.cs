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
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Testing.Remote;
using NuGet.Versioning;

namespace Hazelcast.Testing
{
    internal sealed class ServerVersionDetector : RemoteTestBase
    {
        private static NuGetVersion _version;
        private static bool _forced;

        public static NuGetVersion DetectedServerVersion
        {
            get
            {
                if (_version != null || _forced) return _version;
                new ServerVersionDetector().DetectVersion();
                return _version;
            }
        }

        public static IDisposable ForceNoVersion()
            => ForceVersion((NuGetVersion)null);

        public static IDisposable ForceVersion(string version)
            => ForceVersion(version == null ? null : NuGetVersion.Parse(version));

        public static IDisposable ForceVersion(NuGetVersion version)
        {
            var preserve = _version;
            _version = version;
            _forced = true;

            return new DisposeAction(() =>
            {
                _version = preserve;
                _forced = false;
            });
        }

        // yes, that is truly ugly, async-wise, but we don't have a choice
        private void DetectVersion()
        {
            try
            {
                var client = ConnectToRemoteControllerAsync().Result;
                _version = DetectServerVersion(client).Result;
                client.ExitAsync().Wait();
            }
            catch (AggregateException ae)
            {
                // this weird thing here is to avoid breaking the NUnit test runner
                if (ae.InnerExceptions.Count != 1) throw;
                ExceptionDispatchInfo.Capture(ae.InnerExceptions[0]).Throw();
            }
        }

        public static async Task<NuGetVersion> DetectServerVersion(IRemoteControllerClient client)
        {
            if (_version != null || _forced) return _version;

            const string script = "result=com.hazelcast.instance.GeneratedBuildProperties.VERSION;";
            var response = await client.ExecuteOnControllerAsync(null, script, Lang.JAVASCRIPT).CfAwait();
            var result = response.Result;
            return result == null ? default : NuGetVersion.Parse(Encoding.UTF8.GetString(result));
        }
    }
}
