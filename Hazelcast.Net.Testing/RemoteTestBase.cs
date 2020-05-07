using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Aggregators;
using Hazelcast.Clustering;
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.Testing.Remote;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Hazelcast.Logging;
using Hazelcast.Partitioning.Strategies;
using Hazelcast.Predicates;
using Hazelcast.Projections;
using Hazelcast.Security;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Portable;
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

        protected RemoteTestBase()
        {
            // #if DEBUG
            // Environment.SetEnvironmentVariable("hazelcast.logging.type", "trace");

            // #else
            Environment.SetEnvironmentVariable("hazelcast.logging.type", "console");
            // #endif
            Environment.SetEnvironmentVariable("hazelcast.logging.level", "finest");

            Logger = Services.Get.LoggerFactory().CreateLogger(GetType());
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
        /// <param name="config">The client configuration.</param>
        protected virtual void ConfigureClient(ClientConfig config)
        {
            config.GetNetworkConfig().AddAddress("localhost:5701");
            var cs = config.GetConnectionStrategyConfig();
            cs.AsyncStart = false;
            cs.ReconnectMode = ReconnectMode.ON;
            cs.ConnectionRetryConfig.ClusterConnectTimeoutMillis = 60000;
            cs.ConnectionRetryConfig.InitialBackoffMillis = 2000;
        }

        /// <summary>
        /// Creates the cluster.
        /// </summary>
        /// <returns>A cluster.</returns>
        protected virtual Clustering.Cluster CreateCluster()
        {
            return new Clustering.Cluster(new Authenticator(), new List<IClusterEventSubscriber>(),  new NullLoggerFactory());
        }

        /// <summary>
        /// Creates the serialization service.
        /// </summary>
        /// <returns>A serialization service.</returns>
        protected virtual ISerializationService CreateSerializationService()
        {
            var serializerHooks = new SerializerHooks();
            serializerHooks.Add<PredicateDataSerializerHook>();
            serializerHooks.Add<AggregatorDataSerializerHook>();
            serializerHooks.Add<ProjectionDataSerializerHook>();

            return new SerializationService(
                new ByteArrayInputOutputFactory(Endianness.Native),
                1,
                new Dictionary<int, IDataSerializableFactory>(),
                new Dictionary<int, IPortableFactory>(),
                new List<IClassDefinition>(),
                serializerHooks,
                false,
                new NullPartitioningStrategy(),
                512);
        }

        /// <summary>
        /// Creates a client.
        /// </summary>
        /// <returns>A client.</returns>
        protected virtual IHazelcastClient CreateClient()
        {
            // FIXME what is the point of keeping track of all clients?

            Logger.LogInformation("Creating new client");

            // FIXME: if that's the only way to create a client, why the interface?
            var client = new HazelcastClient(ConfigureClient, CreateCluster(), CreateSerializationService(), new NullLoggerFactory());

            // FIXME there should be a 30 mins timeout on the opening
            // uh - lifecycle events - HOW??? for ClientConnected???
            client.OpenAsync().Wait(); // FIXME async oops!

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
                if (clientInternal.Cluster.Partitioner.GetPartitionId(randomKey) == partitionId)
                    return randomKey;
            }
        }
    }
}
