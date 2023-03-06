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
using System.Collections.Generic;
using System.IO;
using Hazelcast.Core;
using Ionic.Zlib;

namespace Hazelcast.Metrics
{
    // compresses metrics, closely following the MetricsCompressor.java code to ensure interoperability
    internal class MetricsCompressor : IDisposable
    {
        private const int InitialStringsBufferSize = 2 << 10; // 2kB
        private const int InitialMetricsBufferSize = 2 << 11; // 4kB
        private const int InitialTempBufferSize = 2 << 8; // 512B

        // about compression
        // read https://stackoverflow.com/questions/6522778/java-util-zip-deflater-equivalent-in-c-sharp
        // System.IO.Compression.DeflateStream is *not* Java-compatible!
        // now using ZlibStream from DotNetZip, would be worth benchmarking against SharpZipLib

        // output streams for the blob containing the strings
        private MemoryStream _stringsBuffer;
        private ZlibStream _stringsCompressStream;
        private DataOutputStream _stringsOutput;

        // output streams for the blob containing the metrics
        private MemoryStream _metricsBuffer;
        private ZlibStream _metricsCompressStream;
        private DataOutputStream _metricsOutput;

        // temporary buffer to avoid fragmented writes to the compressed streams, when
        // when writing primitive fields - TODO: is this needed in C#?
        private readonly MemoryStream _tempBuffer;
        private readonly DataOutputStream _tempOutput;

        private SortedDictionary<string, int> _strings = new SortedDictionary<string, int>();
        private int _count;
        private IMetricDescriptor _lastDescriptor;
        private bool _disposed, _closed;

        public MetricsCompressor()
        {
            Reset(InitialStringsBufferSize, InitialMetricsBufferSize);

            // that one is never reset
            _tempBuffer = new MemoryStream(InitialTempBufferSize);
            _tempOutput = new DataOutputStream(_tempBuffer);
        }

        private static void Reset(ref MemoryStream buffer, ref ZlibStream compress, ref DataOutputStream output, int size)
        {
            compress?.Dispose();

            // shrink if capacity is more than 50% larger than the estimated size
            if (buffer == null || buffer.Capacity > 3 * size/ 2)
            {
                buffer?.Dispose();
                buffer = new MemoryStream(size);
            }

            buffer.Seek(0, SeekOrigin.Begin);
            compress = new ZlibStream(buffer, CompressionMode.Compress, CompressionLevel.BestSpeed, true);
            output = new DataOutputStream(compress);
        }

        private void Reset(int stringsBufferSize, int metricsBufferSize)
        {
            Reset(ref _stringsBuffer, ref _stringsCompressStream, ref _stringsOutput, stringsBufferSize);
            Reset(ref _metricsBuffer, ref _metricsCompressStream, ref _metricsOutput, metricsBufferSize);

            _strings = new SortedDictionary<string, int>();
            _count = 0;
            _lastDescriptor = null;
            _closed = false;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _tempBuffer?.Dispose();

            _stringsCompressStream?.Dispose();
            _stringsBuffer?.Dispose();

            _metricsCompressStream?.Dispose();
            _metricsBuffer?.Dispose();
        }

        public void Append(Metric metric)
        {
            if (_closed) throw new InvalidOperationException("Compressor is closed.");

            // TODO: if we buffered metrics and ordered them by prefix we could save more space?

            if (!(metric is Metric<double>) &&
                !(metric is Metric<long>) &&
                !(metric is Metric<int>))
            {
                // we can only send numeric metrics, everything else is ignored
                return;
            }

            _tempBuffer.Seek(0, SeekOrigin.Begin);
            AppendDescriptor(metric.Descriptor);

            switch (metric)
            {
                case Metric<double> doubleMetric:
                    _tempOutput.WriteByte((byte) MetricValueType.Double);
                    _tempOutput.WriteDouble(doubleMetric.Value);
                    break;
                case Metric<long> longMetric:
                    _tempOutput.WriteByte((byte) MetricValueType.Long);
                    _tempOutput.WriteLong(longMetric.Value);
                    break;
                case Metric<int> intMetric:
                    _tempOutput.WriteByte((byte) MetricValueType.Long);
                    _tempOutput.WriteLong(intMetric.Value);
                    break;
            }

            // protect case
            if (_tempBuffer.Position > int.MaxValue) throw new InvalidOperationException($"Out of range: _tempBuffer.Position ({_tempBuffer.Position}).");

            _metricsOutput.Write(_tempBuffer.GetBuffer(), 0, (int) _tempBuffer.Position);
        }

        private void AppendDescriptor(IMetricDescriptor descriptor)
        {
            var mask = GetMask(descriptor);
            _tempOutput.WriteByte((byte) mask);

            if (mask.HasNone(DescriptorMask.Prefix))
                _tempOutput.WriteInt(GetStringId(descriptor.Prefix));

            if (mask.HasNone(DescriptorMask.Name))
                _tempOutput.WriteInt(GetStringId(descriptor.Name));

            if (mask.HasNone(DescriptorMask.DiscriminatorName))
                _tempOutput.WriteInt(GetStringId(descriptor.DiscriminatorKey));

            if (mask.HasNone(DescriptorMask.DiscriminatorValue))
                _tempOutput.WriteInt(GetStringId(descriptor.DiscriminatorValue));

            if (mask.HasNone(DescriptorMask.Unit))
                _tempOutput.WriteByte((byte) descriptor.Unit);

            // include excludeTargets for compatibility purposes (but it's always zero)
            if (mask.HasNone(DescriptorMask.ExcludedTargets))
                _tempOutput.WriteByte(0);

            if (mask.HasNone(DescriptorMask.TagCount))
                _tempOutput.WriteByte((byte) descriptor.Tags.Count);

            // further compression would be possible by writing only the different tags
            foreach (var (tagName, tagValue) in descriptor.Tags)
            {
                _tempOutput.WriteInt(GetStringId(tagName));
                _tempOutput.WriteInt(GetStringId(tagValue));
            }

            _count++;
            _lastDescriptor = descriptor; // TODO: Java clones the descriptor, are we safe?
        }

        private DescriptorMask GetMask(IMetricDescriptor descriptor)
        {
            var mask = DescriptorMask.None;

            // "excluded targets" are not supported, hence always masked
            mask |= DescriptorMask.ExcludedTargets;

            if (_lastDescriptor == null)
                return mask;

            if (descriptor.Prefix == _lastDescriptor.Prefix)
                mask |= DescriptorMask.Prefix;

            if (descriptor.Name == _lastDescriptor.Name)
                mask |= DescriptorMask.Name;

            if (descriptor.DiscriminatorKey == _lastDescriptor.DiscriminatorKey)
                mask |= DescriptorMask.DiscriminatorName;

            if (descriptor.DiscriminatorValue == _lastDescriptor.DiscriminatorValue)
                mask |= DescriptorMask.DiscriminatorValue;

            if (descriptor.Unit == _lastDescriptor.Unit)
                mask |= DescriptorMask.Unit;

            // "excluded targets" are not supported, hence always masked
            //if (Objects.equals(descriptor.excludedTargets(), lastDescriptor.excludedTargets()))
            //    mask |= DescriptorMask.ExcludedTargets;

            if (descriptor.TagCount == _lastDescriptor.TagCount)
                mask |= DescriptorMask.TagCount;

            return mask;
        }

        private int GetStringId(string s)
        {
            if (s == null)
                return -1;

            if (_strings.TryGetValue(s, out var id))
                return id;

            id = _strings.Count;
            _strings[s] = id;
            return id;
        }

        private void AppendStrings()
        {
            _stringsOutput.WriteInt(_strings.Count);
            var prevText = "";

            // sorted dictionary is ordered by natural order of its keys
            // so that delta-processing is efficient
            foreach (var (stringText, stringId) in _strings)
            {
                // this should have been checked earlier, this is a safety check
                // this protects the length casts to byte below
                if (stringText.Length > byte.MaxValue)
                    throw new InvalidOperationException($"Out of range: stringText.Length (\"{stringText}\").");

                // find the span of chars that is common to stringText and prevText
                var maxCommonLen = Math.Min(prevText.Length, stringText.Length);
                var commonLen = 0;
                while (commonLen < maxCommonLen && stringText[commonLen] == prevText[commonLen])
                    commonLen++;

                // compute the length of remaining, non-common chars
                var diffLen = stringText.Length - commonLen;

                // write through temp buffer
                _tempBuffer.Seek(0, SeekOrigin.Begin);

                // write
                _tempOutput.WriteInt(stringId);
                _tempOutput.WriteByte((byte) commonLen);
                _tempOutput.WriteByte((byte) diffLen);
                _tempOutput.WriteString(stringText, commonLen, diffLen);

                // protect case
                if (_tempBuffer.Position > int.MaxValue) throw new InvalidOperationException($"Out of range: _tempBuffer.Position ({_tempBuffer.Position}).");

                _stringsOutput.Write(_tempBuffer.GetBuffer(), 0, (int) _tempBuffer.Position);

                prevText = stringText;
            }
        }

        private byte[] GetBytes()
        {
            const int binaryFormatVersion = 1;
            const int sizeVersion = 2;
            const int sizeDictionaryBlob = 4;
            const int sizeCountMetrics = 4;

            const int bitsInByte = 8;
            const int byteMask = 0xff;

            // close the compressor, we're going to dispose the compressed streams
            _closed = true;

            AppendStrings();

            // got to dispose the compressed streams to flush them all
            _stringsCompressStream.Dispose();
            _stringsCompressStream = null;
            _metricsCompressStream.Dispose();
            _metricsCompressStream = null;

            // version info + dictionary length + dictionary blob + number of metrics + metrics blob
            var completeSize = sizeVersion + sizeDictionaryBlob + _stringsBuffer.Position + sizeCountMetrics + _metricsBuffer.Position;

            // protect casts
            if (completeSize > int.MaxValue) throw new InvalidOperationException($"Out of range: completeSize ({completeSize}).");
            if (_stringsBuffer.Position > int.MaxValue) throw new InvalidOperationException($"Out of range: _stringsBuffer.Length ({_stringsBuffer.Position}).");
            if (_metricsBuffer.Position > int.MaxValue) throw new InvalidOperationException($"Out of range: _metricsBuffer.Length ({_metricsBuffer.Position}).");

            using var buffer = new MemoryStream((int) completeSize);
            var output = new DataOutputStream(buffer);

            // ReSharper disable once ShiftExpressionResultEqualsZero - well, yes
            output.WriteByte((binaryFormatVersion >> bitsInByte) & byteMask);
            output.WriteByte(binaryFormatVersion & byteMask);

            output.WriteInt((int) _stringsBuffer.Position);
            output.Write(_stringsBuffer.GetBuffer(), 0, (int) _stringsBuffer.Position);
            output.WriteInt(_count);
            output.Write(_metricsBuffer.GetBuffer(), 0, (int) _metricsBuffer.Position);

            return buffer.ToArray();
        }

        public byte[] GetBytesAndReset()
        {
            var bytes = GetBytes();

            const int sizeFactoryNumerator = 11;
            const int sizeFactoryDenominator = 10;

            // TODO: use .Length vs .Position vs .Capacity?!
            var dictionaryBufferSize = (int) _stringsBuffer.Length * sizeFactoryNumerator / sizeFactoryDenominator;
            var metricsBufferSize = (int) _metricsBuffer.Length * sizeFactoryNumerator / sizeFactoryDenominator;
            Reset(dictionaryBufferSize, metricsBufferSize);

            return bytes;
        }

        [Flags]
        internal enum DescriptorMask : byte
        {
            None = 0,
            Prefix = 1,
            Name = 1 << 1,
            DiscriminatorName = 1 << 2,
            DiscriminatorValue = 1 << 3,
            Unit = 1 << 4,
            ExcludedTargets = 1 << 5,
            TagCount = 1 << 6
        }
    }
}
