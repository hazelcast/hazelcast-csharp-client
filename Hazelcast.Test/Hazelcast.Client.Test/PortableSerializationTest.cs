using Hazelcast.Client.Model;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class PortableSerializationTest //:HazelcastBaseTest
    {
        internal static readonly int FACTORY_ID = 1;

        [Test]
        public void testBasicPortable()
        {
            var ss = createSerializationService(1, ByteOrder.BigEndian);

            var main = new MainPortable(113, true, 'x', -500, 56789, -50992225L, 900.5678f, -897543.3678909d,
                "this is main portable object created for testing!");

            var data = ss.ToData(main);
            Assert.AreEqual(main, ss.ToObject<MainPortable>(data));
        }

        private static ISerializationService createSerializationService(int version, ByteOrder order)
        {
            return new SerializationServiceBuilder()
                .SetUseNativeByteOrder(false).SetByteOrder(order).SetVersion(version)
                .AddPortableFactory(FACTORY_ID, new TestPortableFactory()).Build();
        }
    }

    internal class TestPortableFactory : IPortableFactory
    {
        public IPortable Create(int classId)
        {
            switch (classId)
            {
                case MainPortable.CLASS_ID:
                    return new MainPortable();
                //case InnerPortable.CLASS_ID:
                //    return new InnerPortable();
                //case NamedPortable.CLASS_ID:
                //    return new NamedPortable();
                //case RawDataPortable.CLASS_ID:
                //    return new RawDataPortable();
                //case InvalidRawDataPortable.CLASS_ID:
                //    return new InvalidRawDataPortable();
                //case InvalidRawDataPortable2.CLASS_ID:
                //    return new InvalidRawDataPortable2();
            }
            return null;
        }
    }

    internal class MainPortable : IPortable
    {
        internal const short CLASS_ID = 1;
        private byte b;
        private byte[] bb;
        private bool bo;
        private char c;
        private double d;
        private float f;
        private int i;
        private long l;
        private short s;
        private string str;

        internal MainPortable()
        {
        }

        internal MainPortable(byte b, bool bo, char c, short s, int i, long l, float f, double d, string str)
        {
            this.b = b;
            this.bo = bo;
            this.c = c;
            this.s = s;
            this.i = i;
            this.l = l;
            this.f = f;
            this.d = d;
            this.str = str;
        }

        public int GetFactoryId()
        {
            return PortableSerializationTest.FACTORY_ID;
        }

        public int GetClassId()
        {
            return CLASS_ID;
        }

        public void WritePortable(IPortableWriter writer)
        {
            writer.WriteByte("b", b);
            writer.WriteBoolean("bool", bo);
            writer.WriteChar("c", c);
            writer.WriteShort("s", s);
            writer.WriteInt("i", i);
            writer.WriteLong("l", l);
            writer.WriteFloat("f", f);
            writer.WriteDouble("d", d);
            writer.WriteUTF("str", str);
        }

        public void ReadPortable(IPortableReader reader)
        {
            b = reader.ReadByte("b");
            bo = reader.ReadBoolean("bool");
            c = reader.ReadChar("c");
            s = reader.ReadShort("s");
            i = reader.ReadInt("i");
            l = reader.ReadLong("l");
            f = reader.ReadFloat("f");
            d = reader.ReadDouble("d");
            str = reader.ReadUTF("str");
        }

        protected bool Equals(MainPortable other)
        {
            return b == other.b && Equals(bb, other.bb) && bo.Equals(other.bo) && c == other.c && d.Equals(other.d) &&
                   f.Equals(other.f) && i == other.i && l == other.l && s == other.s && string.Equals(str, other.str);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MainPortable) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = b.GetHashCode();
                hashCode = (hashCode*397) ^ (bb != null ? bb.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ bo.GetHashCode();
                hashCode = (hashCode*397) ^ c.GetHashCode();
                hashCode = (hashCode*397) ^ d.GetHashCode();
                hashCode = (hashCode*397) ^ f.GetHashCode();
                hashCode = (hashCode*397) ^ i;
                hashCode = (hashCode*397) ^ l.GetHashCode();
                hashCode = (hashCode*397) ^ s.GetHashCode();
                hashCode = (hashCode*397) ^ (str != null ? str.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}