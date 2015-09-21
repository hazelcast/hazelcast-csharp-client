using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Test.Serialization
{
    internal class NamedPortableV2 : NamedPortable, IVersionedPortable
    {
        private int v;

        public NamedPortableV2()
        {
        }

        public NamedPortableV2(string name, int v) : base(name, v*10)
        {
            this.v = v;
        }
       

        public override void WritePortable(IPortableWriter writer)
        {
            base.WritePortable(writer);
            writer.WriteInt("v", v);
        }

        public override void ReadPortable(IPortableReader reader)
        {
            base.ReadPortable(reader);
            v = reader.ReadInt("v");
        }

        public int GetClassVersion()
        {
            return 2;
        }

        protected bool Equals(NamedPortableV2 other)
        {
            return base.Equals(other) && v == other.v;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() == typeof(NamedPortable)) return base.Equals((NamedPortable)obj);
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NamedPortableV2) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ v;
            }
        }
    }
}