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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Metrics;
using Hazelcast.Testing;
using Hazelcast.Testing.Remote;
using NUnit.Framework;

namespace Hazelcast.Tests.Cloud;

[TestFixture]
public class MetricsRemoteTests : SingleMemberClientRemoteTestBase
{
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [Timeout(30_000)]
    public async Task MetricsDecompressorTests(int blobNo)
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
        Console.WriteLine($"Blob is {blob.Length} chars -> {blob.Length / 2} bytes");
        var bytes = new byte[blob.Length / 2];
        for (var i = 0; i < blob.Length; i += 2)
            bytes[i / 2] = byte.Parse(blob.Substring(i, 2), NumberStyles.HexNumber);

        // decompress in C#
        var metrics = MetricsTests.MetricsDecompressor.GetMetrics(bytes, true);
        foreach (var metric in metrics) Console.WriteLine(metric);

        // consume in Java - will throw if exit code is not zero
        await JavaConsume(bytes);
    }

    [Test]
    [Timeout(30_000)]
    public async Task MetricsCompressDecompressorTests()
    {
        // compress some metrics
        var compressor = new MetricsCompressor();
        compressor.Append(MetricDescriptor.Create<int>("name1", MetricUnit.Count).WithValue(1234));
        compressor.Append(MetricDescriptor.Create<int>("name2", MetricUnit.Count).WithValue(5678));
        var bytes = compressor.GetBytesAndReset();

        // decompress in C#
        var metrics = MetricsTests.MetricsDecompressor.GetMetrics(bytes, true);
        foreach (var metric in metrics) Console.WriteLine(metric);

        // consume in Java - will throw if exit code is not zero
        await JavaConsume(bytes);
    }

    private async Task JavaConsume(byte[] bytes)
    {
        const string scriptTemplate = @"
// import types
var ArrayOfBytes = Java.type(""byte[]"")
var MetricsCompressor = Java.type(""com.hazelcast.internal.metrics.impl.MetricsCompressor"")
var MetricConsumer = Java.type(""com.hazelcast.internal.metrics.MetricConsumer"")
var StringBuilder = Java.type(""java.lang.StringBuilder"")

// prepare bytes
var bytes = new ArrayOfBytes($$COUNT$$)
$$BYTES$$

// consumer will append to the string builder
var text = new StringBuilder()
var TestConsumer = Java.extend(MetricConsumer, {
    consumeLong: function(descriptor, value) {
        text.append(""prefix   = "")
        text.append(descriptor.prefix())
        text.append(""\n"")
        text.append(""disc.key = "")
        text.append(descriptor.discriminator())
        text.append(""\n"")
        text.append(""disc.val = "")
        text.append(descriptor.discriminatorValue())
        text.append(""\n"")
        text.append(""string   = "")
        text.append(descriptor.metricString())
        text.append(""\n"")

        text.append(descriptor.metric())
        text.append("" = "")
        text.append(value)
        text.append(""\n"")
    },
    consumeDouble: function(descriptor, value) {
        text.append(descriptor.metric())
        text.append("" = "")
        text.append(value)
        text.append(""\n"")
    }
})
var consumer = new TestConsumer()
MetricsCompressor.extractMetrics(bytes, consumer)

result = """" + text
";

        var script = scriptTemplate
            .Replace("$$COUNT$$", bytes.Length.ToString())
            .Replace("$$BYTES$$", string.Join("\n",
                bytes.Select((x, i) => $"bytes[{i}] = {bytes[i]}")));

        var response = await RcClient.ExecuteOnControllerAsync(RcCluster.Id, script, Lang.JAVASCRIPT);
        Assert.That(response.Success, $"message: {response.Message}");
        Assert.That(response.Result, Is.Not.Null);
        var resultString = Encoding.UTF8.GetString(response.Result, 0, response.Result.Length).Trim();
        Console.WriteLine("JAVA OUTPUT:");
        Console.WriteLine(resultString);
    }
}
