using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    /// <author>ali 5/10/13</author>
    public class ContainsRequest : MultiMapAllPartitionRequest, IRetryableRequest
    {
        internal IData value;

        public ContainsRequest()
        {
        }

        public ContainsRequest(string name, IData value) : base(name)
        {
            this.value = value;
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.CONTAINS_ENTRY;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            IObjectDataOutput @out = writer.GetRawDataOutput();
            @out.WriteData(value);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Read(IPortableReader reader)
        {
            base.Read(reader);
            IObjectDataInput @in = reader.GetRawDataInput();
            value = @in.ReadData();
        }
    }
}