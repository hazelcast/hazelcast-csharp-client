// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Security;
using Hazelcast.Serialization.Compact;
using Hazelcast.Testing;
using Hazelcast.Testing.Conditions;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization.Compact;

[TestFixture]
[Category("enterprise")] // security requires enterprise
[ServerCondition("[5.2,)")] // compact is n/a before 5.2
public class CompactCredentialsTests : RemoteTestBase
{
    private static string GetClusterConfiguration(bool withSerializer)
    {
        var configuration = Resources.Cluster_Default;

        // the configuration in default.xml has no <security> tag, adding
        configuration = configuration.Replace("</hazelcast>", @"
  <security enabled=""true"">
    <realms>
      <realm name=""realm"">
        <authentication>
          <jaas>
            <login-module class-name=""org.example.CustomLoginModule"" usage=""REQUIRED"">
              <properties>
                <property name=""key2"">xyz</property>
                <property name=""key1"">abc</property>
                <property name=""username"">user</property>
              </properties>
            </login-module>
          </jaas>
        </authentication>
      </realm>
    </realms>
    <client-authentication realm=""realm""/>
  </security>
</hazelcast>
");

        // the configuration in default.xml has a <serialization> tag, no <compact-serialization> tag, adding
        if (withSerializer)
            configuration = configuration.Replace("</serialization>", @"
    <compact-serialization>
      <serializers>
        <serializer>org.example.CustomCredentialsSerializer</serializer>
      </serializers>
    </compact-serialization>
  </serialization>
");

        return configuration;
    }

    [Test]
    public async Task FailsWithBareCustomCredentials()
    {
        var clusterConfiguration = GetClusterConfiguration(withSerializer: false);

        var rcClient = await ConnectToRemoteControllerAsync().CfAwait();
        var rcCluster = await rcClient.CreateClusterAsync(clusterConfiguration).CfAwait();
        var rcMember = await rcClient.StartMemberAsync(rcCluster).CfAwait();

        try
        {
            var options = CreateHazelcastOptions();
            options.ClusterName = rcCluster.Id;
            options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 1000; // fail fast
            options.Authentication.ConfigureCredentials(new CustomCredentials("user", "abc", "xyz"));

            await AssertEx.ThrowsAsync<ConnectionException>(async () => await HazelcastClientFactory.StartNewClientAsync(options));
        }
        finally
        {
            await rcClient.StopMemberAsync(rcCluster, rcMember).CfAwait();
            await rcClient.ShutdownClusterAsync(rcCluster).CfAwait();
            await rcClient.ExitAsync().CfAwait();
        }
    }

    [Test]
    public async Task SucceedsWithClusterSerializer()
    {
        //using var _ = HConsole.Capture(x => x.Configure().SetMaxLevel());

        // the cluster will use the configured compact serializer to deserialize the credentials
        var clusterConfiguration = GetClusterConfiguration(withSerializer: true);

        var rcClient = await ConnectToRemoteControllerAsync().CfAwait();
        var rcCluster = await rcClient.CreateClusterAsync(clusterConfiguration).CfAwait();
        var rcMember = await rcClient.StartMemberAsync(rcCluster).CfAwait();

        try
        {
            var options = CreateHazelcastOptions();
            options.ClusterName = rcCluster.Id;
            options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 1000; // fail fast
            options.Authentication.ConfigureCredentials(new CustomCredentials("user", "abc", "xyz"));

            // configure a compact serializer and ensure we use the type name expected by Java
            options.Serialization.Compact.AddSerializer(new CustomCredentialsCompactSerializer("org.example.CustomCredentials"));

            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
        }
        finally
        {
            await rcClient.StopMemberAsync(rcCluster, rcMember).CfAwait();
            await rcClient.ShutdownClusterAsync(rcCluster).CfAwait();
            await rcClient.ExitAsync().CfAwait();
        }
    }

    [Test]
    public async Task SucceedsWithoutClusterSerializer()
    {
        // the cluster will zero-config deserialize the credentials
        var clusterConfiguration = GetClusterConfiguration(withSerializer: false);

        var rcClient = await ConnectToRemoteControllerAsync().CfAwait();
        var rcCluster = await rcClient.CreateClusterAsync(clusterConfiguration).CfAwait();
        var rcMember = await rcClient.StartMemberAsync(rcCluster).CfAwait();

        try
        {
            var options = CreateHazelcastOptions();
            options.ClusterName = rcCluster.Id;
            options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 1000; // fail fast
            options.Authentication.ConfigureCredentials(new CustomCredentials("user", "abc", "xyz"));

            // configure a compact serializer and ensure we use the type name expected by Java
            options.Serialization.Compact.AddSerializer(new CustomCredentialsCompactSerializer("org.example.CustomCredentials"));

            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
        }
        finally
        {
            await rcClient.StopMemberAsync(rcCluster, rcMember).CfAwait();
            await rcClient.ShutdownClusterAsync(rcCluster).CfAwait();
            await rcClient.ExitAsync().CfAwait();
        }
    }

    [Test]
    public async Task FailsWithoutClusterSerializerWithoutClientSerializer()
    {
        // the cluster will zero-config deserialize the credentials
        var clusterConfiguration = GetClusterConfiguration(withSerializer: false);

        var rcClient = await ConnectToRemoteControllerAsync().CfAwait();
        var rcCluster = await rcClient.CreateClusterAsync(clusterConfiguration).CfAwait();
        var rcMember = await rcClient.StartMemberAsync(rcCluster).CfAwait();

        try
        {
            var options = CreateHazelcastOptions();
            options.ClusterName = rcCluster.Id;
            options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 1000; // fail fast
            options.Authentication.ConfigureCredentials(new CustomCredentials("user", "abc", "xyz"));

            // let the client zero-config serialize the credentials, but ensure it uses the type name expected by Java
            // note: this is specific to .NET and internal (not available for general public usage)
            options.Serialization.Compact.SetTypeName<CustomCredentials>("org.example.CustomCredentials");

            // alas - that will not work because the property names don't match the field names :(
            // in other words, we *have* to use the client-side serializer if the property and field names don't match

            //await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
            await AssertEx.ThrowsAsync<ConnectionException>(async () => await HazelcastClientFactory.StartNewClientAsync(options));
        }
        finally
        {
            await rcClient.StopMemberAsync(rcCluster, rcMember).CfAwait();
            await rcClient.ShutdownClusterAsync(rcCluster).CfAwait();
            await rcClient.ExitAsync().CfAwait();
        }
    }

    [Test]
    public async Task FailsWithCompactButInvalidCustomCredentials()
    {
        var clusterConfiguration = GetClusterConfiguration(withSerializer: true);
        Console.WriteLine(clusterConfiguration);

        var rcClient = await ConnectToRemoteControllerAsync().CfAwait();
        var rcCluster = await rcClient.CreateClusterAsync(clusterConfiguration).CfAwait();
        var rcMember = await rcClient.StartMemberAsync(rcCluster).CfAwait();

        try
        {
            var options = CreateHazelcastOptions();
            options.ClusterName = rcCluster.Id;
            options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 1000; // fail fast
            options.Authentication.ConfigureCredentials(new CustomCredentials("user", "abc", "boo"));
            options.Serialization.Compact.AddSerializer(new CustomCredentialsCompactSerializer("org.example.CustomCredentials"));

            await AssertEx.ThrowsAsync<HazelcastException>(async () => await HazelcastClientFactory.StartNewClientAsync(options));
        }
        finally
        {
            await rcClient.StopMemberAsync(rcCluster, rcMember).CfAwait();
            await rcClient.ShutdownClusterAsync(rcCluster).CfAwait();
            await rcClient.ExitAsync().CfAwait();
        }
    }

    [Test]
    public async Task SucceedsWithCompactButGenericCredentials()
    {
        // the cluster will zero-config deserialize the credentials
        // but, because the schema type name is not the Java class name, it will produce a GenericRecord
        var clusterConfiguration = GetClusterConfiguration(withSerializer: false);
        Console.WriteLine(clusterConfiguration);

        var rcClient = await ConnectToRemoteControllerAsync().CfAwait();
        var rcCluster = await rcClient.CreateClusterAsync(clusterConfiguration).CfAwait();
        var rcMember = await rcClient.StartMemberAsync(rcCluster).CfAwait();

        try
        {
            var options = CreateHazelcastOptions();
            options.ClusterName = rcCluster.Id;
            options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 1000; // fail fast
            options.Authentication.ConfigureCredentials(new CustomCredentials("user", "abc", "xyz"));
            options.Serialization.Compact.AddSerializer(new CustomCredentialsCompactSerializer("custom"));

            // works - as long as we support generic records on the server side
            // and we use the client-side serializer to produce the correct property and field names

            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
        }
        finally
        {
            await rcClient.StopMemberAsync(rcCluster, rcMember).CfAwait();
            await rcClient.ShutdownClusterAsync(rcCluster).CfAwait();
            await rcClient.ExitAsync().CfAwait();
        }
    }

    private class CustomCredentialsCompactSerializer : ICompactSerializer<CustomCredentials>
    {
        public CustomCredentialsCompactSerializer(string typeName)
        {
            TypeName = typeName;
        }

        public string TypeName { get; }

        public CustomCredentials Read(ICompactReader reader)
        {
            return new CustomCredentials(
                reader.ReadString("username"),
                reader.ReadString("key1"),
                reader.ReadString("key2")
            );
        }

        public void Write(ICompactWriter writer, CustomCredentials value)
        {
            writer.WriteString("username", value.Name);
            writer.WriteString("key1", value.Key1);
            writer.WriteString("key2", value.Key2);
        }
    }

    private class CustomCredentials : ICredentials
    {
        public CustomCredentials(string name, string key1, string key2)
        {
            Name = name;
            Key1 = key1;
            Key2 = key2;
        }

        public string Name { get; }

        public string Key1 { get; }

        public string Key2 { get; }
    }
}