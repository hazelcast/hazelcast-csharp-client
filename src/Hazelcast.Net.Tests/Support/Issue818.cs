// define SYNC_CLIENT
#define NOT_SYNC_CLIENT

// define CFAWAIT_CLIENT
#define NOT_CFAWAIT_CLIENT

// define SYNC_MAP
#define SYNC_MAP

using System;
using System.Threading.Tasks;
using Hazelcast.Networking;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Hazelcast.Tests.Support;

[TestFixture]
public class Issue818
{
    [Test]
    [Timeout(30_000)]
    public async Task Reproduce()
    {
        var options = new HazelcastOptionsBuilder()
            .With((config, options) => options.Networking.Addresses.Add("127.0.0.1"))
            .With((config, options) => options.Networking.ReconnectMode = ReconnectMode.ReconnectSync)
            .With((config, options) => options.Networking.SmartRouting = true)
            .With((config, options) => options.Networking.ShuffleAddresses = true)
            .With((config, options) => options.Networking.RedoOperations = true)
            .With(o =>
            {
                o.LoggerFactory.Creator = () => LoggerFactory.Create(loggingBuilder =>
                    loggingBuilder
                        .SetMinimumLevel(LogLevel.Trace)
                        .AddConsole());
            })
            .Build();

#if SYNC_CLIENT
        var client = HazelcastClientFactory.StartNewClientAsync(options).Result;
#else
#if CFAWAIT_CLIENT
        var client = await HazelcastClientFactory.StartNewClientAsync(options).ConfigureAwait(false);
#else
        var client = await HazelcastClientFactory.StartNewClientAsync(options);
#endif
#endif

        var mapTask = client.GetMapAsync<string, string>("map-name");

        //while (!mapTask.IsCompleted)
        //{
        //    Console.WriteLine("waiting for map");
        //}

#if SYNC_MAP
        var map = mapTask.Result;
#else
        var map = await mapTask;
#endif

        await map.PutAsync("key", "value");

        Console.WriteLine("map [\"key\"] = " + await map.GetAsync("key"));
    }
}