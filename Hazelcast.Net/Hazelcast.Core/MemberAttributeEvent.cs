using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hazelcast.Client.Request.Cluster;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Core
{
    public class MemberAttributeEvent:MembershipEvent,IDataSerializable
    {
        private MapOperationType operationType;
        private String key;
        private object value;
        private Member member;

        public MemberAttributeEvent():base(null, null, MemberAttributeChanged, null){ }

        public MemberAttributeEvent(ICluster cluster, IMember member, MapOperationType operationType, String key, Object value)
            : base(cluster, member, MemberAttributeChanged, null)
        {
            this.member = (Member) member;
            this.operationType = operationType;
            this.key = key;
            this.value = value;
        }

        public MapOperationType GetOperationType()
        {
            return operationType;
        }

        public String GetKey()
        {
            return key;
        }

        public object GetValue()
        {
            return value;
        }

        public override  IMember GetMember()
        {
            return member;
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteUTF(key);
            member.WriteData(output);
            switch (operationType)
            {
                case MapOperationType.PUT:
                    output.WriteByte(MemberAttributeChange.DELTA_MEMBER_PROPERTIES_OP_PUT);
                    output.WriteObject(value);
                    break;
                case MapOperationType.REMOVE:
                    output.WriteByte(MemberAttributeChange.DELTA_MEMBER_PROPERTIES_OP_REMOVE);
                    break;
            }
        }

        public void ReadData(IObjectDataInput input)
        {
            key = input.ReadUTF();
            member = new Member();
            member.ReadData(input);
            int operation = input.ReadByte();
            switch (operation)
            {
                case MemberAttributeChange.DELTA_MEMBER_PROPERTIES_OP_PUT:
                    operationType = MapOperationType.PUT;
                    value = IOUtil.ReadAttributeValue(input);
                    break;
                case MemberAttributeChange.DELTA_MEMBER_PROPERTIES_OP_REMOVE:
                    operationType = MapOperationType.REMOVE;
                    break;
                default:
                    throw new NotSupportedException("Unknown operation type received: " + operationType);
            }
            this.Source = member;
        }

        public string GetJavaClassName()
        {
            return "com.hazelcast.core.MemberAttributeEvent";
        }
    }
}
