using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    internal class ContainsRequest : MultiMapAllPartitionRequest, IRetryableRequest
    {
        internal IData value;

        public ContainsRequest(string name, IData value) : base(name)
        {
            this.value = value;
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.ContainsEntry;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IPortableWriter writer)
        {
            base.Write(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            output.WriteData(value);
        }
    }
}