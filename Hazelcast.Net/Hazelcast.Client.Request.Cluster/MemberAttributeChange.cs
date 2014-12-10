using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Request.Cluster
{
    internal class MemberAttributeChange : IDataSerializable
    {
        private string key;
        private MemberAttributeOperationType operationType;
        private string uuid;
        private object value;

        public MemberAttributeChange()
        {
        }

        public object Value
        {
            get { return value; }
        }

        public string Uuid
        {
            get { return uuid; }
        }

        public MemberAttributeOperationType OperationType
        {
            get { return operationType; }
        }

        public string Key
        {
            get { return key; }
        }


        public MemberAttributeChange(string uuid, MemberAttributeOperationType operationType
            , string key, object value)
        {
            this.uuid = uuid;
            this.operationType = operationType;
            this.key = key;
            this.value = value;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(uuid);
            output.WriteUTF(key);
            output.WriteByte((int) operationType);
            if (operationType == MemberAttributeOperationType.PUT)
            {
                IOUtil.WriteAttributeValue(value, output);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadData(IObjectDataInput input)
        {
            uuid = input.ReadUTF();
            key = input.ReadUTF();

            operationType = (MemberAttributeOperationType) input.ReadByte();
            if (operationType == MemberAttributeOperationType.PUT)
            {
                value = IOUtil.ReadAttributeValue(input);
            }
        }

        public string GetJavaClassName()
        {
            return "com.hazelcast.cluster.client.MemberAttributeChange";
        }
    }
}