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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Networking;
using Hazelcast.Testing.Remote;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Thrift;

namespace Hazelcast.Testing
{
    public abstract partial class RemoteTestBase // Remoting
    {
        private static bool _canConnectToRemoveController = true;

        /// <summary>
        /// Connects to the remote controller.
        /// </summary>
        /// <returns>A new remote controller client.</returns>
        protected async Task<IRemoteControllerClient> ConnectToRemoteControllerAsync()
        {
            // assume we can start the RC, else mark the test as inconclusive without even trying
            // so... if starting the RC fails once, we probably have a problem (is it even running?)
            // and there is no point trying again and again - faster to stop here
            Assume.That(_canConnectToRemoveController, Is.True, () => "Cannot connect to the Remote Controller (is it running?).");

            // note: it is because we use ASSUME and not ASSERT that the not-attempted tests show as inconclusive

            try
            {
                var rcHostAddress = NetworkAddress.GetIPAddressByName("localhost");
                var configuration = new TConfiguration();
                var tSocketTransport = new Thrift.Transport.Client.TSocketTransport(rcHostAddress, 9701, configuration);
                var transport = new Thrift.Transport.TFramedTransport(tSocketTransport);
                if (!transport.IsOpen)
                {
                    await transport.OpenAsync(CancellationToken.None).CfAwait();
                }
                var protocol = new Thrift.Protocol.TBinaryProtocol(transport);
                var client = RemoteControllerClient.Create(protocol);
                await ServerVersionDetector.DetectServerVersion(client).CfAwait(); // static, will detect only once
                return client;
            }
            catch (Exception e)
            {
                _canConnectToRemoveController = false; // fail fast other tests
                Logger?.LogDebug(e, "Cannot connect to the Remote Controller (is it running?)");
                throw new AssertionException("Cannot connect to the Remote Controller (is it running?)", e);
            }
        }
    }
}
