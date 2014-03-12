using Hazelcast.Client.Request.Base;
using Hazelcast.Client.Request.Transaction;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Collection
{
    internal abstract class TxnCollectionRequest : BaseTransactionRequest
    {
        internal string name;

        internal Data value;


        protected TxnCollectionRequest(string name)
        {
            ///*
            this.name = name;
        }

        public TxnCollectionRequest(string name, Data value) : this(name)
        {
            this.value = value;
        }

        public override int GetFactoryId()
        {
            return CollectionPortableHook.FId;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            IOUtil.WriteNullableData(writer.GetRawDataOutput(), value);
        }


    }
}