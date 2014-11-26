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

        public MemberAttributeChange(string uuid, MemberAttributeOperationType operationType
            , string key, object value)
        {
            this.uuid = uuid;
            this.operationType = operationType;
            this.key = key;
            this.value = value;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteData(IObjectDataOutput @out)
        {
            @out.WriteUTF(uuid);
            @out.WriteUTF(key);
            @out.WriteByte((int) operationType);
            if (operationType == MemberAttributeOperationType.PUT)
            {
                IOUtil.WriteAttributeValue(value, @out);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void ReadData(IObjectDataInput @in)
        {
            uuid = @in.ReadUTF();
            key = @in.ReadUTF();

            operationType = (MemberAttributeOperationType) @in.ReadByte();
            if (operationType == MemberAttributeOperationType.PUT)
            {
                value = IOUtil.ReadAttributeValue(@in);
            }
        }

        public string GetJavaClassName()
        {
            return "com.hazelcast.cluster.client.MemberAttributeChange";
        }

        public virtual string GetUuid()
        {
            return uuid;
        }

        public virtual MemberAttributeOperationType GetOperationType()
        {
            return operationType;
        }

        public virtual string GetKey()
        {
            return key;
        }

        public virtual object GetValue()
        {
            return value;
        }
    }
}