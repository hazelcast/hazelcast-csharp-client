using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.Client.Request.Cluster;
using Hazelcast.Core;
using Hazelcast.IO;
using Hazelcast.Logging;

namespace Hazelcast.Client.Protocol.Codec
{
	internal sealed class ClientMembershipListenerCodec
	{
		public static readonly ClientMessageType RequestType = ClientMessageType.ClientMembershiplistener;

		public const int ResponseType = 104;

		public const bool Retryable = false;

		public class RequestParameters
		{
			public static readonly ClientMessageType Type = RequestType;

			//************************ REQUEST *************************//
			public static int CalculateDataSize()
			{
				int dataSize = ClientMessage.HeaderSize;
				return dataSize;
			}
		}

		public static ClientMessage EncodeRequest()
		{
			int requiredDataSize = ClientMembershipListenerCodec.RequestParameters.CalculateDataSize();
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(RequestType.Id());
			clientMessage.SetRetryable(Retryable);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ClientMembershipListenerCodec.RequestParameters DecodeRequest(ClientMessage clientMessage)
		{
			ClientMembershipListenerCodec.RequestParameters parameters = new ClientMembershipListenerCodec.RequestParameters();
			return parameters;
		}

		public class ResponseParameters
		{
			public string response;

			//************************ RESPONSE *************************//
			public static int CalculateDataSize(string response)
			{
				int dataSize = ClientMessage.HeaderSize;
				dataSize += ParameterUtil.CalculateStringDataSize(response);
				return dataSize;
			}
		}

		public static ClientMessage EncodeResponse(string response)
		{
			int requiredDataSize = ClientMembershipListenerCodec.ResponseParameters.CalculateDataSize(response);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
			clientMessage.SetMessageType(ResponseType);
			clientMessage.Set(response);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ClientMembershipListenerCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
		{
			ClientMembershipListenerCodec.ResponseParameters parameters = new ClientMembershipListenerCodec.ResponseParameters();
			string response;
			response = null;
			response = clientMessage.GetStringUtf8();
			parameters.response = response;
			return parameters;
		}

		//************************ EVENTS *************************//
		public static ClientMessage EncodeMemberEvent(Member member, int eventType)
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

		public static ClientMessage EncodeMemberListEvent(ICollection<Member> members)
		{
			int dataSize = ClientMessage.HeaderSize;
			dataSize += Bits.IntSizeInBytes;
			foreach (Member members_item in members)
			{
				dataSize += MemberCodec.CalculateDataSize(members_item);
			}
			ClientMessage clientMessage = ClientMessage.CreateForEncode(dataSize);
			clientMessage.SetMessageType(EventMessageConst.EventMemberlist);
			clientMessage.AddFlag(ClientMessage.ListenerEventFlag);
			clientMessage.Set(members.Count);
			foreach (Member members_item_1 in members)
			{
				MemberCodec.Encode(members_item_1, clientMessage);
			}
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public static ClientMessage EncodeMemberAttributeChangeEvent(MemberAttributeChange memberAttributeChange)
		{
			int dataSize = ClientMessage.HeaderSize;
			dataSize += MemberAttributeChangeCodec.CalculateDataSize(memberAttributeChange);
			ClientMessage clientMessage = ClientMessage.CreateForEncode(dataSize);
			clientMessage.SetMessageType(EventMessageConst.EventMemberattributechange);
			clientMessage.AddFlag(ClientMessage.ListenerEventFlag);
			MemberAttributeChangeCodec.Encode(memberAttributeChange, clientMessage);
			clientMessage.UpdateFrameLength();
			return clientMessage;
		}

		public abstract class AbstractEventHandler
		{
			public virtual void Handle(ClientMessage clientMessage)
			{
				int messageType = clientMessage.GetMessageType();
				if (messageType == EventMessageConst.EventMember)
				{
					Member member;
					member = null;
					member = MemberCodec.Decode(clientMessage);
					int eventType;
					eventType = clientMessage.GetInt();
					Handle(member, eventType);
					return;
				}
				if (messageType == EventMessageConst.EventMemberlist)
				{
					ICollection<Member> members;
					members = null;
					int members_size = clientMessage.GetInt();
					members = new List<Member>(members_size);
					for (int members_index = 0; members_index < members_size; members_index++)
					{
						Member members_item;
						members_item = MemberCodec.Decode(clientMessage);
						members.Add(members_item);
					}
					Handle(members);
					return;
				}
				if (messageType == EventMessageConst.EventMemberattributechange)
				{
					MemberAttributeChange memberAttributeChange;
					memberAttributeChange = null;
					memberAttributeChange = MemberAttributeChangeCodec.Decode(clientMessage);
					Handle(memberAttributeChange);
					return;
				}
				Logger.GetLogger(base.GetType()).Warning("Unknown message type received on event handler :" + clientMessage.GetMessageType());
			}

			public abstract void Handle(Member member, int eventType);

			public abstract void Handle(ICollection<Member> members);

			public abstract void Handle(MemberAttributeChange memberAttributeChange);
		}
	}
}
