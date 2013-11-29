using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;

namespace Hazelcast.Client.Request.Multimap
{
    public class ContainsEntryRequest : MultiMapAllPartitionRequest, IRetryableRequest
    {
        internal Data key;

        internal Data value;

        public ContainsEntryRequest()
        {
        }

        public ContainsEntryRequest(string name, Data key, Data value) : base(name)
        {
            this.key = key;
            this.value = value;
        }

        public override int GetClassId()
        {
            return MultiMapPortableHook.ContainsEntry;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WritePortable(IPortableWriter writer)
        {
            base.WritePortable(writer);
            IObjectDataOutput output = writer.GetRawDataOutput();
            IOUtil.WriteNullableData(output, key);
            IOUtil.WriteNullableData(output, value);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void ReadPortable(IPortableReader reader)
        {
            base.ReadPortable(reader);
            IObjectDataInput input = reader.GetRawDataInput();
            key = IOUtil.ReadNullableData(input);
            value = IOUtil.ReadNullableData(input);
        }
    }
}