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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Metrics;
using Hazelcast.Networking;
using Hazelcast.Testing;
using Hazelcast.Testing.Conditions;
using Hazelcast.Testing.Configuration;
using Hazelcast.Testing.Logging;
using Ionic.Zlib;
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

        [SetUp]
        public void SetUp()
        {
            Assert.That(Directory.Exists(JdkPath), Is.True, $"JDK directory {JdkPath} does not exist.");
        }

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
        public void ZLibTest()
        {
            // this tests that a zipped blob of text can be unzipped
            const string sourceString = "this is a test";

            // compress the string
            var sourceBytes = Encoding.UTF8.GetBytes(sourceString);
            var sourceStream = new MemoryStream(sourceBytes);
            var compressStream = new ZlibStream(sourceStream, CompressionMode.Compress, CompressionLevel.BestSpeed,false);
            var compressedStream = new MemoryStream();
            compressStream.CopyTo(compressedStream);
            var compressedBytes = compressedStream.ToArray();

            // dump the compressed bytes to console
            Console.WriteLine(compressedBytes.Dump(formatted: false));

            // decompress
            compressedStream = new MemoryStream(compressedBytes);
            var uncompressStream = new ZlibStream(compressedStream, CompressionMode.Decompress, false);
            var destStream = new MemoryStream();
            uncompressStream.CopyTo(destStream);
            var destBytes = destStream.ToArray();
            var destString = Encoding.UTF8.GetString(destBytes);

            // validate
            Assert.That(destString, Is.EqualTo(sourceString));
        }

        [Test]
        public void MetricsCompress()
        {
            // compress some metrics
            var compressor = new MetricsCompressor();
            compressor.Append(MetricDescriptor.Create<int>("name1", MetricUnit.Count).WithValue(1234));
            compressor.Append(MetricDescriptor.Create<int>("name2", MetricUnit.Count).WithValue(5678));
            var bytes = compressor.GetBytesAndReset();

            // dump the compressed bytes to console
            Console.WriteLine(bytes.Dump(formatted: false));

            // get the metrics back
            var metrics = MetricsDecompressor.GetMetrics(bytes);
            Assert.That(metrics.Count(), Is.EqualTo(2));
        }

        private static class MetricsDecompressor
        {
            private static string _prefix;
            private static string _name;
            private static string _discname;
            private static string _discvalue;
            private static MetricUnit _unit;

            // note: BytesExtensions ReadInt() etc methods are highly optimized and assume that boundary
            // checks have been properly performed beforehand - which means that if they fail, they can
            // take the entire process down - so here we protect them with CanRead() calls which we would
            // not use in the actual code - so we get a "normal" exception

            private static byte[] Decompress(byte[] bytes)
            {
                try
                {
                    using var memory = new MemoryStream(bytes);
                    using var uncompressing = new ZlibStream(memory, CompressionMode.Decompress, false);
                    var u = new MemoryStream();
                    uncompressing.CopyTo(u);
                    return u.ToArray();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to decompress!");
                    Console.WriteLine(e);
                    return null;
                }
            }

            private static Dictionary<int, string> GetStrings(byte[] stringsBytes, bool verbose)
            {
                var stringsCount = stringsBytes.ReadInt(0, Endianness.BigEndian);
                if (verbose) Console.WriteLine($"Containing {stringsCount} strings");
                var strings = new Dictionary<int, string>();
                var pos = 4;
                char[] pchars = null;
                for (var i = 0; i < stringsCount; i++)
                {
                    var stringId = stringsBytes.CanRead(pos, BytesExtensions.SizeOfInt).ReadInt(pos, Endianness.BigEndian);
                    pos += 4;
                    var commonLen = stringsBytes.ReadByte(pos++);
                    var diffLen = stringsBytes.ReadByte(pos++);
                    var chars = new char[commonLen + diffLen];
                    for (var j = 0; j < commonLen; j++)
                    {
                        chars[j] = pchars[j];
                    }
                    for (var j = commonLen; j < commonLen + diffLen; j++)
                    {
                        chars[j] = stringsBytes.CanRead(pos, BytesExtensions.SizeOfChar).ReadChar(pos, Endianness.BigEndian);
                        pos += 2;
                    }
                    pchars = chars;
                    strings[stringId] = new string(chars);
                    if (verbose) Console.WriteLine($"s[{stringId:000}]={strings[stringId]}");
                }
                return strings;
            }

            private static string GetString(Dictionary<int, string> strings, int id)
            {
                if (id < 0) return null;
                if (strings.TryGetValue(id, out var value)) return value;
                return $"<err:{id}>";
            }

            private static Metric GetMetric(byte[] metricsBytes, ref int pos, Dictionary<int, string> strings, bool verbose)
            {
                var mask = (MetricsCompressor.DescriptorMask)metricsBytes.ReadByte(pos);
                if (verbose) Console.WriteLine($"  Mask: 0x{(byte)mask:x2} -> {mask}");
                pos += 1;

                var prefix = _prefix;
                if (mask.HasNone(MetricsCompressor.DescriptorMask.Prefix))
                {
                    var id = metricsBytes.CanRead(pos, BytesExtensions.SizeOfInt).ReadInt(pos, Endianness.BigEndian);
                    prefix = GetString(strings, id);
                    if (verbose) Console.WriteLine($"  PrefixId: {id} -> {prefix}");
                    pos += 4;
                }

                var name = _name;
                if (mask.HasNone(MetricsCompressor.DescriptorMask.Name))
                {
                    var id = metricsBytes.CanRead(pos, BytesExtensions.SizeOfInt).ReadInt(pos, Endianness.BigEndian);
                    name = GetString(strings, id);
                    if (verbose) Console.WriteLine($"  NameId: {id} -> {name}");
                    pos += 4;
                }

                var discname = _discname;
                if (mask.HasNone(MetricsCompressor.DescriptorMask.DiscriminatorName))
                {
                    var id = metricsBytes.CanRead(pos, BytesExtensions.SizeOfInt).ReadInt(pos, Endianness.BigEndian);
                    discname = GetString(strings, id);
                    if (verbose) Console.WriteLine($"  Disc.Key: {id} -> {discname}");
                    pos += 4;
                }

                var discvalue = _discvalue;
                if (mask.HasNone(MetricsCompressor.DescriptorMask.DiscriminatorValue))
                {
                    var id = metricsBytes.CanRead(pos, BytesExtensions.SizeOfInt).ReadInt(pos, Endianness.BigEndian);
                    discvalue = GetString(strings, id);
                    if (verbose) Console.WriteLine($"  Disc.Value: {id} -> {discvalue}");
                    pos += 4;
                }

                var unit = _unit;
                if (mask.HasNone(MetricsCompressor.DescriptorMask.Unit))
                {
                    unit = (MetricUnit)metricsBytes.CanRead(pos, BytesExtensions.SizeOfByte).ReadByte(pos);
                    if ((byte) unit == 255) unit = MetricUnit.None;
                    if (verbose) Console.WriteLine($"  Unit: {unit}");
                    pos += 1;
                }

                if (mask.HasNone(MetricsCompressor.DescriptorMask.ExcludedTargets))
                {
                    var excludedTargets = metricsBytes.CanRead(pos, BytesExtensions.SizeOfByte).ReadByte(pos);
                    if (verbose) Console.WriteLine($"  ExcludedTargets: {excludedTargets}");
                    pos += 1;
                }
                if (mask.HasNone(MetricsCompressor.DescriptorMask.TagCount))
                {
                    var tagCount = metricsBytes.CanRead(pos, BytesExtensions.SizeOfByte).ReadByte(pos);
                    if (verbose) Console.WriteLine($"  TagCount: {tagCount}");
                    pos += 1;
                }

                Metric metric;
                var type = (MetricValueType)metricsBytes.ReadByte(pos);
                pos += 1;
                if (verbose) Console.WriteLine($"  ValueType: {type}");
                switch (type)
                {
                    case MetricValueType.Double:
                        var d = metricsBytes.CanRead(pos, BytesExtensions.SizeOfDouble).ReadDouble(pos, Endianness.BigEndian);
                        if (verbose) Console.WriteLine($"  Value: {d}");
                        pos += BytesExtensions.SizeOfDouble;
                        var ddesc = MetricDescriptor.Create<double>(prefix, name, unit);
                        if (discname != null) ddesc = ddesc.WithDiscriminator(discname, discvalue);
                        metric = ddesc.WithValue(d);
                        break;
                    case MetricValueType.Long:
                        var l = metricsBytes.CanRead(pos, BytesExtensions.SizeOfLong).ReadLong(pos, Endianness.BigEndian);
                        if (verbose) Console.WriteLine($"  Value: {l}");
                        pos += BytesExtensions.SizeOfLong;
                        var ldesc = MetricDescriptor.Create<long>(prefix, name, unit);
                        if (discname != null) ldesc = ldesc.WithDiscriminator(discname, discvalue);
                        metric = ldesc.WithValue(l);
                        break;
                    default:
                        if (verbose) Console.WriteLine("  Value: ?!");
                        // TODO: how shall we handle eg strings?!
                        metric = null;
                        break;
                }

                _prefix = prefix;
                _name = name;
                _discname = discname;
                _discvalue = discvalue;
                _unit = unit;

                return metric;
            }

            public static IEnumerable<Metric> GetMetrics(byte[] bytes, bool verbose = false)
            {
                // get the strings blob and decompress it
                var stringsLength = bytes.CanRead(2, BytesExtensions.SizeOfInt).ReadInt(2, Endianness.BigEndian);
                if (verbose) Console.WriteLine($"StringsLength is {stringsLength} bytes [{2 + 4}..{2 + 4 + stringsLength - 1}]");
                var stringsBytes = new byte[stringsLength];
                for (var i = 0; i < stringsLength; i++) stringsBytes[i] = bytes[2 + 4 + i];
                var stringsData = Decompress(stringsBytes);
                if (verbose) Console.WriteLine($"Uncompressed to {stringsData.Length} bytes");

                // build the strings dictionary
                var strings = GetStrings(stringsData, verbose);
                if (verbose) Console.WriteLine($"Contains {strings.Count} strings");

                // get the metrics count
                var metricsCount = bytes.ReadInt(2 + 4 + stringsLength, Endianness.BigEndian);
                Console.WriteLine($"MetricsCount is {metricsCount} metrics");

                // get the metrics blob and decompress it
                var metricsLength = bytes.Length - 2 - 4 - stringsLength - 4; // everything that is left
                if (verbose) Console.WriteLine($"MetricsLength is {metricsLength} bytes [{2 + 4 + stringsLength + 4}..{2 + 4 + stringsLength + 4 + metricsLength}]");
                var metricsBytes = new byte[metricsLength];
                for (var i = 0; i < metricsLength; i++) metricsBytes[i] = bytes[2 + 4 + stringsLength + 4 + i];
                var metricsData = Decompress(metricsBytes);
                if (verbose) Console.WriteLine($"Uncompressed to {metricsData.Length} bytes");

                // get the metrics
                var pos = 0;
                var metrics = new List<Metric>();
                for (var i = 0; i < metricsCount; i++)
                {
                    if (verbose) Console.WriteLine($"[{i}]");
                    metrics.Add(GetMetric(metricsData, ref pos, strings, verbose));
                }
                return metrics;
            }
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [Timeout(30_000)]
        public void MetricsDecompressorTests(int blobNo)
        {
            var blobs = new[]
            {
                // this blob was captured during the execution of a .NET client
                "0001000001bd78018d93594fc2501085cf95452da222b8a3fe054d8c898fc6e54913a3c657835095a4052cc5edd7fbcd452d90684cd3de65b633674e25ed482aabaa865e78db8af8def30d75a9445d35d9f579ba9cfaf83a6d701769c029c596e8185b8727e43e25839d6e58631f99922f568fc89c5bc716f3b4f1b3d8966ed927ec077845bae02ef695de758de583b35473257c423c26b34bab9ac5f2826f56db506eba406f54b5ec599c59e615e8c1e70bc7ea49f9fc123d3fe99dce2cdfef880af932f85ef1e8b1f6588da50cf19af2e4b12eade2966aec3b9edf885d6b04d11571cf746f1c0fbdeb73ce4749753f0bbbbfe61d766127c3d6f7ac477e4e59e7dfac4b2b2ac1a4e1bbd3ae0eb447d5a60eb50f9ee2540d6ecec863533ef1b59b30d2c63bf5ecdb4407204ef15ec81527785a77057c4d13865a5aa6db0eb56cde52d5cdb036c8670c36e1c16ea7995497fc219effad3ce5864c6cb879b8367db574ea5125cc6814e376108c61349b3125cd30ef1e58321d1f731efce8539a035bf2d5ed50b3425315e24d07a6d258e7c437a87ec40c4df10d3db24a8bb06c8cd974c6d52b0585ffaaa954f85b4d1515c167b3f9c6b7e466c1667c8cfc319f117fa36c0000001a78015590490e825010441b704070c001678d319a78000fc019d8b8e500ba75250b6fe0c218171ecc6b7802ad6af83fb192caaf4777bad3acbe90407c55042797e73acb990a7aa4d70b920b7b7c9dcd3d12d62b702152d580d66a7f542fc9ddb1d3577a9dd384d4e01a152928b3e894d0d211a909b7ec52ff046cdb06edef1418df9422252f11616b574fc555ef2db1a745f1924c32c43e3c806378680e3f7cd839b22b75ea189f28073f4a64a219b4274d4b928034331492e68674cac212572c2dc90fd6e326f5",

                // this blob was captured during the execution of a .NET client
                "0001000001f278018d93dd6fd26018c54fb78250d8701bea36bff53ff0c2c44bddf4464d165df4d230a84ad2422d30c5bfdedf79191648344bd3be1fcfd779ce792ae9a9a41d1da8a74bdea132be177c539da9d4587d76139e31a709be918eb9cb34e334c556ea04db8827e57e4a069fce59f31039255fae82c8ede8085bce33c4cfb1037d625fb29fe195e93d7779a834d7472cbf394bdda8854f8ac76676e98e9a582ef1ad6a1be5dd28d12faa3a7b1567cbae127d0df9d2b57a521cefd3f377cde9ccf9fe8da816ef80ef271e056bc16a962ac4878ac9e32e5df1bebaec4781df8cdd6005d107e27ed0bd395e78df6b4721ea21bd590bdf9f6137fbc3704ac9e57aa7c4ad76273dea34f416cb3c6832c36f4afdc79df8af1ed2934e5b9f895b6a30d22bfcad86b12a3ea40f6b66ae5ddb4c4c42be2c4c45c5f339366b2cdd568b9dd9f802b7177ac63bd073bdc056dfeaa2c41b7c3d53a7a14e3fd42fc8eea9f2fc2cb176b6eb1baa1c45b580647285f016dc8ea8b5a87c1035b8ef91c57af561dd786ec0dd183466eaba95b7a205efc7d12eca7a9a077a1d182c37f87c90246b188ddf4c490da6ab004bf5d79c709eadb0df065b79d5ed92bda6f688f7d459855cef88ef51fd251363dd7bfac62add8465336675d6ff1529a95d77765bb5ffcfee9eeae0b3364b7cfb51136ce663e5fffc03975bb6320000001e78015590c90dc23014441d8725ec6109fb12502e5c39724a2914001d0489129090e8840e28291c38c19f89e3285f1a795efe77c676f893525258590027f9bc0eb52b9ca1e89888d32217ab133d9442bf22ca0a54cd81bd5a89ea86b4c6a4478ae3ef0dd4400c0bd4343e4b68593a8b6b8b3a36d4bb0876ed00537b190677924fd2a9af30dae755c5044fe0804da5d3589dc40e452351201ae717dfbf3139b191fceb543eb11c9c776640f1c5e639b9e82d72e2be6589569610b1b6c4a36eec9310c3e20018de96bbbb62ef1f0b1d2c2d",

                // this blob was captured during the execution of a Python client
                "00010000015378018d91594fc2401485cf48cb52c01571f93bc44713a3c657832d8626dd8482e2aff7bb4390faa03193cedce59cbb9c4aba9674c6996acd972ae37ee19ee94e0b958ab1969c126f09f64a43f21bd53e7ecf6d8814f64c09f9cb30d00376e1bd23eac770734e0ac758899eb017d82b7a65ba2596fbfa1b98a93ef125e7fa6066206a6225f51e79739f0bd4e35de3c7df599badeb227da822be68f02c73a8b15e7d3ddb6bcefc4bcffebdff4930649a77ea54bc15afedb99f2f54401ddbc9ea471a61175ec30c2b69f4378dded8d574dca27b03e7592d78b9eff04c07d374ee3795da2ea48ae96e2ce9806e05c8edfe725de253f689b96358a6d80054e9d11d77c8e405d14437bc35c88d266457de93fa51f44377cbd974d2395a55e0f77f7e82bf6aa87faa0ef96da5dd1fb9500b760c521aba31ec9ac9b27f6a7d1cfeadf5586dfa55d4dcf51bb99ed726696ef105d47175b0000000147801636080004620c504c4cc20060303632dc3f49c4fb940160b0b580044d402312b8c1708e2b1c17825201e3b8c9703e271c078607d9c301e3b488e0bc65b03b2821bc613ca480032798098f73f0c80e4f8dfe526804de103f1b817052b8079fc201e8b8f9403982700e209f941550a8278bc2d5c17186a40c2402c0c12010101900b44c04cc69d0160799095a26011f955d601301d6260110686fa1339409638104b404518aabd41864842b99607003c0523c7",

                // this blob was captured during the execution of the MetricsCompress test
                "00010000002078016360606062000156863c8644865c86540643208f918591c10800198d021200000002780153f80f040c4000a2418005c40101964bb5409211cc666010d30300a21b0db1"
            };

            var blob = blobs[blobNo];

            // get the bytes
            Console.WriteLine($"Blob is {blob.Length} chars -> {blob.Length/2} bytes");
            var bytes = new byte[blob.Length / 2];
            for (var i = 0; i < blob.Length; i += 2)
                bytes[i/2] = byte.Parse(blob.Substring(i, 2), NumberStyles.HexNumber);

            // decompress in C#
            var metrics = MetricsDecompressor.GetMetrics(bytes, true);
            foreach (var metric in metrics) Console.WriteLine(metric);

            // consume in Java
            var rc = JavaConsume(bytes);
            Assert.That(rc, Is.Zero, "Java failed.");
        }

        private int JavaConsume(byte[] bytes)
        {
            // determine solution path
            var assemblyLocation = GetType().Assembly.Location;
            var solutionPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(assemblyLocation), "../../../../.."));

            // name a temp directory
            var tempPath = Path.Combine(Path.GetTempPath(), $"hz-tests-{Guid.NewGuid():N}");

            var serverVersion = "5.0";  // TODO: need a better way of passing the server's version

            try
            {
                return JavaConsume(bytes, solutionPath, tempPath, serverVersion);
            }
            finally
            {
                // get rid of the temp directory
                Directory.Delete(tempPath, true);
            }
        }

        private int JavaConsume(byte[] bytes, string solutionPath, string tempPath, string serverVersion)
        {
            // create the temp directory and copy the source files
            Directory.CreateDirectory(tempPath);
            File.WriteAllText(Path.Combine(tempPath, "Program.java"), TestFiles.ReadAllText(this, "Java/CloudTests/Program.java"));
            File.WriteAllText(Path.Combine(tempPath, "TestConsumer.java"), TestFiles.ReadAllText(this, "Java/Cloudtests/TestConsumer.java"));

            // validate that we have the server JAR
            var version = ServerVersion.GetVersion(NuGetVersion.Parse(serverVersion));
            Console.WriteLine($"Server Version: {version}");
            var serverJarPath = Path.Combine(solutionPath, $"temp/lib/hazelcast-{version}.jar");
            Assert.That(File.Exists(serverJarPath), Is.True, $"Could not find JAR file {serverJarPath}, try 'hz get-server -server {version}");

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
            Assert.That(p.ExitCode, Is.Zero, "Java compilation failed.");

            // execute
            Console.WriteLine("Execute...");
            var asyncReads = true; // syncReads can hang if output is too big?
            p = new Process
            {
                StartInfo = new ProcessStartInfo(Path.Combine(JdkPath, "bin/java.exe"), $"-cp {serverJarPath};{tempPath} Program")
                    .WithRedirects(true, true, true)
            };

            if (asyncReads)
            {
                p.OutputDataReceived += (sender, args) => { Console.WriteLine(args.Data); };
                p.ErrorDataReceived += (sender, args) =>
                {
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("ERR: " + args.Data);
                    Console.ForegroundColor = color;
                };
            }

            p.Start();

            if (asyncReads)
            {
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
            }

            Console.WriteLine($"Writing {bytes.Length} bytes to java");
            p.StandardInput.BaseStream.Write(bytes, 0, bytes.Length);
            p.StandardInput.Close();
            Console.WriteLine("Waiting for completion...");
            p.WaitForExit();
            Console.WriteLine($"Execution exit code: {p.ExitCode}");
            if (!asyncReads)
            {
                Console.WriteLine("Execution stderr:");
                Console.WriteLine(p.StandardError.ReadToEnd());
                Console.WriteLine("Execution stdout:");
                Console.WriteLine(p.StandardOutput.ReadToEnd());
            }

            return p.ExitCode;
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
    }
}
