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
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.DistributedObjects;
using Hazelcast.Examples.Models;
using Hazelcast.Query;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Examples.SoakTests
{
    // ./hz.ps1 run-example ~Soak1 --%
    //   --hazelcast:examples:soak1:ThreadCount=1             (valid integer)
    //   --hazelcast:examples:soak1:Duration="HH:MM:SS"       (valid timespan)
    //   --hazelcast:networking:addresses:0=127.0.0.1:5701
    //   --hazelcast:networking:addresses:1=127.0.0.1:5702

    // ReSharper disable once UnusedMember.Global
    public class Soak1
    {
        private static int[] _reports;

        private class SoakOptions
        {
            public int ThreadCount { get; set; } = 2;

            public string Duration { get; set; } = null;

            public int EntryCount { get; set; } = 10_000;
        }

        /// <summary>
        /// Runs the soak test.
        /// </summary>
        /// <param name="args"></param>
        /// <returns>A task that will complete when the soak test has run.</returns>
        public static async Task Main(string[] args)
        {
            // creates the example options
            var soakOptions = new SoakOptions();

            var options = new HazelcastOptionsBuilder()
                .With(args)
                .WithConsoleLogger(LogLevel.Information)
                .Bind("hazelcast:examples:soak1", soakOptions)
                .Build();

            var logger = options.LoggerFactory.Service.CreateLogger<Soak1>();

            // say hello
            logger.LogInformation("Hazelcast .NET soak tests");

            // get duration
            var duration = TimeSpan.FromMinutes(1); // default to 1 minute
            if (soakOptions.Duration.EndsWith("h"))
                duration = TimeSpan.FromHours(int.Parse(soakOptions.Duration.TrimEnd('h')));
            else if (soakOptions.Duration.EndsWith("m"))
                duration = TimeSpan.FromMinutes(int.Parse(soakOptions.Duration.TrimEnd('m')));
            else if (soakOptions.Duration.EndsWith("s"))
                duration = TimeSpan.FromSeconds(int.Parse(soakOptions.Duration.TrimEnd('s')));


            // report
            logger.LogInformation($"Start {nameof(Soak1)} with {soakOptions.ThreadCount} threads, duration {duration:G}");
            logger.LogInformation("Connecting");

            var cancellation = new CancellationTokenSource(duration);

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options, cancellation.Token).ConfigureAwait(false);

            // register hook for Ctrl-C on console
            Console.CancelKeyPress += (_, a) =>
            {
                logger.LogWarning("Aborted!");
                a.Cancel = true;
                cancellation.Cancel();
            };

            // start the threads
            _reports = new int[soakOptions.ThreadCount];
            var token = cancellation.Token;
            var tasks = new List<Task>();
            for (var i = 0; i < soakOptions.ThreadCount; i++)
            {
                var task = RunTask(client, token, i, soakOptions.EntryCount, logger);
                tasks.Add(task);
            }

            // report
            logger.LogInformation("Connected");
            logger.LogInformation($"{nameof(Soak1)} is running, will end in {soakOptions.Duration:G}");
            logger.LogInformation("Or, press Ctrl-C to abort");

            // wait for threads
            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                /* ignore */
            }

            // report
            logger.LogInformation("Stopped");

            // report
            var sb = new StringBuilder();
            sb.AppendLine("|  id |                count |");
            sb.AppendLine("|-----|----------------------|");
            for (var i = 0; i < soakOptions.ThreadCount; i++)
            {
                sb.AppendLine($"| {i,3:D} | {_reports[i],20:D} |");
            }

            logger.LogInformation(sb.ToString());

            // say hello
            logger.LogInformation($"Ended {nameof(Soak1)}");
        }

        private static async Task RunTask(IHazelcastClient client, CancellationToken token, int id, int entryCount, ILogger logger)
        {
            logger.LogInformation($"Thread {id}: start");

            IHMap<string, string> map = null;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    map = await client.GetMapAsync<string, string>("soak1").ConfigureAwait(false);
                    break;
                }
                catch (Exception e)
                {
                    logger.LogError(e.StackTrace);
                }
            }

            logger.LogInformation($"Thread {id}: acquired the map");

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var key = RandomProvider.Random.Next(0, entryCount).ToString();
                    var operation = RandomProvider.Random.Next(0, 100);
                    if (operation < 30)
                    {
                        await map.GetAsync(key).ConfigureAwait(false);
                    }
                    else if (operation < 60)
                    {
                        await map.PutAsync(key, RandomProvider.Random.Next().ToString()).ConfigureAwait(false);
                    }
                    else if (operation < 80)
                    {
                        await map.GetValuesAsync(Predicates.Value().IsBetween(0, 10)).ConfigureAwait(false);
                    }
                    else
                    {
                        await map.ExecuteAsync(new UpdateEntryProcessor(key), key).ConfigureAwait(false);
                    }

                    _reports[id] += 1;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message);
                }
            }
        }
    }
}
