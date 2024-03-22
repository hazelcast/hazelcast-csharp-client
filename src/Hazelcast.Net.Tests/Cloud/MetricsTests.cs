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
using System.IO;
using System.Linq;
using System.Text;
using Hazelcast.Core;
using Hazelcast.Metrics;
using Hazelcast.Polyfills;
using NUnit.Framework;

namespace Hazelcast.Tests.Cloud;

[TestFixture]
public class MetricsTests
{
    [Test]
    public void ZLibTest()
    {
        // this tests that a zipped blob of text can be unzipped
        const string sourceString = "this is a test";

        // compress the string
        var sourceBytes = Encoding.UTF8.GetBytes(sourceString);
        var sourceStream = new MemoryStream(sourceBytes);

        var compressedStream = new MemoryStream();
        var compress = ZLibStreamFactory.Compress(compressedStream, false);
        sourceStream.CopyTo(compress);
        compress.Dispose(); // because, Flush is not enough
        var compressedBytes = compressedStream.ToArray();

        // dump the compressed bytes to console
        Console.WriteLine(compressedBytes.Dump(formatted: false));

        // decompress
        compressedStream = new MemoryStream(compressedBytes);
        var decompress = ZLibStreamFactory.Decompress(compressedStream, false);
        var decompressedStream = new MemoryStream();
        decompress.CopyTo(decompressedStream);
        var resultBytes = decompressedStream.ToArray();
        var resultString = Encoding.UTF8.GetString(resultBytes);

        // validate
        Assert.That(resultString, Is.EqualTo(sourceString));
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

    internal static class MetricsDecompressor
    {
        private static string _prefix;
        private static string _name;
        private static string _discName;
        private static string _discValue;
        private static MetricUnit _unit;

        // note: BytesExtensions ReadInt() etc methods are highly optimized and assume that boundary
        // checks have been properly performed beforehand - which means that if they fail, they can
        // take the entire process down - so here we protect them with CanRead() calls which we would
        // not use in the actual code - so we get a "normal" exception

        private static byte[] Decompress(byte[] bytes)
        {
            try
            {
                using var bytesStream = new MemoryStream(bytes);
                using var decompress = ZLibStreamFactory.Decompress(bytesStream, false);
                var u = new MemoryStream();
                decompress.CopyTo(u);
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

            var discName = _discName;
            if (mask.HasNone(MetricsCompressor.DescriptorMask.DiscriminatorName))
            {
                var id = metricsBytes.CanRead(pos, BytesExtensions.SizeOfInt).ReadInt(pos, Endianness.BigEndian);
                discName = GetString(strings, id);
                if (verbose) Console.WriteLine($"  Disc.Key: {id} -> {discName}");
                pos += 4;
            }

            var discValue = _discValue;
            if (mask.HasNone(MetricsCompressor.DescriptorMask.DiscriminatorValue))
            {
                var id = metricsBytes.CanRead(pos, BytesExtensions.SizeOfInt).ReadInt(pos, Endianness.BigEndian);
                discValue = GetString(strings, id);
                if (verbose) Console.WriteLine($"  Disc.Value: {id} -> {discValue}");
                pos += 4;
            }

            var unit = _unit;
            if (mask.HasNone(MetricsCompressor.DescriptorMask.Unit))
            {
                unit = (MetricUnit)metricsBytes.CanRead(pos, BytesExtensions.SizeOfByte).ReadByte(pos);
                if ((byte)unit == 255) unit = MetricUnit.None;
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
                    var doubleDescriptor = MetricDescriptor.Create<double>(prefix, name, unit);
                    if (discName != null) doubleDescriptor = doubleDescriptor.WithDiscriminator(discName, discValue);
                    metric = doubleDescriptor.WithValue(d);
                    break;
                case MetricValueType.Long:
                    var l = metricsBytes.CanRead(pos, BytesExtensions.SizeOfLong).ReadLong(pos, Endianness.BigEndian);
                    if (verbose) Console.WriteLine($"  Value: {l}");
                    pos += BytesExtensions.SizeOfLong;
                    var longDescriptor = MetricDescriptor.Create<long>(prefix, name, unit);
                    if (discName != null) longDescriptor = longDescriptor.WithDiscriminator(discName, discValue);
                    metric = longDescriptor.WithValue(l);
                    break;
                default:
                    if (verbose) Console.WriteLine("  Value: ?!");
                    // TODO: how shall we handle eg strings?!
                    metric = null;
                    break;
            }

            _prefix = prefix;
            _name = name;
            _discName = discName;
            _discValue = discValue;
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
}
