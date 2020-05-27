using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Hazelcast.Core;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Hazelcast.Networking;

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
            // #if DEBUG
            // Environment.SetEnvironmentVariable("hazelcast.logging.type", "trace");

            // #else
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");
            // #endif
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "finest");

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
        /// <param name="configuration">The client configuration.</param>
        protected virtual void ConfigureClient(HazelcastConfiguration configuration)
        {
            configuration.AsyncStart = false;

            var n = configuration.Networking;
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
        protected virtual async ValueTask<IHazelcastClient> CreateOpenClientAsync(Action<HazelcastConfiguration> configureClient)
        {
            Logger.LogInformation("Creating new client");

            var client = new HazelcastClientFactory().CreateClient(configureClient);

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
