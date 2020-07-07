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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
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

        [OneTimeSetUp]
        public async Task BaseOneTimeSetUp()
        {
            RcClient = await CreateRemoteControllerAsync().CAF();
            RcCluster = await RcClient.CreateClusterAsync(ClusterConfiguration).CAF();
        }

        /// <summary>
        /// Gets the cluster configuration.
        /// </summary>
        protected string ClusterConfiguration => Remote.Resources.hazelcast;

        /// <summary>
        /// Gets the remote controller client.
        /// </summary>
        protected Remote.IRemoteControllerClient RcClient { get; private set; }

        /// <summary>
        /// Gets the remote controller cluster.
        /// </summary>
        protected Remote.Cluster RcCluster { get; private set; }

        [OneTimeTearDown]
        public async Task BaseOneTimeTearDown()
        {
            if (RcClient != null)
            {
                if (RcCluster != null)
                    await RcClient.ShutdownClusterAsync(RcCluster).CAF();
                await RcClient.ExitAsync().CAF();
            }
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
        /// Creates the Hazelcast options.
        /// </summary>
        protected virtual HazelcastOptions CreateHazelcastOptions()
        {
            var options = HazelcastOptions.Build();

            options.AsyncStart = false;

            options.ClusterName = RcCluster?.Id ?? options.ClusterName;

            options.Networking.Addresses.Clear();
            options.Networking.Addresses.Add("127.0.0.1:5701");

            //n.ReconnectMode = ReconnectMode.ReconnectSync;

            //var r = n.ConnectionRetry;
            //r.ClusterConnectionTimeoutMilliseconds = 60 * 1000;
            //r.InitialBackoffMilliseconds = 2 * 1000;

            return options;
        }

        protected virtual TimeSpan CreateOpenClientTimeout { get; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Creates a client.
        /// </summary>
        /// <returns>A client.</returns>
        protected virtual async ValueTask<IHazelcastClient> CreateOpenClientAsync()
        {
            Logger.LogInformation("Create new client");

            var client = new HazelcastClientFactory(CreateHazelcastOptions()).CreateClient();
            await client.OpenAsync(CreateOpenClientTimeout).CAF();
            return client;
        }

        /// <summary>
        /// Creates a client.
        /// </summary>
        /// <returns>A client.</returns>
        protected virtual async ValueTask<IHazelcastClient> CreateOpenClientAsync(Action<HazelcastOptions> configure)
        {
            Logger.LogInformation("Create new client");

            var client = new HazelcastClientFactory(CreateHazelcastOptions()).CreateClient(configure);
            await client.OpenAsync(CreateOpenClientTimeout).CAF();
            return client;
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
