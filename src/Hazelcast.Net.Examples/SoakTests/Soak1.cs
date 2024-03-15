// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.DistributedObjects;
using Hazelcast.Examples.Models;
using Hazelcast.Query;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Examples.SoakTests
{
    // run the soak test with:
    //
    // ./hz.ps1 run-example ~Soak1
    //
    // or, with arguments:
    //
    // ./hz.ps1 run-example ~Soak1 ---
    //   --hazelcast:examples:soak1:ThreadCount=1             (valid integer)
    //   --hazelcast:examples:soak1:Duration="HH:MM:SS"       (valid timespan)
    //   --hazelcast:networking:addresses:0=127.0.0.1:5701
    //   --hazelcast:networking:addresses:1=127.0.0.1:5702
    //
    // NOTE1: if you do not pass all arguments in one unique line, but use instead multiple lines
    //  as displayed above, don't forget to terminate each line with the continuation character
    //  for your shell, which is ` (backtick) for PowerShell or \ (backslash) for bash
    // NOTE2: the --% thing is a PowerShell thing that is required for proper arguments parsing
    //
    // alternatively, to run from published examples, run with:
    //
    // ./hx.exe ~Soak1
    //   --hazelcast:examples:soak1:ThreadCount=1             (valid integer)
    //   --hazelcast:examples:soak1:Duration="HH:MM:SS"       (valid timespan)
    //   --hazelcast:networking:addresses:0=127.0.0.1:5701
    //   --hazelcast:networking:addresses:1=127.0.0.1:5702
    //
    // each server must run a 4.2+ server; start a server once by running:
    //
    // ./hz.ps1 run-server -server 4.2
    //
    // this will download all the required JARs from the proper repositories, *and*
    // a default hazelcast-<version>.xml configuration file into the build directory.
    // stop the server, and edit the build/hazelcast-<version>.xml file to add the
    // following xml fragment to the <serialization> element:
    //
    //   <data-serializable-factories>
    //     <data-serializable-factory factory-id="66">com.hazelcast.client.test.IdentifiedFactory</data-serializable-factory>
    //   </data-serializable-factories>
    //
    // restart the server


    // ReSharper disable once UnusedMember.Global
    public class Soak1
    {
        /// <summary>
        /// Operation counts by thread id
        /// </summary>
        private static int[] _reports;

        private static long _maxMemoryUsage = long.MinValue;

        private static long _minMemoryUsage = long.MaxValue;

        private static long _disconnectedCount;

        private static readonly Process _currentProcess = Process.GetCurrentProcess();

        private class SoakOptions
        {
            public int ThreadCount { get; set; } = 2;

            public string Duration { get; set; } = "1m";

            public int EntryCount { get; set; } = 10_000;

            public string MemoryMonitorPeriod { get; set; } = "5s";
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

            var duration = ParseDurationOrGetDefault(soakOptions.Duration, TimeSpan.FromMinutes(1));
            var memoryPeriod = ParseDurationOrGetDefault(soakOptions.MemoryMonitorPeriod, TimeSpan.FromSeconds(5));

            // report
            logger.LogInformation($"Start {nameof(Soak1)} with {soakOptions.ThreadCount} threads, duration {duration.TotalHours,0:f4}h");
            logger.LogInformation("Connecting");

            var cancellation = new CancellationTokenSource(duration);

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options, cancellation.Token).ConfigureAwait(false);

            //Register for events
            await client.SubscribeAsync(events => events.StateChanged((sender, args) =>
            {
                if (args.State == ClientState.Disconnected)
                    Interlocked.Increment(ref _disconnectedCount);
            }));

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

            tasks.Add(MonitorMemoryUsage(memoryPeriod, logger, token));

            // report
            logger.LogInformation("Connected");
            logger.LogInformation($"{nameof(Soak1)} is running, will end in duration {duration}");
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
            for (var i = 0; i < soakOptions.ThreadCount; i++)
            {
                logger.LogInformation($"Thread {i,3:D} = {_reports[i]} ops @ {_reports[i] / (long)duration.TotalSeconds} ops/s");
            }

            logger.LogInformation($"Min Memory Usage: {FormatMemorySize(_minMemoryUsage)}");
            logger.LogInformation($"Max Memory Usage: {FormatMemorySize(_maxMemoryUsage)}");
            logger.LogInformation($"# of Disconnects: {_disconnectedCount}");

            // say hello
            logger.LogInformation($"Ended {nameof(Soak1)}");
        }

        private static string FormatMemorySize(long value)
        {
            if (value < 4096) return $"{value}B";
            value /= 1024;
            if (value < 4096) return $"{value}KB";
            value /= 1024;
            if (value < 4096) return $"{value}MB";
            value /= 1024;
            return $"{value}GB";
        }

        private static TimeSpan ParseDurationOrGetDefault(string duration, TimeSpan defaultValue)
        {
            // get duration
            var parsedDuration = defaultValue;
            if (duration.EndsWith("h"))
                parsedDuration = TimeSpan.FromHours(int.Parse(duration.TrimEnd('h')));
            else if (duration.EndsWith("m"))
                parsedDuration = TimeSpan.FromMinutes(int.Parse(duration.TrimEnd('m')));
            else if (duration.EndsWith("s"))
                parsedDuration = TimeSpan.FromSeconds(int.Parse(duration.TrimEnd('s')));
            else
            {
                if (TimeSpan.TryParse(duration, out var parsedTimeSpan))
                    parsedDuration = parsedTimeSpan;
            }
            return parsedDuration;
        }

        private static async Task MonitorMemoryUsage(TimeSpan period, ILogger logger, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                _currentProcess.Refresh();

                var size = _currentProcess.PrivateMemorySize64;

                if (size > 0)
                {
                    _maxMemoryUsage = Math.Max(_maxMemoryUsage, size);
                    _minMemoryUsage = Math.Min(_minMemoryUsage, size);
                }

                try
                {
                    await Task.Delay(period, token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
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
                catch (Exception ex)
                {
                    logger.LogError($"Thread {id} failed to acquire the map ({ex.GetType().Name}: {ex.Message}), abort");
                    return;
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

                    if (_reports[id] % 10_000 == 0)
                        logger.LogInformation($"Thread: {id}, Operation Completed: {_reports[id]}");

                }
                catch (Exception ex)
                {
                    logger.LogError($"Thead {id} caught {ex.GetType().Name}: {ex.Message}");
                }
            }
        }
    }
}
