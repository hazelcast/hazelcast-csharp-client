using System;
using System.Linq;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Test.Serialization
{
    internal class KitchenSinkPortable : IPortable
    {
        private static readonly Random Random = new Random();
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
        public string StringUTF8 { get; set; }
        public IDataSerializable Serializable { get; set; }

        public int GetFactoryId()
        {
            return KitchenSinkPortableFactory.FactoryId;
        }

        public int GetClassId()
        {
            return 1;
        }

        public void WritePortable(IPortableWriter writer)
        {
            writer.WriteBoolean("bool", Bool);
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
            writer.WriteUTF("stringUTF8", StringUTF8);
        }

        public void ReadPortable(IPortableReader reader)
        {
            Bool = reader.ReadBoolean("bool");
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
            StringUTF8 = reader.ReadUTF("stringUTF8");
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((KitchenSinkPortable)obj);
        }

        public static KitchenSinkPortable Generate()
        {
            return new KitchenSinkPortable
            {
                Bool = TestSupport.RandomBool(),
                Byte = TestSupport.RandomByte(),
                ByteArray = TestSupport.RandomBytes(),
                Char = (char) Random.Next(),
                Double = TestSupport.RandomDouble(),
                DoubleArray = TestSupport.RandomArray(TestSupport.RandomDouble),
                CharArray = TestSupport.RandomArray(TestSupport.RandomChar),
                Float = TestSupport.RandomFloat(),
                FloatArray = TestSupport.RandomArray(TestSupport.RandomFloat),
                Int = TestSupport.RandomInt(),
                IntArray = TestSupport.RandomArray(TestSupport.RandomInt),
                Long = TestSupport.RandomLong(),
                LongArray = TestSupport.RandomArray(TestSupport.RandomLong),
                Short = TestSupport.RandomShort(),
                ShortArray = TestSupport.RandomArray(TestSupport.RandomShort),
                StringUTF8 = TestSupport.RandomString()
            };
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Bool.GetHashCode();
                hashCode = (hashCode*397) ^ (ByteArray != null ? ByteArray.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Byte.GetHashCode();
                hashCode = (hashCode*397) ^ (CharArray != null ? CharArray.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Char.GetHashCode();
                hashCode = (hashCode*397) ^ (ShortArray != null ? ShortArray.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Short.GetHashCode();
                hashCode = (hashCode*397) ^ (IntArray != null ? IntArray.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Int;
                hashCode = (hashCode*397) ^ (LongArray != null ? LongArray.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Long.GetHashCode();
                hashCode = (hashCode*397) ^ (FloatArray != null ? FloatArray.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Float.GetHashCode();
                hashCode = (hashCode*397) ^ (DoubleArray != null ? DoubleArray.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Double.GetHashCode();
                hashCode = (hashCode*397) ^ (StringUTF8 != null ? StringUTF8.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Serializable != null ? Serializable.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return
                string.Format(
                    "Bool: {0}, ByteArray: {1}, Byte: {2}, CharArray: {3}, Char: {4}, ShortArray: {5}, Short: {6}, IntArray: {7}, Int: {8}, LongArray: {9}, Long: {10}, FloatArray: {11}, Float: {12}, DoubleArray: {13}, Double: {14}, StringUTF8: {15}, Serializable: {16}, Portable: {17}, PortableArray: {18}",
                    Bool, ByteArray, Byte, CharArray, Char, ShortArray, Short, IntArray, Int, LongArray, Long,
                    FloatArray, Float, DoubleArray, Double, StringUTF8, Serializable);
        }

        protected bool Equals(KitchenSinkPortable other)
        {
            return Bool == other.Bool && ByteArray.SequenceEqual(other.ByteArray) && Byte == other.Byte &&
                   CharArray.SequenceEqual(other.CharArray) && Char == other.Char &&
                   ShortArray.SequenceEqual(other.ShortArray) &&
                   Short == other.Short && IntArray.SequenceEqual(other.IntArray) && Int == other.Int &&
                   LongArray.SequenceEqual(other.LongArray) && Long == other.Long &&
                   FloatArray.SequenceEqual(other.FloatArray) &&
                   Float.Equals(other.Float) && DoubleArray.SequenceEqual(other.DoubleArray) &&
                   Double.Equals(other.Double) 
                   && string.Equals(StringUTF8, other.StringUTF8) &&
                   Equals(Serializable, other.Serializable)
                ;
        }
    }

    public class KitchenSinkPortableFactory : IPortableFactory
    {
        public IPortable Create(int classId)
        {
            return new KitchenSinkPortable();
        }

        public const int FactoryId = 1;
    }
}