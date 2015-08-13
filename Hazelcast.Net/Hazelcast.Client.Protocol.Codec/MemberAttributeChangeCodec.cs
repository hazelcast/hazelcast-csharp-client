using Hazelcast.Client.Protocol.Util;
using Hazelcast.Client.Spi;
using Hazelcast.IO;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class MemberAttributeChangeCodec
    {
        private MemberAttributeChangeCodec()
        {
        }

        public static MemberAttributeChange Decode(IClientMessage clientMessage)
        {
            var uuid = clientMessage.GetStringUtf8();
            var key = clientMessage.GetStringUtf8();
            var operationType = (MemberAttributeOperationType) clientMessage.GetInt();
            string value = null;
            if (operationType == MemberAttributeOperationType.PUT)
            {
                value = clientMessage.GetStringUtf8();
            }
            return new MemberAttributeChange(uuid, operationType, key, value);
        }

        public static void Encode(MemberAttributeChange memberAttributeChange, ClientMessage clientMessage)
        {
            clientMessage.Set(memberAttributeChange.Uuid);
            clientMessage.Set(memberAttributeChange.Key);
            var operationType = memberAttributeChange.OperationType;
            clientMessage.Set((int) operationType);
            if (operationType == MemberAttributeOperationType.PUT)
            {
                clientMessage.Set(memberAttributeChange.Value);
            }
        }

        public static int CalculateDataSize(MemberAttributeChange memberAttributeChange)
        {
            if (memberAttributeChange == null)
            {
                return Bits.BooleanSizeInBytes;
            }
            var dataSize = ParameterUtil.CalculateDataSize(memberAttributeChange.Uuid);
            dataSize += ParameterUtil.CalculateDataSize(memberAttributeChange.Key);
            //operation type
            dataSize += Bits.IntSizeInBytes;
            var operationType = memberAttributeChange.OperationType;
            if (operationType == MemberAttributeOperationType.PUT)
            {
                dataSize += ParameterUtil.CalculateDataSize(memberAttributeChange.Value);
            }
            return dataSize;
        }
    }
}