﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Testing.Remote;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

#if !NETFRAMEWORK
using Hazelcast.Networking;
using Hazelcast.Core;
#endif

namespace Hazelcast.Testing
{
    public abstract partial class RemoteTestBase // Remoting
    {
        private static bool _canStartRemoteController = true;

        /// <summary>
        /// Creates a remote controller.
        /// </summary>
        /// <returns>A new remote controller.</returns>
        protected
#if !NETFRAMEWORK
            async
#endif
        Task<IRemoteControllerClient> CreateRemoteControllerAsync()
        {
            // assume we can start the RC, else mark the test as inconclusive without even trying
            // so... if starting the RC fails once, we probably have a problem (is it even running?)
            // and there is no point trying again and again - faster to stop here
            Assume.That(_canStartRemoteController, Is.True, () => "Cannot start Remote Controller.");

            try
            {
#if NETFRAMEWORK
                var transport = new Thrift.Transport.TFramedTransport(new Thrift.Transport.TSocket("localhost", 9701));
                transport.Open();
                var protocol = new Thrift.Protocol.TBinaryProtocol(transport);
                return Task.FromResult(RemoteControllerClient.Create(protocol));
#else
                var rcHostAddress = NetworkAddress.GetIPAddressByName("localhost");
                var tSocketTransport = new Thrift.Transport.Client.TSocketTransport(rcHostAddress, 9701);
                var transport = new Thrift.Transport.TFramedTransport(tSocketTransport);
                if (!transport.IsOpen)
                {
                    await transport.OpenAsync().CAF();
                }
                var protocol = new Thrift.Protocol.TBinaryProtocol(transport);
                return RemoteControllerClient.Create(protocol);
#endif
            }
            catch (Exception e)
            {
                _canStartRemoteController = false; // fail fast other tests
                Logger?.LogDebug(e, "Cannot start Remote Controller");
                throw new AssertionException("Cannot start Remote Controller", e);
            }
        }
    }
}
