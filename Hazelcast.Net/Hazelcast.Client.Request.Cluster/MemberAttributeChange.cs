using System;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Request.Cluster
{
    /// <summary></summary>
    internal class MemberAttributeChange : IDataSerializable
    {
        internal const int DELTA_MEMBER_PROPERTIES_OP_PUT = 2;
        internal const int DELTA_MEMBER_PROPERTIES_OP_REMOVE = 3;

        private String key;
        private MapOperationType operationType;
        private String uuid;
        private Object value;

        public object Value
        {
            get { return value; }
        }

        public string Uuid
        {
            get { return uuid; }
        }

        public MapOperationType OperationType
        {
            get { return operationType; }
        }

        public string Key
        {
            get { return key; }
        }

        public MemberAttributeChange()
        {
        }

        public MemberAttributeChange(String uuid, MapOperationType operationType, String key, Object value)
        {
            this.uuid = uuid;
            this.operationType = operationType;
            this.key = key;
            this.value = value;
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(uuid);
            output.WriteUTF(key);
            switch (operationType)
            {
                case MapOperationType.PUT:
                    output.WriteByte(DELTA_MEMBER_PROPERTIES_OP_PUT);
                    IOUtil.WriteAttributeValue(value, output);
                    break;
                case MapOperationType.REMOVE:
                    output.WriteByte(DELTA_MEMBER_PROPERTIES_OP_REMOVE);
                    break;
            }
        }

        public void ReadData(IObjectDataInput input)
        {
            uuid = input.ReadUTF();
            key = input.ReadUTF();
            int operation = input.ReadByte();
            switch (operation)
            {
                case DELTA_MEMBER_PROPERTIES_OP_PUT:
                    operationType = MapOperationType.PUT;
                    value = IOUtil.ReadAttributeValue(input);
                    break;
                case DELTA_MEMBER_PROPERTIES_OP_REMOVE:
                    operationType = MapOperationType.REMOVE;
                    break;
            }
        }

        public string GetJavaClassName()
        {
            return "com.hazelcast.cluster.client.MemberAttributeChange";
        }
    }
}