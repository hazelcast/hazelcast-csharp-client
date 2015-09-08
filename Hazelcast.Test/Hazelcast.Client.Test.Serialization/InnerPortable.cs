using System;
using System.Linq;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Test.Serialization
{
    internal class InnerPortable : IPortable
    {
        private byte[] bb;
        private char[] cc;
        private double[] dd;
        private float[] ff;
        private int[] ii;
        private long[] ll;
        private NamedPortable[] nn;
        private short[] ss;

        public InnerPortable()
        {
        }

        public InnerPortable(byte[] bb, char[] cc, short[] ss, int[] ii, long[] ll, float[] ff, double[] dd,
            NamedPortable[] nn)
        {
            this.bb = bb;
            this.cc = cc;
            this.ss = ss;
            this.ii = ii;
            this.ll = ll;
            this.ff = ff;
            this.dd = dd;
            this.nn = nn;
        }

        public int GetClassId()
        {
            return TestSerializationConstants.INNER_PORTABLE;
        }

        public void WritePortable(IPortableWriter writer)
        {
            writer.WriteByteArray("b", bb);
            writer.WriteCharArray("c", cc);
            writer.WriteShortArray("s", ss);
            writer.WriteIntArray("i", ii);
            writer.WriteLongArray("l", ll);
            writer.WriteFloatArray("f", ff);
            writer.WriteDoubleArray("d", dd);
            writer.WritePortableArray("nn", nn);
        }

        public void ReadPortable(IPortableReader reader)
        {
            bb = reader.ReadByteArray("b");
            cc = reader.ReadCharArray("c");
            ss = reader.ReadShortArray("s");
            ii = reader.ReadIntArray("i");
            ll = reader.ReadLongArray("l");
            ff = reader.ReadFloatArray("f");
            dd = reader.ReadDoubleArray("d");
            var pp = reader.ReadPortableArray("nn");
            nn = new NamedPortable[pp.Length];
            Array.Copy(pp, 0, nn, 0, nn.Length);
        }

        public int GetFactoryId()
        {
            return TestSerializationConstants.PORTABLE_FACTORY_ID;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((InnerPortable) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (bb != null ? bb.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (cc != null ? cc.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (dd != null ? dd.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ff != null ? ff.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ii != null ? ii.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ll != null ? ll.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (nn != null ? nn.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ss != null ? ss.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("Bb: {0}, Cc: {1}, Dd: {2}, Ff: {3}, Ii: {4}, Ll: {5}, Nn: {6}, Ss: {7}", bb, cc, dd,
                ff, ii, ll, nn, ss);
        }

        protected bool Equals(InnerPortable other)
        {
            return Enumerable.SequenceEqual(bb, other.bb) && Enumerable.SequenceEqual(cc, other.cc) && Enumerable.SequenceEqual(dd, other.dd) && Enumerable.SequenceEqual(ff, other.ff) &&
                   Enumerable.SequenceEqual(ii, other.ii) && Enumerable.SequenceEqual(ll, other.ll) && Enumerable.SequenceEqual(nn, other.nn) && Enumerable.SequenceEqual(ss, other.ss);
        }
    }
}