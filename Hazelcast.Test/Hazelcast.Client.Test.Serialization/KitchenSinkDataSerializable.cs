﻿// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

#pragma warning disable 659 //No need for GetHashCode()

namespace Hazelcast.Client.Test.Serialization
{
    // test object that contains all types
    internal class KitchenSinkDataSerializable : IIdentifiedDataSerializable
    {
        private static readonly Random Random = new Random();
        public bool[] BoolArray { get; set; }
        public bool Bool { get; set; }
        public byte[] ByteArray { get; set; }
        public byte Byte { get; set; }
        public char[] CharArray { get; set; }
        public char Char { get; set; }
        public short[] ShortArray { get; set; }
        public short Short { get; set; }
        public int[] IntArray { get; set; }
        public int Int { get; set; }
        public long[] LongArray { get; set; }
        public long Long { get; set; }
        public float[] FloatArray { get; set; }
        public float Float { get; set; }
        public double[] DoubleArray { get; set; }
        public double Double { get; set; }
        public string Chars { get; set; }
        public string String { get; set; }
        public string[] StringArray { get; set; }
        public IIdentifiedDataSerializable Serializable { get; set; }
        public IPortable Portable { get; set; }
        public IPortable[] PortableArray { get; set; }
        public IData Data { get; set; }
        public DateTime DateTime { get; set; }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteBoolean(Bool);
            output.WriteBooleanArray(BoolArray);
            output.WriteByte(Byte);
            output.WriteByteArray(ByteArray);
            output.WriteChar(Char);
            output.WriteCharArray(CharArray);
            output.WriteShort(Short);
            output.WriteShortArray(ShortArray);
            output.WriteInt(Int);
            output.WriteIntArray(IntArray);
            output.WriteLong(Long);
            output.WriteLongArray(LongArray);
            output.WriteFloat(Float);
            output.WriteFloatArray(FloatArray);
            output.WriteDouble(Double);
            output.WriteDoubleArray(DoubleArray);
            output.WriteObject(Serializable);
            //output.WriteObject(SerializableArray);
            //output.WriteObject(Portable);
            //output.WriteObject(PortableArray);
            output.WriteInt(Chars.Length);
            output.WriteChars(Chars);
            output.WriteUTF(String);
            output.WriteUTFArray(StringArray);
            output.WriteObject(DateTime);
        }

        public int GetFactoryId()
        {
            return 1;
        }

        public int GetId()
        {
            return 0;
        }

        public void ReadData(IObjectDataInput input)
        {
            Bool = input.ReadBoolean();
            BoolArray = input.ReadBooleanArray();
            Byte = input.ReadByte();
            ByteArray = input.ReadByteArray();
            Char = input.ReadChar();
            CharArray = input.ReadCharArray();
            Short = input.ReadShort();
            ShortArray = input.ReadShortArray();
            Int = input.ReadInt();
            IntArray = input.ReadIntArray();
            Long = input.ReadLong();
            LongArray = input.ReadLongArray();
            Float = input.ReadFloat();
            FloatArray = input.ReadFloatArray();
            Double = input.ReadDouble();
            DoubleArray = input.ReadDoubleArray();
            Serializable = input.ReadObject<IIdentifiedDataSerializable>();
            //input.ReadObject(SerializableArray);
            //Portable = input.ReadObject<IPortable>();
            //input.ReadObject(PortableArray);
            var charLength = input.ReadInt();
            var chars = new char[charLength];
            for (var i = 0; i < charLength; i++)
            {
                chars[i] = input.ReadChar();
            }
            Chars = new string(chars);
            String = input.ReadUTF();
            StringArray = input.ReadUTFArray();
            DateTime = input.ReadObject<DateTime>();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((KitchenSinkDataSerializable) obj);
        }


        public static KitchenSinkDataSerializable Generate()
        {
            var truncatedDateTime = DateTime.Now;
            // truncate datetime to miliseconds
            truncatedDateTime =
                new DateTime(truncatedDateTime.Ticks - (truncatedDateTime.Ticks%TimeSpan.TicksPerMillisecond));

            return new KitchenSinkDataSerializable
            {
                Bool = TestSupport.RandomBool(),
                BoolArray = TestSupport.RandomArray(TestSupport.RandomBool),
                Byte = TestSupport.RandomByte(),
                ByteArray = TestSupport.RandomBytes(),
                Char = (char) Random.Next(),
                Double = TestSupport.RandomDouble(),
                DoubleArray = TestSupport.RandomArray(TestSupport.RandomDouble),
                Chars = TestSupport.RandomString(),
                CharArray = TestSupport.RandomArray(TestSupport.RandomChar),
                DateTime = truncatedDateTime,
                Float = TestSupport.RandomFloat(),
                FloatArray = TestSupport.RandomArray(TestSupport.RandomFloat),
                Int = TestSupport.RandomInt(),
                IntArray = TestSupport.RandomArray(TestSupport.RandomInt),
                Long = TestSupport.RandomLong(),
                LongArray = TestSupport.RandomArray(TestSupport.RandomLong),
                Short = TestSupport.RandomShort(),
                ShortArray = TestSupport.RandomArray(TestSupport.RandomShort),
                String = TestSupport.RandomString(),
                StringArray = TestSupport.RandomArray(TestSupport.RandomString)
            };
        }

        public override string ToString()
        {
            return
                string.Format(
                    "Bool: {0}, ByteArray: {1}, Byte: {2}, CharArray: {3}, Char: {4}, ShortArray: {5}, Short: {6}, IntArray: {7}, Int: {8}, LongArray: {9}, Long: {10}, FloatArray: {11}, Float: {12}, DoubleArray: {13}, Double: {14}, Chars: {15}, StringUTF8: {16}, Serializable: {17}, Portable: {18}, Data: {19}, DateTime: {20}",
                    Bool, ByteArray, Byte, CharArray, Char, ShortArray, Short, IntArray, Int, LongArray, Long,
                    FloatArray, Float, DoubleArray, Double, Chars, String, Serializable, Portable, Data, DateTime);
        }

        protected bool Equals(KitchenSinkDataSerializable other)
        {
            return BoolArray.SequenceEqual(other.BoolArray) && Bool == other.Bool &&
                   ByteArray.SequenceEqual(other.ByteArray) && Byte == other.Byte &&
                   CharArray.SequenceEqual(other.CharArray) && Char == other.Char &&
                   ShortArray.SequenceEqual(other.ShortArray) &&
                   Short == other.Short && IntArray.SequenceEqual(other.IntArray) && Int == other.Int &&
                   LongArray.SequenceEqual(other.LongArray) && Long == other.Long &&
                   FloatArray.SequenceEqual(other.FloatArray) &&
                   Float.Equals(other.Float) && DoubleArray.SequenceEqual(other.DoubleArray) &&
                   Double.Equals(other.Double) &&
                   string.Equals(Chars, other.Chars) && string.Equals(String, other.String) &&
                   StringArray.SequenceEqual(other.StringArray) &&
                   Equals(Serializable, other.Serializable) && Equals(Portable, other.Portable) &&
                   Equals(Data, other.Data) && DateTime.Equals(other.DateTime);
        }
    }
}