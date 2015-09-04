using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Test.Serialization
{
    internal class NamedPortable : IPortable
    {
        private int k;
        private string name;

        public NamedPortable()
        {
        }

        public NamedPortable(string name, int k)
        {
            this.name = name;
            this.k = k;
        }

        public int GetFactoryId()
        {
            return TestSerializationConstants.PORTABLE_FACTORY_ID;
        }

        public int GetClassId()
        {
            return TestSerializationConstants.NAMED_PORTABLE;
        }

        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("name", name);
            writer.WriteInt("myint", k);
        }

        public virtual void ReadPortable(IPortableReader reader)
        {
            k = reader.ReadInt("myint");
            name = reader.ReadUTF("name");
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((NamedPortable) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((name != null ? name.GetHashCode() : 0)*397) ^ k;
            }
        }

        public override string ToString()
        {
            return string.Format("K: {0}, Name: {1}", k, name);
        }

        protected bool Equals(NamedPortable other)
        {
            return string.Equals(name, other.name) && k == other.k;
        }
    }
}