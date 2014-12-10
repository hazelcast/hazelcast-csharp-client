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
        private MemberAttributeOperationType operationType;
        private String key;
        private object value;
        private Member member;

        public MemberAttributeEvent():base(null, null, MemberAttributeChanged, null){ }

        public MemberAttributeEvent(ICluster cluster, IMember member, MemberAttributeOperationType operationType, String key, Object value)
            : base(cluster, member, MemberAttributeChanged, null)
        {
            this.member = (Member) member;
            this.operationType = operationType;
            this.key = key;
            this.value = value;
        }

        public MemberAttributeOperationType GetOperationType()
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
            output.WriteByte((byte) operationType);
            if (operationType == MemberAttributeOperationType.PUT)
            {
                IOUtil.WriteAttributeValue(value, output);
            }
        }

        public void ReadData(IObjectDataInput input)
        {
            key = input.ReadUTF();
            member = new Member();
            member.ReadData(input);
            operationType = (MemberAttributeOperationType) input.ReadByte();
            if (operationType == MemberAttributeOperationType.PUT) 
            {
                value = IOUtil.ReadAttributeValue(input);
            }
            this.Source = member;
        }

        public string GetJavaClassName()
        {
            return "com.hazelcast.core.MemberAttributeEvent";
        }
    }
}
