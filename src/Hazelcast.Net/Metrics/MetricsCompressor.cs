using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Hazelcast.Core;

namespace Hazelcast.Metrics
{
    // provides Java DataOutputStream (at least, what we really need) to C#

    // compresses metrics, closely following the MetricsCompressor.java code to ensure interoperability
    internal class MetricsCompressor : IDisposable
    {
        private const int InitialStringsBufferSize = 2 << 10; // 2kB
        private const int InitialMetricsBufferSize = 2 << 11; // 4kB
        private const int InitialTempBufferSize = 2 << 8; // 512B

        // output streams for the blob containing the strings
        private MemoryStream _stringsBuffer;
        private DeflateStream _stringsDeflateStream;
        private DataOutputStream _stringsOutput;

        // output streams for the blob containing the metrics
        private MemoryStream _metricsBuffer;
        private DeflateStream _metricsDeflateStream;
        private DataOutputStream _metricsOutput;

        // temporary buffer to avoid fragmented writes to the deflate streams, when
        // when writing primitive fields - TODO: is this needed in C#?
        private readonly MemoryStream _tempBuffer;
        private readonly DataOutputStream _tempOutput;

        private SortedDictionary<string, int> _strings = new SortedDictionary<string, int>();
        private int _count;
        private MetricDescriptor _lastDescriptor;
        private bool _disposed, _closed;

        public MetricsCompressor()
        {
            Reset(InitialStringsBufferSize, InitialMetricsBufferSize);

            // that one is never reset
            _tempBuffer = new MemoryStream(InitialTempBufferSize);
            _tempOutput = new DataOutputStream(_tempBuffer);
        }

        private static void Reset(ref MemoryStream buffer, ref DeflateStream deflate, ref DataOutputStream output, int size)
        {
            deflate?.Dispose();

            // shrink if capacity is more than 50% larger than the estimated size
            if (buffer == null || buffer.Capacity > 3 * size/ 2)
            {
                buffer?.Dispose();
                buffer = new MemoryStream(size);
            }

            buffer.Seek(0, SeekOrigin.Begin);
            deflate = new DeflateStream(buffer, CompressionLevel.Fastest, true);
            output = new DataOutputStream(deflate);
        }

        private void Reset(int stringsBufferSize, int metricsBufferSize)
        {
            Reset(ref _stringsBuffer, ref _stringsDeflateStream, ref _stringsOutput, stringsBufferSize);
            Reset(ref _metricsBuffer, ref _metricsDeflateStream, ref _metricsOutput, metricsBufferSize);

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

            _stringsBuffer?.Dispose();
            _stringsDeflateStream?.Dispose();

            _metricsBuffer?.Dispose();
            _metricsDeflateStream?.Dispose();
        }

        public void AppendLong(MetricDescriptor descriptor, long value)
        {
            if (_closed) throw new InvalidOperationException("Compressor is closed.");

            _tempBuffer.Seek(0, SeekOrigin.Begin);
            AppendDescriptor(descriptor);
            _tempOutput.WriteByte((byte) MetricValueType.Long);
            _tempOutput.WriteLong(value);

            // protect case
            if (_tempBuffer.Length > int.MaxValue) throw new InvalidOperationException($"Out of range: _tempBuffer.Length ({_tempBuffer.Length}).");

            _metricsOutput.Write(_tempBuffer.GetBuffer(), 0, (int) _tempBuffer.Length);
        }

        public void AppendDouble(MetricDescriptor descriptor, double value)
        {
            if (_closed) throw new InvalidOperationException("Compressor is closed.");

            _tempBuffer.Seek(0, SeekOrigin.Begin);
            AppendDescriptor(descriptor);
            _tempOutput.WriteByte((byte) MetricValueType.Double);
            _tempOutput.WriteDouble(value);

            // protect case
            if (_tempBuffer.Length > int.MaxValue) throw new InvalidOperationException($"Out of range: _tempBuffer.Length ({_tempBuffer.Length}).");

            _metricsOutput.Write(_tempBuffer.GetBuffer(), 0, (int) _tempBuffer.Length);
        }

        private void AppendDescriptor(MetricDescriptor descriptor)
        {
            var mask = GetMask(descriptor);
            _tempOutput.WriteByte((byte) mask);

            if (mask.HasNone(DescriptorMask.Prefix))
                _tempOutput.WriteInt(GetStringId(descriptor.Prefix));

            if (mask.HasNone(DescriptorMask.Name))
                _tempOutput.WriteInt(GetStringId(descriptor.Name));

            if (mask.HasNone(DescriptorMask.DiscriminatorName))
                _tempOutput.WriteInt(GetStringId(descriptor.DiscriminatorName));

            if (mask.HasNone(DescriptorMask.DiscriminatorValue))
                _tempOutput.WriteInt(GetStringId(descriptor.DiscriminatorValue));

            if (mask.HasNone(DescriptorMask.Unit))
                _tempOutput.WriteInt((int) descriptor.Unit);

            //if (mask.HasNone(DescriptorMask.ExcludedTargets))
            //    tmpDos.WriteByte(MetricTarget.BitSet(descriptor.ExcludedTargets));

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

        private DescriptorMask GetMask(MetricDescriptor descriptor)
        {
            var mask = DescriptorMask.None;

            if (_lastDescriptor == null) 
                return mask;

            if (descriptor.Prefix == _lastDescriptor.Prefix)
                mask |= DescriptorMask.Prefix;

            if (descriptor.Name == _lastDescriptor.Name)
                mask |= DescriptorMask.Name;

            if (descriptor.DiscriminatorName == _lastDescriptor.DiscriminatorName)
                mask |= DescriptorMask.DiscriminatorName;

            if (descriptor.DiscriminatorValue == _lastDescriptor.DiscriminatorValue)
                mask |= DescriptorMask.DiscriminatorValue;

            if (descriptor.Unit == _lastDescriptor.Unit)
                mask |= DescriptorMask.Unit;

            //if (Objects.equals(descriptor.excludedTargets(), lastDescriptor.excludedTargets()))
            mask |= DescriptorMask.ExcludedTargets;

            if (descriptor.Tags.Count == _lastDescriptor.Tags.Count)
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
                if (_tempBuffer.Length > int.MaxValue) throw new InvalidOperationException($"Out of range: _tempBuffer.Length ({_tempBuffer.Length}).");

                _stringsOutput.Write(_tempBuffer.GetBuffer(), 0, (int) _tempBuffer.Length);

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

            // close the compressor, we're going to dispose the deflate streams
            _closed = true;

            AppendStrings();

            // got to dispose the deflate streams to flush them all
            _stringsDeflateStream.Dispose();
            _stringsDeflateStream = null;
            _metricsDeflateStream.Dispose();
            _metricsDeflateStream = null;

            // version info + dictionary length + dictionary blob + number of metrics + metrics blob
            var completeSize = sizeVersion + sizeDictionaryBlob + _stringsBuffer.Length + sizeCountMetrics + _metricsBuffer.Length;

            // protect casts
            if (completeSize > int.MaxValue) throw new InvalidOperationException($"Out of range: completeSize ({completeSize}).");
            if (_stringsBuffer.Length > int.MaxValue) throw new InvalidOperationException($"Out of range: _stringsBuffer.Length ({_stringsBuffer.Length}).");
            if (_metricsBuffer.Length > int.MaxValue) throw new InvalidOperationException($"Out of range: _metricsBuffer.Length ({_metricsBuffer.Length}).");

            using var buffer = new MemoryStream((int) completeSize);
            var output = new DataOutputStream(buffer);

            // ReSharper disable once ShiftExpressionResultEqualsZero - well, yes
            output.WriteByte((binaryFormatVersion >> bitsInByte) & byteMask);
            output.WriteByte(binaryFormatVersion & byteMask);

            output.WriteInt((int) _stringsBuffer.Length);
            output.Write(_stringsBuffer.GetBuffer(), 0, (int) _stringsBuffer.Length);
            output.WriteInt(_count);
            output.Write(_metricsBuffer.GetBuffer(), 0, (int) _metricsBuffer.Length);

            return buffer.ToArray();
        }

        public byte[] GetBytesAndReset()
        {
            var bytes = GetBytes();

            const int sizeFactoryNumerator = 11;
            const int sizeFactoryDenominator = 10;

            var dictionaryBufferSize = (int) _stringsBuffer.Length * sizeFactoryNumerator / sizeFactoryDenominator;
            var metricsBufferSize = (int) _metricsBuffer.Length * sizeFactoryNumerator / sizeFactoryDenominator;
            Reset(dictionaryBufferSize, metricsBufferSize);

            return bytes;
        }

        [Flags]
        private enum DescriptorMask : byte
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
