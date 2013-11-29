using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    public abstract class TxnCollectionRequest : IPortable
    {
        internal string name;

        internal Data value;

        public TxnCollectionRequest()
        {
        }

        public TxnCollectionRequest(string name)
        {
            ///*
            this.name = name;
        }

        public TxnCollectionRequest(string name, Data value) : this(name)
        {
            this.value = value;
        }

        public virtual int GetFactoryId()
        {
            return CollectionPortableHook.FId;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            IOUtil.WriteNullableData(writer.GetRawDataOutput(), value);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            name = reader.ReadUTF("n");
            value = IOUtil.ReadNullableData(reader.GetRawDataInput());
        }

        public abstract int GetClassId();
    }
}