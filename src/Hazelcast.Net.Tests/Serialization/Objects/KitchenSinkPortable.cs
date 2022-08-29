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
using System.Linq;
using Hazelcast.Serialization;
using Hazelcast.Testing;

#pragma warning disable 659 //No need for GetHashCode()

namespace Hazelcast.Tests.Serialization.Objects
{
    internal class KitchenSinkPortable : IPortable
    {
        private static readonly Random Random = new Random();
        public bool Bool { get; set; }
        public bool[] BoolArray { get; set; }
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
        public string String { get; set; }
        public string[] StringArray { get; set; }

        public int FactoryId => KitchenSinkPortableFactory.FactoryId;

        public int ClassId => 1;

        public void WritePortable(IPortableWriter writer)
        {
            writer.WriteBoolean("bool", Bool);
            writer.WriteBooleanArray("boolArray", BoolArray);
            writer.WriteByte("byte", Byte);
            writer.WriteByteArray("byteArray", ByteArray);
            writer.WriteChar("char", Char);
            writer.WriteCharArray("charArray", CharArray);
            writer.WriteShort("short", Short);
            writer.WriteShortArray("shortArray", ShortArray);
            writer.WriteInt("int", Int);
            writer.WriteIntArray("intArray", IntArray);
            writer.WriteLong("long", Long);
            writer.WriteLongArray("longArray", LongArray);
            writer.WriteFloat("float", Float);
            writer.WriteFloatArray("floatArray", FloatArray);
            writer.WriteDouble("double", Double);
            writer.WriteDoubleArray("doubleArray", DoubleArray);
            writer.WriteString("string", String);
            writer.WriteStringArray("stringArray", StringArray);
        }

        public void ReadPortable(IPortableReader reader)
        {
            Bool = reader.ReadBoolean("bool");
            BoolArray = reader.ReadBooleanArray("boolArray");
            Byte = reader.ReadByte("byte");
            ByteArray = reader.ReadByteArray("byteArray");
            Char = reader.ReadChar("char");
            CharArray = reader.ReadCharArray("charArray");
            Short = reader.ReadShort("short");
            ShortArray = reader.ReadShortArray("shortArray");
            Int = reader.ReadInt("int");
            IntArray = reader.ReadIntArray("intArray");
            Long = reader.ReadLong("long");
            LongArray = reader.ReadLongArray("longArray");
            Float = reader.ReadFloat("float");
            FloatArray = reader.ReadFloatArray("floatArray");
            Double = reader.ReadDouble("double");
            DoubleArray = reader.ReadDoubleArray("doubleArray");
            String = reader.ReadString("string");
            StringArray = reader.ReadStringArray("stringArray");
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((KitchenSinkPortable)obj);
        }
             
        /// <summary>
        /// Generates a portable object with data. Also used to size strings in the string arrays.
        /// n(where 0<n<=len(randomstring)) length of string array will be like["x", "xx", "xxx", ....., "xxx..xxx"].
        /// It will allow to test deseriazliser with various size of strings.
        /// </summary>
        /// <param name="arraySize">arraySize">Size of for array type fields</param>
        /// <returns></returns>
        public static KitchenSinkPortable Generate(int arraySize = 5)
        {
            return new KitchenSinkPortable
            {
                Bool = TestUtils.RandomBool(),
                BoolArray = TestUtils.RandomArray(TestUtils.RandomBool),
                Byte = TestUtils.RandomByte(),
                ByteArray = TestUtils.RandomBytes(),
                Char = (char)Random.Next(),
                Double = TestUtils.RandomDouble(),
                DoubleArray = TestUtils.RandomArray(TestUtils.RandomDouble, arraySize),
                CharArray = TestUtils.RandomArray(TestUtils.RandomChar, arraySize),
                Float = TestUtils.RandomFloat(),
                FloatArray = TestUtils.RandomArray(TestUtils.RandomFloat, arraySize),
                Int = TestUtils.RandomInt(),
                IntArray = TestUtils.RandomArray(TestUtils.RandomInt, arraySize),
                Long = TestUtils.RandomLong(),
                LongArray = TestUtils.RandomArray(TestUtils.RandomLong, arraySize),
                Short = TestUtils.RandomShort(),
                ShortArray = TestUtils.RandomArray(TestUtils.RandomShort, arraySize),
                String = TestUtils.RandomString(),
                StringArray = TestUtils.RandomArray(TestUtils.RandomStringWithLength, arraySize)
            };
        }

        public override string ToString()
        {
            return
                string.Format(
                    "Bool: {0}, BoolArray: {1}, ByteArray: {2}, Byte: {3}, CharArray: {4}, Char: {5}, ShortArray: {6}, Short: {7}, IntArray: {8}, Int: {9}, LongArray: {10}, Long: {11}, FloatArray: {12}, Float: {13}, DoubleArray: {14}, Double: {15}, String: {16}, StringArray: {17}, ",
                    Bool, BoolArray, ByteArray, Byte, CharArray, Char, ShortArray, Short, IntArray, Int, LongArray, Long,
                    FloatArray, Float, DoubleArray, Double, String, StringArray);
        }

        protected bool Equals(KitchenSinkPortable other)
        {
            return Bool == other.Bool && BoolArray.SequenceEqual(other.BoolArray) &&
                   ByteArray.SequenceEqual(other.ByteArray) && Byte == other.Byte &&
                   CharArray.SequenceEqual(other.CharArray) && Char == other.Char &&
                   ShortArray.SequenceEqual(other.ShortArray) &&
                   Short == other.Short && IntArray.SequenceEqual(other.IntArray) && Int == other.Int &&
                   LongArray.SequenceEqual(other.LongArray) && Long == other.Long &&
                   FloatArray.SequenceEqual(other.FloatArray) &&
                   Float.Equals(other.Float) && DoubleArray.SequenceEqual(other.DoubleArray) &&
                   Double.Equals(other.Double)
                   && string.Equals(String, other.String) && StringArray.SequenceEqual(other.StringArray)
                ;
        }
    }

    public class KitchenSinkPortableFactory : IPortableFactory
    {
        public const int FactoryId = 1;

        public IPortable Create(int classId)
        {
            return new KitchenSinkPortable();
        }
    }
}
