using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;
using Hazelcast.Client.Spi;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class ClientMembershipListenerCodec
    {

        public static readonly ClientMessageType RequestType = ClientMessageType.ClientMembershipListener;
        public const int ResponseType = 104;
        public const bool Retryable = false;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly ClientMessageType TYPE = RequestType;

            public static int CalculateDataSize()
            {
                int dataSize = ClientMessage.HeaderSize;
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest()
        {
            int requiredDataSize = RequestParameters.CalculateDataSize();
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ RESPONSE *************************//


        public class ResponseParameters
        {
            public string response;
        }

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            ResponseParameters parameters = new ResponseParameters();
            string response = null;
            response = clientMessage.GetStringUtf8();
            parameters.response = response;
            return parameters;
        }


        //************************ EVENTS *************************//

        public static ClientMessage EncodeMemberEvent(Core.IMember member, int eventType)
        {
            int dataSize = ClientMessage.HeaderSize;
                dataSize += MemberCodec.CalculateDataSize(member);
                dataSize += Bits.IntSizeInBytes;

            ClientMessage clientMessage = ClientMessage.CreateForEncode(dataSize);
            clientMessage.SetMessageType(EventMessageConst.EventMember);
            clientMessage.AddFlag(ClientMessage.ListenerEventFlag);

            MemberCodec.Encode(member, clientMessage);
            clientMessage.Set(eventType);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }
        public static ClientMessage EncodeMemberSetEvent(ISet<Core.IMember> members)
        {
            int dataSize = ClientMessage.HeaderSize;
                dataSize += Bits.IntSizeInBytes;
                foreach (var members_item in members )
                {
                dataSize += MemberCodec.CalculateDataSize(members_item);
                }

            ClientMessage clientMessage = ClientMessage.CreateForEncode(dataSize);
            clientMessage.SetMessageType(EventMessageConst.EventMemberSet);
            clientMessage.AddFlag(ClientMessage.ListenerEventFlag);

            clientMessage.Set(members.Count);
            foreach (var members_item in members) {
            MemberCodec.Encode(members_item, clientMessage);
            }
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }
        public static ClientMessage EncodeMemberAttributeChangeEvent(MemberAttributeChange memberAttributeChange)
        {
            int dataSize = ClientMessage.HeaderSize;
                dataSize += MemberAttributeChangeCodec.CalculateDataSize(memberAttributeChange);

            ClientMessage clientMessage = ClientMessage.CreateForEncode(dataSize);
            clientMessage.SetMessageType(EventMessageConst.EventMemberAttributeChange);
            clientMessage.AddFlag(ClientMessage.ListenerEventFlag);

            MemberAttributeChangeCodec.Encode(memberAttributeChange, clientMessage);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        public abstract class AbstractEventHandler
        {
            public static void Handle(IClientMessage clientMessage, HandleMember handleMember, HandleMemberSet handleMemberSet, HandleMemberAttributeChange handleMemberAttributeChange)
            {
                int messageType = clientMessage.GetMessageType();
                if (messageType == EventMessageConst.EventMember) {
            Core.IMember member = null;
            member = MemberCodec.Decode(clientMessage);
            int eventType ;
            eventType = clientMessage.GetInt();
                    handleMember(member, eventType);
                    return;
                }
                if (messageType == EventMessageConst.EventMemberSet) {
            ISet<Core.IMember> members = null;
            int members_size = clientMessage.GetInt();
            members = new HashSet<Core.IMember>();
            for (int members_index = 0; members_index<members_size; members_index++) {
                Core.IMember members_item;
            members_item = MemberCodec.Decode(clientMessage);
                members.Add(members_item);
            }
                    handleMemberSet(members);
                    return;
                }
                if (messageType == EventMessageConst.EventMemberAttributeChange) {
            MemberAttributeChange memberAttributeChange = null;
            memberAttributeChange = MemberAttributeChangeCodec.Decode(clientMessage);
                    handleMemberAttributeChange(memberAttributeChange);
                    return;
                }
                Hazelcast.Logging.Logger.GetLogger(typeof(AbstractEventHandler)).Warning("Unknown message type received on event handler :" + clientMessage.GetMessageType());
            }

            public delegate void HandleMember(Core.IMember member, int eventType);
            public delegate void HandleMemberSet(ISet<Core.IMember> members);
            public delegate void HandleMemberAttributeChange(MemberAttributeChange memberAttributeChange);
       }

    }
}
