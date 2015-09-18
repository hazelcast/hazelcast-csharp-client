using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Test.Serialization
{
    internal class ObjectCarryingPortable : IPortable
    {
        private object _object;

        public ObjectCarryingPortable()
        {
        }

        public ObjectCarryingPortable(object @object)
        {
            _object = @object;
        }

        public int GetFactoryId()
        {
            return TestSerializationConstants.PORTABLE_FACTORY_ID;
        }

        public int GetClassId()
        {
            return TestSerializationConstants.OBJECT_CARRYING_PORTABLE;
        }

        public void WritePortable(IPortableWriter writer)
        {
            var output = writer.GetRawDataOutput();
            output.WriteObject(_object);
        }

        public void ReadPortable(IPortableReader reader)
        {
            var input = reader.GetRawDataInput();
            _object = input.ReadObject<object>();
        }

        protected bool Equals(ObjectCarryingPortable other)
        {
            return Equals(_object, other._object);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ObjectCarryingPortable) obj);
        }

        public override int GetHashCode()
        {
            return (_object != null ? _object.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return string.Format("O: {0}", _object);
        }
    }
}