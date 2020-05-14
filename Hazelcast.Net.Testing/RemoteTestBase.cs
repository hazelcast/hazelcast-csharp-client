using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Hazelcast.Networking;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging.Abstractions;

namespace Hazelcast.Testing
{
    /// <summary>
    /// Provides a base class for Hazelcast tests that require a remote environment.
    /// </summary>
    public abstract partial class RemoteTestBase : HazelcastTestBase
    {
        private readonly ConcurrentQueue<UnobservedTaskExceptionEventArgs> _unobservedExceptions =
            new ConcurrentQueue<UnobservedTaskExceptionEventArgs>();

        private readonly ISerializationService _serializationService; // FIXME initialize!

        protected RemoteTestBase()
        {
            // #if DEBUG
            // Environment.SetEnvironmentVariable("hazelcast.logging.type", "trace");

            // #else
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");
            // #endif
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "finest");

            var loggerFactory = new NullLoggerFactory(); // TODO: replace with real logger
            Logger = loggerFactory.CreateLogger(GetType());
            Logger.LogInformation("LOGGER ACTIVE");

            TaskScheduler.UnobservedTaskException += UnobservedTaskException;
        }

        protected ILogger Logger { get; }

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

        [OneTimeTearDown]
        public void ShutdownAllClients()
        {
            // FIXME re-establish by registering clients in CreateClient?
            //HazelcastClient.ShutdownAll();
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
            => await CreateOpenClientAsync(ConfigureClient);

        /// <summary>
        /// Creates a client.
        /// </summary>
        /// <returns>A client.</returns>
        protected virtual async ValueTask<IHazelcastClient> CreateOpenClientAsync(Action<HazelcastConfiguration> configureClient)
        {
            // FIXME what is the point of keeping track of all clients?

            Logger.LogInformation("Creating new client");

            var client = new HazelcastClientFactory().CreateClient(configureClient);

            // FIXME there should be a 30 mins timeout on the opening
            // uh - lifecycle events - HOW??? for ClientConnected???
            await client.OpenAsync();

            return client;
        }

        protected async Task<int> GetUniquePartitionOwnerCountAsync(IHazelcastClient client)
        {
            //trigger partition table create
            var map = await client.GetMapAsync<object, object>("default");
            _ = await map.GetAsync(new object());

            var clientInternal = (HazelcastClient) client;
            var count = clientInternal.Cluster.Partitioner.Count;

            var owners = new HashSet<Guid>();
            for (var i = 0; i < count; i++)
            {
                var owner = clientInternal.Cluster.Partitioner.GetPartitionOwner(i);
                if (owner != default) owners.Add(owner);
            }
            return owners.Count;
        }

        protected object GenerateKeyForPartition(IHazelcastClient client, int partitionId)
        {
            var clientInternal = (HazelcastClient) client;

            while (true)
            {
                var randomKey = TestUtils.RandomString();
                var randomKeyData = _serializationService.ToData(randomKey);
                if (clientInternal.Cluster.Partitioner.GetPartitionId(randomKeyData) == partitionId)
                    return randomKey;
            }
        }
    }
}
