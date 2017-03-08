using Hazelcast.Client.Test;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Map;

namespace Hazelcast.Client.Model
{
    internal class IdentifiedEntryProcessor : IIdentifiedDataSerializable, IEntryProcessor
    {
        internal const int ClassId = 1;

        private string value;

        public IdentifiedEntryProcessor(string value = null)
        {
            this.value = value;
        }

        public void ReadData(IObjectDataInput input)
        {
            value = input.ReadUTF();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(value);
        }

        public int GetFactoryId()
        {
            return IdentifiedFactory.FactoryId;
        }

        public int GetId()
        {
            return ClassId;
        }
    }
}