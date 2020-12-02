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
using Hazelcast.Core;
using Hazelcast.Serialization;
using Hazelcast.Serialization.ConstantSerializers;
using Hazelcast.Serialization.DefaultSerializers;
using Hazelcast.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization
{
    public class StringSerializationTest
    {
        private const string TestDataTurkish = "Pijamalı hasta, yağız şoföre çabucak güvendi.";
        private const string TestDataJapanese = "イロハニホヘト チリヌルヲ ワカヨタレソ ツネナラム";
        private const string TestDataAscii = "The quick brown fox jumps over the lazy dog";
        private const string TestDataAll = TestDataTurkish + TestDataJapanese + TestDataAscii;
        private const int TestStrSize = 1 << 20;
        private static readonly byte[] TestDataBytesAll = Encoding.UTF8.GetBytes(TestDataAll);
        private static readonly char[] AllChars;
        private ISerializationService _serializationService;

        static StringSerializationTest()
        {
            var chars = new List<char>(char.MaxValue);
            for (var c = 0; c < char.MaxValue; c++)
            {
                if (char.IsSurrogate((char) c)) break;

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
            _serializationService = new SerializationServiceBuilder(new NullLoggerFactory())
                .AddDefinitions(new ConstantSerializerDefinitions())
                .AddDefinitions(new DefaultSerializerDefinitions())
                .Build();
        }

        [TearDown]
        public virtual void TearDown()
        {
            _serializationService.Destroy();
        }

        [Test]
        public virtual void TestLargeStringEncodeDecode()
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
            var expectedDataByte = ToDataByte(strBytes, actualStr.Length);
            var decodedStr = (string) _serializationService.ToObject<object>(new HeapData(expectedDataByte));
            Assert.AreEqual(expectedDataByte, actualDataBytes, "Deserialized byte array do not match utf-8 encoding");
            Assert.AreEqual(decodedStr, actualStr);
        }

        [Test]
        public virtual void TestNullStringEncodeDecode()
        {
            var nullData = _serializationService.ToData(null);
            var decodedStr = (string) _serializationService.ToObject<object>(nullData);
            Assert.IsNull(decodedStr);
        }

        [Test]
        public virtual void TestNullStringEncodeDecode2()
        {
            var objectDataOutput = _serializationService.CreateObjectDataOutput(256);
            objectDataOutput.Write(null);
            var bytes = objectDataOutput.ToByteArray();
            var objectDataInput = _serializationService.CreateObjectDataInput(bytes);
            var decodedStr = objectDataInput.ReadString();
            Assert.IsNull(decodedStr);
        }

        [Test]
        public virtual void TestStringAllCharLetterDecode()
        {
            var allstr = new string(AllChars);
            var expected = Encoding.UTF8.GetBytes(allstr);
            IData data = new HeapData(ToDataByte(expected, expected.Length));
            var actualStr = (string) _serializationService.ToObject<object>(data);
            Assert.AreEqual(allstr, actualStr);
        }

        [Test]
        public virtual void TestStringAllCharLetterEncode()
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
        public void TestStringSurrogateCharEncode()
        {
            var data = _serializationService.ToData(Utf8Char.FourBytes);
            var s = _serializationService.ToObject<string>(data);

            Assert.That(s, Is.EqualTo(Utf8Char.FourBytes));
        }

        [Test]
        public virtual void TestStringArrayEncodeDecode()
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
        public virtual void TestStringDecode()
        {
            IData data = new HeapData(ToDataByte(TestDataBytesAll, TestDataBytesAll.Length));
            var actualStr = (string) _serializationService.ToObject<object>(data);
            Assert.AreEqual(TestDataAll, actualStr);
        }

        [Test]
        public virtual void TestStringEncode()
        {
            var expected = ToDataByte(TestDataBytesAll, TestDataBytesAll.Length);
            var actual = _serializationService.ToData(TestDataAll).ToByteArray();
            Assert.AreEqual(expected, actual);
        }

        private static byte[] ToDataByte(byte[] input, int length)
        {
            var bytes = new byte[3 * BytesExtensions.SizeOfInt + input.Length];
            var pos = 0;
            bytes.WriteInt(pos, 0);
            pos += BytesExtensions.SizeOfInt;
            bytes.WriteInt(pos, SerializationConstants.ConstantTypeString);
            pos += BytesExtensions.SizeOfInt;
            bytes.WriteInt(pos, length);
            pos += BytesExtensions.SizeOfInt;
            input.CopyTo(bytes, pos);
            return bytes;
        }
    }
}
