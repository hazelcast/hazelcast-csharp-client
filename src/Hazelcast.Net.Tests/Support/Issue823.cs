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

using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using System.Threading;
using System;
using Hazelcast.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.IO;
using System.Linq;
using Hazelcast.Configuration.Binding;
using Hazelcast.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Tests.Support;

[TestFixture]
public class Issue823
{
    [TearDown]
    public void TearDown()
    {
        var directory = AppContext.BaseDirectory;
        var appsettings = Path.Combine(directory, "appsettings.json");
        if (File.Exists(appsettings)) File.Delete(appsettings);
    }

    // this test was working
    //
    [Test]
    public void CanBindConfiguration()
    {
        // option files need to be in the current directory
        var directory = AppContext.BaseDirectory;
        foreach (var file in Directory.GetFiles(directory, "*.json"))
            File.Delete(file);
        var appsettings = Path.Combine(directory, "appsettings.json");
        TestFiles.Copy<Issue823>("Options/ConfigurationTests1.json", appsettings);

        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false)
            .Build();

        var configuredClusterName = "";
        foreach (var entry in configuration.AsEnumerable())
        {
            Console.WriteLine($"{entry.Key} := {entry.Value}");
            if (entry.Key == "hazelcast:clusterName") configuredClusterName = entry.Value;
        }

        // cluster name is found in the configuration
        Assert.That(configuredClusterName, Is.EqualTo("my-cluster-name"));

        var options = new HazelcastOptions();
        configuration.GetSection("hazelcast").HzBind(options);

        // cluster name is found in options
        Assert.That(options.ClusterName, Is.EqualTo("my-cluster-name"));
    }

    // this test was failing
    //
    [Test]
    public void Test()
    {
        // option files need to be in the current directory
        var directory = AppContext.BaseDirectory;
        foreach (var file in Directory.GetFiles(directory, "*.json"))
            File.Delete(file);
        var appsettings = Path.Combine(directory, "appsettings.json");
        TestFiles.Copy<Issue823>("Options/ConfigurationTests1.json", appsettings);

        // build explicit options and bare HostBuilder
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false)
            .Build();

        // NOTE
        // new HostBuilder() creates a blank host builder with *nothing* configured
        // Host.CreateDefaultBuilder() creates a host builder which will at least know about appsettings.json
        //var builder = Host.CreateDefaultBuilder()...

        IConfiguration capturedConfiguration = null;

        var builder = new HostBuilder()

            // this is not required if we provide fully initialized options
            // instead of letting the host builder deal with configuration
            //.ConfigureHazelcast()

            .ConfigureServices((hostingContext, services) =>
            {
                // if we did let the host builder deal with configuration, we'd do
                //capturedConfiguration = hostingContext.Configuration;
                //services.AddHazelcast(hostingContext.Configuration);

                capturedConfiguration = configuration;

                // verify that configuration as been read as expected
                if (configuration.AsEnumerable().All(x => x.Key != "hazelcast:clusterName" || x.Value != "my-cluster-name"))
                    throw new Exception("uh?");

                //services.AddOptions(); // AddHazelcast already does this
                services.AddHazelcast(configuration);
            });

        using var host = builder.Build();
        var services = host.Services;
        var hazelcastOptions = services.GetRequiredService<IOptions<HazelcastOptions>>().Value;

        var configuredClusterName = "";
        foreach (var entry in configuration.AsEnumerable())
        {
            Console.WriteLine($"{entry.Key} := {entry.Value}");
            if (entry.Key == "hazelcast:clusterName") configuredClusterName = entry.Value;
        }

        // cluster name is found in the configuration
        Assert.That(configuredClusterName, Is.EqualTo("my-cluster-name"));

        // the assertion below would fail before the issue was fixed

        // cluster name has been assigned to options
        Assert.That(hazelcastOptions.ClusterName, Is.EqualTo("my-cluster-name"));
    }
}