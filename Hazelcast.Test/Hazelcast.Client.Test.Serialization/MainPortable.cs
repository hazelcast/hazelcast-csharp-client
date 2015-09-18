using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Test.Serialization
{
    internal class MainPortable : IPortable
    {
        private byte b;
        private byte[] bb;
        private bool bo;
        private char c;
        private double d;
        private float f;
        private int i;
        private long l;
        private InnerPortable p;
        private short s;
        private string str;

        internal MainPortable()
        {
        }

        internal MainPortable(byte b, bool bo, char c, short s, int i, long l, float f, double d, string str,
            InnerPortable p)
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
            this.p = p;
        }

        public int GetFactoryId()
        {
            return TestSerializationConstants.PORTABLE_FACTORY_ID;
        }

        public int GetClassId()
        {
            return TestSerializationConstants.MAIN_PORTABLE;
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
            if (p != null)
            {
                writer.WritePortable("p", p);
            }
            else
            {
                writer.WriteNullPortable("p", TestSerializationConstants.PORTABLE_FACTORY_ID,
                    TestSerializationConstants.INNER_PORTABLE);
            }
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
            p = reader.ReadPortable<InnerPortable>("p");
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
                hashCode = (hashCode*397) ^ (p != null ? p.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("B: {0}, Bb: {1}, Bo: {2}, C: {3}, D: {4}, F: {5}, L: {6}, S: {7}, Str: {8}, P: {9}", b,
                bb, bo, c, d, f, l, s, str, p);
        }

        protected bool Equals(MainPortable other)
        {
            return b == other.b && Equals(bb, other.bb) && bo == other.bo && c == other.c && d.Equals(other.d) &&
                   f.Equals(other.f) && i == other.i && l == other.l && s == other.s && string.Equals(str, other.str) &&
                   Equals(p, other.p);
        }
    }
}