using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    public abstract class TxnMultiMapRequest : IPortable
    {
        internal string name;

        protected internal TxnMultiMapRequest()
        {
        }

        protected internal TxnMultiMapRequest(string name)
        {
            this.name = name;
        }

        public virtual int GetFactoryId()
        {
            return MultiMapPortableHook.FId;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadPortable(IPortableReader reader)
        {
            name = reader.ReadUTF("n");
        }

        public abstract int GetClassId();
    }
}