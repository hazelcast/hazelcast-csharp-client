// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Exceptions;
using Hazelcast.Networking;
using Microsoft.Extensions.Logging;

// ReSharper disable MethodSupportsCancellation - meh

namespace Hazelcast.Examples.SoakTests
{
    // FIXME this is temporary code while troubleshooting
    public class KubeTemp
    {
        private static bool _cancelKeyInstalled;
        private static bool _cancelKeyPressed;

        private static bool CancelKeyPressed
        {
            get
            {
                if (!_cancelKeyInstalled)
                {
                    Console.WriteLine("Press ^C to abort.");
                    _cancelKeyPressed = false;
                    _cancelKeyInstalled = true;
                    Console.CancelKeyPress += (_, a) =>
                    {
                        Console.WriteLine("^C!");
                        a.Cancel = true;
                        _cancelKeyPressed = true;
                    };
                }

                if (!_cancelKeyPressed) return false;
                _cancelKeyPressed = false;
                return true;
            }
        }

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Started");
            var options = new HazelcastOptionsBuilder().Build();
            Console.WriteLine("Reconnect mode is set");

            //options.Networking.Addresses.Add("hz-hazelcast");
            options.Networking.Addresses.Add("192.168.1.200:5701");

            // this must be consistent with what's in the GitHub action: if the action waits for 120s before testing
            // the log, then we must set an invocation retry timeout greater than 120s, else the invocations will
            // start and exceptions *will* be reported - all in all I am not 100% sure these tests are correct
            options.Messaging.RetryTimeoutSeconds = 180;

            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
            await using var map = await client.GetMapAsync<string, string>("map");
            await map.PutAsync("key", "value");
            var value = await map.GetAsync("key");
            if (value == "value")
            {
                Console.WriteLine("Successful connection!");
            }
            else
            {
                throw new Exception("Connection failed, check your configuration.");
            }
            Console.WriteLine("Starting to fill the map with random entries.");
            var random = new Random();
            var i = 0;
            while (!CancelKeyPressed)
            {
                var randomKey = random.Next(100_000);

                try
                {
                    await map.PutAsync("key" + randomKey, "value" + randomKey);

                    if (i++ % 20 == 0) Console.WriteLine("Current map size: {0}", await map.GetSizeAsync());
                }
                catch (ClientOfflineException e)
                {
                    if (i++ % 20 == 0) Console.WriteLine($"{e.GetType()} - State: {e.State}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.GetType()}: {e.Message} - State: {client.State}");
                }

                await Task.Delay(100);
            }
        }
    }

    // ReSharper disable once UnusedMember.Global
    public class Kube
    {
        private class KubeCtrl
        {
            private const string SshExe = @"C:\Windows\System32\OpenSSH\ssh.exe";

            private readonly ILogger _logger;
            private readonly string _host;
            private int _count;
            private bool _counted;

            public KubeCtrl(string host, ILoggerFactory loggerFactory)
            {
                _host = host;
                _logger = loggerFactory.CreateLogger<KubeCtrl>();
            }

            private class ProcessResult
            {
                // ReSharper disable UnusedAutoPropertyAccessor.Local - meh
                public int ExitCode { get; set; }
                public string ErrorOutput { get; set; }
                public string StandardOutput { get; set; }
                // ReSharper restore UnusedAutoPropertyAccessor.Local
            }

            private async Task<ProcessResult> Execute(string command)
            {
                var exited = new TaskCompletionSource<int>();
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo(SshExe, _host + " " + command)
                    {
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    },
                    EnableRaisingEvents = true
                };
                process.Exited += (sender, args) =>
                {
                    // ReSharper disable once AccessToDisposedClosure - but will be disposed only after we've accessed it
                    exited.TrySetResult(process.ExitCode);
                };
                process.Start();
                var rc = await exited.Task;
                var error = await process.StandardError.ReadToEndAsync();
                var output = await process.StandardOutput.ReadToEndAsync();
                process.Dispose();
                return new ProcessResult { ExitCode = rc, StandardOutput = output, ErrorOutput = error };
            }

            public async Task<int> CountMembers()
            {
                _logger.LogInformation("Count members.");
                var result = await Execute("cd shared/kube; ./member.sh | grep hz-hazelcast- | wc -l");
                _count = int.Parse(result.StandardOutput);
                _logger.LogInformation($"Counted {_count} member{(_count>1?"s":"")}.");
                _counted = true;
                return _count;
            }

            public async Task StartMember()
            {
                if (!_counted) await CountMembers();
                _logger.LogInformation($"Start member {_count}.");
                _ = await Execute($"cd shared/kube; ./member.sh start {_count}");
                _logger.LogInformation($"Member {_count} has started.");
                _count++;
            }

            public async Task StopMember()
            {
                if (!_counted) await CountMembers();
                if (_count == 0) return;
                _count--;
                _logger.LogInformation($"Stop member {_count}.");
                _ = await Execute($"cd shared/kube; ./member.sh stop {_count}");
                _logger.LogInformation($"Member {_count} has stopped.");
            }
        }

        private static async Task CancellableDelay(TimeSpan delay, CancellationToken token = default)
        {
            try
            {
                await Task.Delay(delay, token);
            }
            catch (OperationCanceledException)
            { }
        }

        private static async Task WaitWhile(Func<bool> condition, TimeSpan delay, CancellationToken token = default)
        {
            while (!token.IsCancellationRequested && condition()) await CancellableDelay(delay, token);
        }

        private static async Task LogClientState(IHazelcastClient client, ILogger logger, TimeSpan delay, CancellationToken token = default)
        {
            var msg0 = "";
            do
            {
                // NOTE that there is a race condition here between client.State, client.IsConnected and client.Members, but
                // we are not going to introduce more locks in the client just for the sake of these tests

                var msg = $"Client: {client.State} {(client.IsActive ? "+" : "-")}active {(client.IsConnected ? "+" : "-")}connected "
                          + string.Join(",", client.Members.Select(x => $"{x.Member.PublicAddress}{(x.IsConnected ? "*" : "")}"));
                if (msg != msg0)
                    logger.LogInformation(msg0 = msg);

                await CancellableDelay(delay, token);
            } while (!token.IsCancellationRequested);
        }

        private static bool _mapWorks;

        private static async Task DoSomeWork(IHazelcastClient client, ILogger logger, TimeSpan delay, CancellationToken token)
        {
            // NOTE that regardless of ConfigureAwait(false) the map operations below may seem to
            // hand if the cluster is currently managing the loss of a member (i.e. if a member pod
            // was just deleted) because the *member* does not respond because it's presumably
            // dealing with the situation - nothing we can do nor need to fix at .NET level

            do
            {
                try
                {
                    await using var map = await client.GetMapAsync<string, int>("test-map").ConfigureAwait(false);

                    await map.SetAsync("key", 42).ConfigureAwait(false);
                    var value = await map.GetAsync("key").ConfigureAwait(false);
                    if (value != 42) logger.LogError("Map read error.");

                    logger.LogDebug("Map worked.");
                    _mapWorks = true;
                }
                catch (ClientOfflineException)
                {
                    logger.LogWarning("Map error: offline.");
                    _mapWorks = false;
                }
                catch (TargetDisconnectedException)
                {
                    logger.LogWarning("Map error: disconnected.");
                    _mapWorks = false;
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Map error.");
                    _mapWorks = false;
                }

                await CancellableDelay(delay, token);
            } while (!token.IsCancellationRequested);
            _mapWorks = false;
        }

        private static readonly Dictionary<ClientState, List<TaskCompletionSource<object>>> StateChangeWaiters = new Dictionary<ClientState, List<TaskCompletionSource<object>>>();

        private static Task WaitForClientState(IHazelcastClient client, ClientState state)
        {
            lock (StateChangeWaiters)
            {
                if (client.State == state) return Task.CompletedTask;

                var source = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
                if (!StateChangeWaiters.TryGetValue(state, out var sources)) StateChangeWaiters[state] = sources = new List<TaskCompletionSource<object>>();
                sources.Add(source);
                return source.Task;
            }
        }

        private static void OnStateChanged(ClientState state)
        {
            lock (StateChangeWaiters)
            {
                if (StateChangeWaiters.TryGetValue(state, out var sources))
                {
                    StateChangeWaiters.Remove(state);
                    foreach (var source in sources) source.TrySetResult(null);
                }
            }
        }

        public static async Task Main(string[] args)
        {
            ILogger logger = null;

            // build options
            var exampleOptions = new ExampleOptions();

            var options = new HazelcastOptionsBuilder()
                .Bind("hazelcast:example", exampleOptions)
                .With(args)
                .WithConsoleLogger()
                .With(o =>
                {
                    o.AddSubscriber(events => events
                        .StateChanged((c, a) =>
                        {
                            logger.LogDebug($"StateChanged! {a.State}");
                            OnStateChanged(a.State);
                        })
                        .MembersUpdated((c, a) =>
                        {
                            // note: these are declared members, regardless of whether they are connected
                            logger.LogDebug($"MembersUpdated! {string.Join(",", a.Members.Select(x => x.PublicAddress))}");
                        }));
                })
                .Build();

            // obtain a logger even before the client exists
            logger = options.LoggerFactory.CreateLogger<Kube>();
            logger.LogInformation("Begin.");

            // initialize the Kubernetes controller
            // ensure we don't have members (else the whole example fails)
            var kc = new KubeCtrl("sgay-l4", options.LoggerFactory);
            if (await kc.CountMembers() != 0) throw new Exception("Expected ZERO members.");

            // create and start a client
            // if we were to await for the client to start, we would need to ensure that member exists first
            // instead, we are going to async-start the client below
            //await using var client = await HazelcastClientFactory.StartNewClientAsync(options); // disposed when method exits

            // instead, get a starting client
            var clientStart = HazelcastClientFactory.GetNewStartingClient(options);
            await using var client = clientStart.Client; // so that the client is disposed when the method exits

            // create the cancellation + cancel it when the client shuts down
            var cancel = new CancellationTokenSource();
            var cancelling = WaitForClientState(client, ClientState.Shutdown).ContinueWith(_ =>
            {
                logger.LogDebug("State is Shutdown, cancel tasks.");
                cancel.Cancel();
            });

            // introduce some delay...
            await CancellableDelay(TimeSpan.FromSeconds(2), cancel.Token);

            // start the background logging of the client state
            var logging = LogClientState(client, logger, TimeSpan.FromMilliseconds(500), cancel.Token);

            // start some background work on a map
            var working = DoSomeWork(client, logger, TimeSpan.FromMilliseconds(2000), cancel.Token);

            // start a first member
            await kc.StartMember();

            // introduce some delay...
            await CancellableDelay(TimeSpan.FromSeconds(2), cancel.Token);

            // an HazelcastClientStart.Task cannot be entirely "fire and forget", it's important to do something
            // about it - here, we observe its completion and make sure we handle and log the exception, if
            // any
            _ = clientStart.Task.ContinueWith(t =>
            {
                if (t.Status == TaskStatus.RanToCompletion)
                    logger.LogDebug($"StartingClient {t.Status}");
                else
                    logger.LogError(t.Exception, $"StartingClient {t.Status}"); // observe!
            });

            // start a second member
            await kc.StartMember();

            // wait until notified about the second member being added, *and* connected to it
            await WaitWhile(() => client.Members.Count(x => x.IsConnected) < 2, TimeSpan.FromMilliseconds(1000), cancel.Token);
            await WaitWhile(() => !_mapWorks, TimeSpan.FromMilliseconds(2000), cancel.Token);

            // stop the second member
            await kc.StopMember();

            // wait until disconnected from the second member
            await WaitWhile(() => client.Members.Count(x => x.IsConnected) == 2, TimeSpan.FromMilliseconds(1000), cancel.Token);
            await WaitWhile(() => !_mapWorks, TimeSpan.FromMilliseconds(2000), cancel.Token);

            // wait until notified about the second member being removed
            // note: can take ages for the first member to figure it out
            await WaitWhile(() => client.Members.Count() == 2, TimeSpan.FromMilliseconds(1000), cancel.Token);
            await WaitWhile(() => !_mapWorks, TimeSpan.FromMilliseconds(2000), cancel.Token);

            // stop the first member
            await kc.StopMember();

            // wait until disconnected from the first member
            await WaitWhile(() => client.Members.Count(x => x.IsConnected) == 1, TimeSpan.FromMilliseconds(1000), cancel.Token);
            await WaitWhile(() => _mapWorks, TimeSpan.FromMilliseconds(2000), cancel.Token);

            // wait until notified about the first member being removed
            // NOT! no more members = no more events!
            //while (client.Members2.Count() == 1) await Task.Delay(1000);

            // await the tasks
            await cancelling;
            await logging;
            await working;

            // beware of options! this works only if ReconnectMode=Shutdown!

            // end
            logger.LogInformation("End.");
        }

        public class ExampleOptions
        {
            public TimeSpan PauseDuration { get; set; } = TimeSpan.Zero;

            public int RunCount { get; set; } = 1;
        }
    }
}
