// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Networking;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Hazelcast.Testing
{
    /// <summary>
    /// Provides a base class for Hazelcast tests that require a remote environment.
    /// </summary>
    public abstract partial class RemoteTestBase : HazelcastTestBase
    {
        private readonly ConcurrentQueue<UnobservedTaskExceptionEventArgs> _unobservedExceptions =
            new ConcurrentQueue<UnobservedTaskExceptionEventArgs>();

        protected RemoteTestBase()
        {
            // FIXME we probably need to setup the options at some point?
            // with log level (debug) logging to console, etc

            Logger.LogInformation("LOGGER ACTIVE");

            TaskScheduler.UnobservedTaskException += UnobservedTaskException;
        }

        private void UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Logger.LogWarning("UnobservedTaskException Error sender:" + sender);
            Logger.LogWarning(e.Exception, "UnobservedTaskException Error.");
            _unobservedExceptions.Enqueue(e);
        }

        [TearDown]
        public void BaseTearDown()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            var failed = false;
            foreach (var exceptionEventArg in _unobservedExceptions)
            {
                var innerException = exceptionEventArg.Exception.Flatten().InnerException;
                Logger.LogWarning(innerException, "Exception.");
                failed = true;
            }
            if (failed)
            {
                Assert.Fail("UnobservedTaskException occured.");
            }
        }

        /// <summary>
        /// Configures the client.
        /// </summary>
        /// <param name="options">The client configuration.</param>
        protected virtual void ConfigureClient(HazelcastOptions options)
        {
            options.AsyncStart = false;

            var n = options.Networking;
            n.Addresses.Add("localhost:5701");
            n.ReconnectMode = ReconnectMode.ReconnectSync;

            var r = n.ConnectionRetry;
            r.ClusterConnectionTimeoutMilliseconds = 60 * 1000;
            r.InitialBackoffMilliseconds = 2 * 1000;
        }

        /// <summary>
        /// Creates a client.
        /// </summary>
        /// <returns>A client.</returns>
        protected virtual async ValueTask<IHazelcastClient> CreateOpenClientAsync()
            => await CreateOpenClientAsync(ConfigureClient).CAF();

        /// <summary>
        /// Creates a client.
        /// </summary>
        /// <returns>A client.</returns>
        protected virtual async ValueTask<IHazelcastClient> CreateOpenClientAsync(Action<HazelcastOptions> configure)
        {
            Logger.LogInformation("Creating new client");

            var client = new HazelcastClientFactory(HazelcastOptions.Build()).CreateClient(configure);

            try
            {
                // TODO: set a timeout on OpenAsync
                AddDisposable(client);
                await client.OpenAsync().CAF();
                return client;
            }
            catch
            {
                await client.DisposeAsync().CAF();
                RemoveDisposable(client);
                throw;
            }
        }

        protected object GenerateKeyForPartition(IHazelcastClient client, int partitionId)
        {
            var clientInternal = (HazelcastClient) client;

            while (true)
            {
                var randomKey = TestUtils.RandomString();
                var randomKeyData = clientInternal.SerializationService.ToData(randomKey);
                if (clientInternal.Cluster.Partitioner.GetPartitionId(randomKeyData) == partitionId)
                    return randomKey;
            }
        }
    }
}
