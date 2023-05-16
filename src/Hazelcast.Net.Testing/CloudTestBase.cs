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

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Networking;
using Hazelcast.Testing.Remote;
using NUnit.Framework;
using Thrift;

namespace Hazelcast.Testing;

/// <summary>
/// Provides a base class for Hazelcast tests that require a cloud remote environment.
/// </summary>
public class CloudTestBase : RemoteTestBase
{
    private string _baseUrl;

    /// <summary>
    /// Gets the remote controller client.
    /// </summary>
    protected IRemoteControllerClient RcClient { get; private set; }

    /// <summary>
    /// Gets the remote clusters.
    /// </summary>
    protected List<CloudCluster> RcCloudClusters { get; } = new();

    [OneTimeSetUp]
    public async Task CloudOneTimeSetUp()
    {
        // validate that we have some environment variables
        if (string.IsNullOrWhiteSpace(_baseUrl = Environment.GetEnvironmentVariable("BASE_URL")))
            throw new ArgumentNullException("BASE_URL", "The cloud BASE_URL environment variable is not set.");

        // create a client to the local remote controller
        RcClient = await ConnectToRemoteControllerAsync().CfAwait();

        // connect the local remote controller to cloud, using environment variables for parameters
        try
        {
            await RcClient.LoginToCloudAsync();
        }
        catch (TException e)
        {
            // ServerException, CloudException...
            // Thrift exceptions are weird and need to be "fixed"
            e.FixMessage();
            throw;
        }
    }

    /// <summary>
    /// Creates a cluster in the cloud.
    /// </summary>
    /// <param name="version">The Hazelcast version for members to run.</param>
    /// <param name="tlsEnabled">Whether TLS is enabled on the cluster.</param>
    /// <param name="token">An optional cancellation Token.</param>
    /// <returns>The cloud cluster.</returns>
    public async Task<CloudCluster> CreateCloudCluster(string version, bool tlsEnabled, CancellationToken token = default)
    {
        try
        {
            var cluster = await RcClient.CreateCloudClusterAsync(version, tlsEnabled, token)
                          ?? throw new Exception("Failed to create a cloud cluster.");

            RcCloudClusters.Add(cluster);
            return cluster;
        }
        catch (TException e)
        {
            // ServerException, CloudException...
            // Thrift exceptions are weird and need to be "fixed"
            e.FixMessage();
            throw;
        }
    }

    protected ValueTask<IHazelcastClient> CreateAndStartClientAsync(CloudCluster cluster, Action<HazelcastOptions> configure)
    {
        return base.CreateAndStartClientAsync(options =>
        {
            options.ClusterName = cluster.ReleaseName;
            options.Networking.Cloud.Url = new Uri(_baseUrl);
            options.Networking.Cloud.DiscoveryToken = cluster.Token;
            options.Networking.Addresses.Clear();
            options.Networking.ReconnectMode = ReconnectMode.ReconnectSync;
            options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 30_000;

            if (cluster.IsTlsEnabled)
            {
                options.Networking.Ssl.Enabled = cluster.IsTlsEnabled;
                options.Networking.Ssl.CertificatePassword = cluster.TlsPassword;
                options.Networking.Ssl.CertificatePath = cluster.CertificatePath;
            }

            configure?.Invoke(options);
        });
    }
    
    [TearDown]
    public async Task CloudTearDown()
    {
        // terminate all clusters
        foreach (var cluster in RcCloudClusters)
            await RcClient.DeleteCloudClusterAsync(cluster.Id).CfAwaitNoThrow();
        RcCloudClusters.Clear();
    }

    [OneTimeTearDown]
    public async Task CloudOneTimeTearDown()
    {
        // terminate the client
        if (RcClient != null) await RcClient.ExitAsync().CfAwaitNoThrow();
    }
}
