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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Metrics;
using Hazelcast.Networking;
using Hazelcast.Testing;
using Hazelcast.Testing.Conditions;
using Hazelcast.Testing.Configuration;
using Hazelcast.Testing.Logging;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using NUnit.Framework;

namespace Hazelcast.Tests.Cloud
{
    [TestFixture]
    [Explicit("Has special requirements, see comments in code.")]
    public class CloudTests
    {
        // REQUIREMENTS
        //
        // 1. a working Cloud environment
        //    browse to https://cloud.hazelcast.com/ to create an environment
        //    (or to one of the internal Hazelcast test clouds)
        //
        // 2. parameters for this environment, configured as Visual Studio secrets,
        //    with a specific key indicated by the following constant. The secrets
        //    file would then need to contain a section looking like:
        //      {
        //          "cloud-test": {
        //              "clusterName": "<cluster-name>",
        //              "networking": {
        //                  "cloud": {
        //                      "discoveryToken": "<token>",
        //                      "url": "<cloud-url>"
        //                  }
        //              }
        //          },
        //      }
        //
        private const string SecretsKey = "cloud-test";
        //
        // 3. a valid path to a Java JDK, indicated by the following constant
        private const string JdkPath = @"C:\Program Files\Java\jdk1.8.0_241";

        // 4. the number of put/get iterations + how long to wait between each iteration
        private const int IterationCount = 60;
        private const int IterationPauseMilliseconds = 100;

        [TestCase(true)]
        [TestCase(false)]
        public async Task SampleClient(bool previewOptions)
        {
            using var _ = HConsoleForTest();

            HConsole.WriteLine(this, "Hazelcast Cloud Client");
            var stopwatch = Stopwatch.StartNew();

            HConsole.WriteLine(this, "Build options...");
            var options = new HazelcastOptionsBuilder()
                .WithHConsoleLogger()
                .With("Logging:LogLevel:Hazelcast", "Debug")
                .WithUserSecrets(GetType().Assembly, SecretsKey)
                .Build();

            // log level must be a valid Microsoft.Extensions.Logging.LogLevel value
            //   Trace | Debug | Information | Warning | Error | Critical | None

            // enable metrics
            options.Metrics.Enabled = true;

            // enable reconnection
            if (previewOptions)
            {
                options.Preview.EnableNewReconnectOptions = true;
                options.Preview.EnableNewRetryOptions = true;
            }
            else
            {
                options.Networking.ReconnectMode = ReconnectMode.ReconnectAsync;
            }

            // instead of using Visual Studio secrets, configuration via code is
            // possible, by uncommenting some of the blocks below - however, this
            // is not recommended as it increases the risk of leaking private
            // infos in a Git repository.

            // uncomment to run on localhost
            /*
            options.Networking.Addresses.Clear();
            options.Networking.Addresses.Add("localhost:5701");
            options.ClusterName = "dev";
            */

            // uncomment to run on cloud
            /*
            options.ClusterName = "...";
            options.Networking.Cloud.DiscoveryToken = "...";
            options.Networking.Cloud.Url = new Uri("https://...");
            */

            HConsole.WriteLine(this, "Get and connect client...");
            HConsole.WriteLine(this, $"Connect to cluster \"{options.ClusterName}\"{(options.Networking.Cloud.Enabled ? " (cloud)" : "")}");
            if (options.Networking.Cloud.Enabled) HConsole.WriteLine(this, $"Cloud Discovery Url: {options.Networking.Cloud.Url}");
            var client = await HazelcastClientFactory.StartNewClientAsync(options).ConfigureAwait(false);

            HConsole.WriteLine(this, "Get map...");
            var map = await client.GetMapAsync<string, string>("map").ConfigureAwait(false);

            HConsole.WriteLine(this, "Put value into map...");
            await map.PutAsync("key", "value").ConfigureAwait(false);

            HConsole.WriteLine(this, "Get value from map...");
            var value = await map.GetAsync("key").ConfigureAwait(false);

            HConsole.WriteLine(this, "Validate value...");
            if (!value.Equals("value"))
            {
                HConsole.WriteLine(this, "Error: check your configuration.");
                return;
            }

            HConsole.WriteLine(this, "Put/Get values in/from map with random values...");
            var random = new Random();
            var step = IterationCount / 10;
            for (var i = 0; i < IterationCount; i++)
            {
                var randomValue = random.Next(100_000);
                await map.PutAsync("key_" + randomValue, "value_" + randomValue).ConfigureAwait(false);

                randomValue = random.Next(100_000);
                await map.GetAsync("key" + randomValue).ConfigureAwait(false);

                if (i % step == 0)
                {
                    HConsole.WriteLine(this, $"[{i:D3}] map size: {await map.GetSizeAsync().ConfigureAwait(false)}");
                }

                if (IterationPauseMilliseconds > 0)
                    await Task.Delay(IterationPauseMilliseconds).ConfigureAwait(false);
            }

            HConsole.WriteLine(this, "Destroy the map...");
            await map.DestroyAsync().ConfigureAwait(false);

            HConsole.WriteLine(this, "Dispose map...");
            await map.DisposeAsync().ConfigureAwait(false);

            HConsole.WriteLine(this, "Dispose client...");
            await client.DisposeAsync().ConfigureAwait(false);

            HConsole.WriteLine(this, $"Done (elapsed: {stopwatch.Elapsed.ToString("hhmmss\\.fff\\ ", CultureInfo.InvariantCulture)}).");
        }

        [Test]
        public void MetricsCompressorTests()
        {
            // compress bytes
            byte[] bytes;
            using (var compressor = new MetricsCompressor())
            {
                compressor.Append(new Metric<long>
                {
                    Descriptor = new MetricDescriptor<long>("name"),
                    Value = 42
                });
                bytes = compressor.GetBytesAndReset();
            }

            // determine solution path
            var assemblyLocation = GetType().Assembly.Location;
            var solutionPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(assemblyLocation), "../../../../.."));

            // name a temp directory
            var tempPath = Path.Combine(Path.GetTempPath(), $"hz-tests-{Guid.NewGuid():N}");

            try
            {
                // create the temp directory and copy the source files
                Directory.CreateDirectory(tempPath);
                //File.WriteAllText(Path.Combine(tempPath, "Program.java"), Resources.Java_CloudTests_Program);
                //File.WriteAllText(Path.Combine(tempPath, "TestConsumer.java"), Resources.Java_Cloudtests_TestConsumer);
                File.WriteAllText(Path.Combine(tempPath, "Program.java"), TestFiles.ReadAllText(this, "Java/CloudTests/Program.java"));
                File.WriteAllText(Path.Combine(tempPath, "TestConsumer.java"), TestFiles.ReadAllText(this, "Java/Cloudtests/TestConsumer.java"));

                // validate that we have the server JAR
                var serverJarPath = Path.Combine(solutionPath, $"temp/lib/hazelcast-{ServerVersion.GetVersion(NuGetVersion.Parse("4.0"))}.jar");
                Assert.That(File.Exists(serverJarPath), Is.True, $"Could not find JAR file {serverJarPath}");

                // compile
                Console.WriteLine("Compile...");
                Assert.That(Directory.GetFiles(tempPath, "*.java").Any(), "Could not find source files.");

                var p = Process.Start(new ProcessStartInfo(Path.Combine(JdkPath, "bin/javac.exe"), $"-cp {serverJarPath} {Path.Combine(tempPath, "*.java")}")
                    .WithRedirects(true, true, false));
                Assert.That(p, Is.Not.Null);
                p.WaitForExit();
                Console.WriteLine($"Compilation exit code: {p.ExitCode}");
                Console.WriteLine("Compilation stderr:");
                Console.WriteLine(p.StandardError.ReadToEnd());
                Console.WriteLine("Compilation stdout:");
                Console.WriteLine(p.StandardOutput.ReadToEnd());
                Assert.That(p.ExitCode, Is.Zero, "Compilation failed.");

                // execute
                Console.WriteLine("Execute...");
                Console.WriteLine($"Writing {bytes.Length} bytes to java");
                p = Process.Start(new ProcessStartInfo(Path.Combine(JdkPath, "bin/java.exe"), $"-cp {serverJarPath};{tempPath} Program")
                    .WithRedirects(true, true, true));
                Assert.That(p, Is.Not.Null);
                p.StandardInput.BaseStream.Write(bytes, 0, bytes.Length);
                p.StandardInput.Close();
                p.WaitForExit();
                Console.WriteLine($"Execution exit code: {p.ExitCode}");
                Console.WriteLine("Execution stderr:");
                Console.WriteLine(p.StandardError.ReadToEnd());
                Console.WriteLine("Execution stdout:");
                var output = p.StandardOutput.ReadToEnd();
                Console.WriteLine(output);
                Assert.That(p.ExitCode, Is.Zero, "Execution failed.");

                // assert that things were properly decompressed
                Assert.That(output.Contains("name = 42"));
            }
            finally
            {
                // get rid of the temp directory
                Directory.Delete(tempPath, true);
            }
        }

        private IDisposable HConsoleForTest()

            => HConsole.Capture(options => options
                .ClearAll()
                .Configure<HConsoleLoggerProvider>().SetPrefix("LOG").SetMaxLevel()
                .Configure().SetMinLevel().EnableTimeStamp(origin: DateTime.Now)
                .Configure(this).SetMaxLevel().SetPrefix("TEST")
            );
    }

    public static class CloudTestsExtensions
    {
        public static ProcessStartInfo WithRedirects(this ProcessStartInfo info, bool redirectOutput, bool redirectError, bool redirectInput)
        {
            info.CreateNoWindow = true;
            info.RedirectStandardOutput = redirectOutput;
            info.RedirectStandardError = redirectError;
            info.RedirectStandardInput = redirectInput;

#if !NETCOREAPP
            info.UseShellExecute = false;
#endif

            return info;
        }

        public static HazelcastOptionsBuilder WithHConsoleLogger(this HazelcastOptionsBuilder builder)
        {
            return builder
                .With("Logging:LogLevel:Default", "None")
                .With("Logging:LogLevel:System", "None")
                .With("Logging:LogLevel:Microsoft", "None")
                .With((configuration, options) =>
                {
                    // configure logging factory and add the console provider
                    options.LoggerFactory.Creator = () => LoggerFactory.Create(loggingBuilder =>
                        loggingBuilder
                            .AddConfiguration(configuration.GetSection("logging"))
                            .AddHConsole());
                });
        }
    }
}
