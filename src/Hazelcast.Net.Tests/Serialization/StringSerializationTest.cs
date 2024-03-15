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
using System.Text;
using Hazelcast.Core;
using Hazelcast.Serialization;
using Hazelcast.Serialization.ConstantSerializers;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization
{
    public class StringSerializationTest
    {
        private const string TestDataTurkish = "Pijamalı hasta, yağız şoföre çabucak güvendi.";
        private const string TestDataJapanese = "イロハニホヘト チリヌルヲ ワカヨタレソ ツネナラム";
        private const string TestDataAscii = "The quick brown fox jumps over the lazy dog";
        private const string TestDataUtf4ByteEmojis = "loudly crying face:\uD83D\uDE2D nerd face: \uD83E\uDD13";
        private const string TestDataAll =
            TestDataTurkish + TestDataJapanese + TestDataAscii + TestDataUtf4ByteEmojis;
        private const int TestStrSize = 1 << 20;
        private static readonly byte[] TestDataBytesAll = Encoding.UTF8.GetBytes(TestDataAll);
        private static readonly char[] AllChars;

        private SerializationService _serializationService;

        static StringSerializationTest()
        {
            var chars = new List<char>(char.MaxValue);
            for (var c = 0; c < char.MaxValue; c++)
            {
                if (char.IsLetter((char) c))
                {
                    chars.Add((char) c);
                }
            }

            AllChars = chars.ToArray();
        }

        [SetUp]
        public virtual void Setup()
        {
            _serializationService = new SerializationServiceBuilder(new SerializationOptions(), new NullLoggerFactory())
                .AddDefinitions(new ConstantSerializerDefinitions())
                .Build();
        }

        [TearDown]
        public virtual void TearDown()
        {
            _serializationService.Dispose();
        }

        [Test]
        public void TestLargeStringEncodeDecode()
        {
            var sb = new StringBuilder();
            var i = 0;
            var j = 0;
            while (j < TestStrSize)
            {
                var ch = i++%char.MaxValue;
                if (char.IsLetter((char) ch))
                {
                    sb.Append(ch);
                    j++;
                }
            }
            var actualStr = sb.ToString();
            var strBytes = Encoding.UTF8.GetBytes(actualStr);
            var actualDataBytes = _serializationService.ToData(actualStr).ToByteArray();
            var expectedDataByte = ToDataByte(strBytes);
            var decodedStr = (string) _serializationService.ToObject<object>(new HeapData(expectedDataByte));
            Assert.AreEqual(decodedStr, actualStr);
            Assert.AreEqual(expectedDataByte, actualDataBytes, "Deserialized byte array do not match utf-8 encoding");
        }

        [Test]
        public void TestNullStringEncodeDecode()
        {
            var nullData = _serializationService.ToData(null);
            var decodedStr = (string) _serializationService.ToObject<object>(nullData);
            Assert.IsNull(decodedStr);
        }

        [Test]
        public void TestNullStringEncodeDecode2()
        {
            var objectDataOutput = _serializationService.CreateObjectDataOutput(256);
            objectDataOutput.WriteString(null);
            var bytes = objectDataOutput.ToByteArray();
            var objectDataInput = _serializationService.CreateObjectDataInput(bytes);
            var decodedStr = objectDataInput.ReadString();
            Assert.IsNull(decodedStr);
        }

        [Test]
        public void TestStringAllCharLetterDecode()
        {
            var allstr = new string(AllChars);
            var expected = Encoding.UTF8.GetBytes(allstr);
            IData data = new HeapData(ToDataByte(expected));
            var actualStr = _serializationService.ToObject<string>(data);
            Assert.AreEqual(allstr, actualStr);
        }

        [Test]
        public void TestStringAllCharLetterEncode()
        {
            var allstr = new string(AllChars);
            var expected = Encoding.UTF8.GetBytes(allstr);
            var bytes = _serializationService.ToData(allstr).ToByteArray();

            // data offset + length
            var offset = HeapData.DataOffset + BytesExtensions.SizeOfInt;
            var length = bytes.Length - offset;
            var actual = new byte[length];
            Array.Copy(bytes, offset, actual, 0, length);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestStringArrayEncodeDecode()
        {
            var stringArray = new string[TestStrSize];
            for (var i = 0; i < stringArray.Length; i++)
            {
                stringArray[i] = TestDataAll;
            }
            var dataStrArray = _serializationService.ToData(stringArray);
            var actualStr = (string[]) _serializationService.ToObject<object>(dataStrArray);
            Assert.AreEqual(SerializationConstants.ConstantTypeStringArray, dataStrArray.TypeId);
            Assert.AreEqual(stringArray, actualStr);

        }

        [Test]
        public void TestStringDecode()
        {
            var dataByte = ToDataByte(TestDataBytesAll);
            var data = new HeapData(dataByte);
            var actualStr = _serializationService.ToObject(data);
            Assert.AreEqual(TestDataAll, actualStr);
        }

        [Test]
        public void TestStringEncode()
        {
            var expected = ToDataByte(TestDataBytesAll);
            var actual = _serializationService.ToData(TestDataAll).ToByteArray();
            Assert.AreEqual(expected, actual);
        }

        private byte[] ToDataByte(byte[] input)
        {
            // the first 4 byte of type id, 4 byte string length and last 4 byte of partition hashCode
            var endianness = _serializationService.Endianness;

            var bytes = new byte[3 * BytesExtensions.SizeOfInt + input.Length];
            var pos = 0;
            bytes.WriteInt(pos, 0, endianness);
            pos += BytesExtensions.SizeOfInt;
            // even when serialization service is configured with little endian byte order,
            // the serializerTypeId (CONSTANT_TYPE_STRING) is still output in BIG_ENDIAN
            bytes.WriteInt(pos, SerializationConstants.ConstantTypeString, Endianness.BigEndian);
            pos += BytesExtensions.SizeOfInt;
            bytes.WriteInt(pos, input.Length, endianness);
            pos += BytesExtensions.SizeOfInt;
            input.CopyTo(bytes, pos);
            return bytes;
        }
    }
}
