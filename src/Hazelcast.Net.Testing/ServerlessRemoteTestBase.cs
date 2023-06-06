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

namespace Hazelcast.Testing;

public class ServerlessRemoteTestBase : RemoteTestBase
{
    private string _baseUrl;

    /// <summary>
    /// Gets the remote controller client.
    /// </summary>
    protected IRemoteControllerClient RcClient { get; private set; }

    /// <summary>
    /// Gets the remote controller cluster.
    /// </summary>
    protected List<CloudCluster> RcCloudClusters { get; } = new();


    [OneTimeSetUp]
    public async Task ClusterOneTimeSetUp()
    {
        // create remote client and cluster
        RcClient = await ConnectToRemoteControllerAsync().CfAwait();
        try
        {
            await RcClient.LoginCloudWithEnvironment();

            _baseUrl = Environment.GetEnvironmentVariable("BASE_URL");

            if (string.IsNullOrEmpty(_baseUrl))
                throw new ArgumentNullException("BASE_URL", "BASE_URL of the cloud must be set.");
        }
        catch (ServerException e)
        {
            // Thrift exceptions are weird and need to be "fixed"
            e.FixMessage();
            throw;
        }
    }

    /// <summary>
    /// Creates and stores cloud cluster. Stored clusters will be deleted at tear down.
    /// </summary>
    /// <param name="version">Server version.</param>
    /// <param name="tlsEnabled">Whether TLS enabled.</param>
    /// <param name="token">Cancellation Token.</param>
    /// <returns><see cref="CloudCluster"/></returns>
    /// <exception cref="Exception">Throws if cloud cluster not created.</exception>
    public async Task<CloudCluster> CreateCloudCluster(string version, bool tlsEnabled, CancellationToken token = default)
    {
        var cluster = await RcClient.CreateCloudClusterAsync(version, tlsEnabled, token);

        if (cluster is not null)
            RcCloudClusters.Add(cluster);
        else
            throw new Exception("Cloud cluster not created.");

        return cluster;
    }

    protected ValueTask<IHazelcastClient> CreateClientAsync(CloudCluster cluster, Action<HazelcastOptions> optionAction = default)
    {
        var optionsBuilder = new HazelcastOptionsBuilder()
            .With(configure =>
            {
                configure.ClusterName = cluster.Name;
                configure.Networking.Cloud.Url = new Uri(_baseUrl);
                configure.Networking.Cloud.DiscoveryToken = cluster.Token;
                if (cluster.IsTlsEnabled)
                {
                    configure.Networking.Ssl.Enabled = cluster.IsTlsEnabled;
                    configure.Networking.Ssl.CertificatePassword = cluster.TlsPassword;
                    configure.Networking.Ssl.CertificatePath = cluster.CertificatePath;
                }
                configure.Networking.ReconnectMode = ReconnectMode.ReconnectSync;
            });

        if (optionAction != default)
            optionsBuilder.With(optionAction);

        return HazelcastClientFactory.StartNewClientAsync(optionsBuilder.Build());
    }

    [OneTimeTearDown]
    public async Task ClusterOneTimeTearDown()
    {
        // terminate & remove client and cluster
        if (RcClient != null)
        {
            if (RcCloudClusters.Count > 0)
            {
                foreach (var cluster in RcCloudClusters)
                {
                    try
                    {
                        await RcClient.DeleteCloudClusterAsync(cluster.Id);
                    }
                    catch
                    {
                        // ignored since closing the workshop.
                    } 
                }
            }

            await RcClient.ExitAsync().CfAwait();
        }
    }
}
