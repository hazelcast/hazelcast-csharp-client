/*
 * Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections;
using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec.BuiltIn;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using static Hazelcast.Client.Protocol.Codec.BuiltIn.FixedSizeTypesCodec;
using static Hazelcast.Client.Protocol.ClientMessage;
using static Hazelcast.IO.Bits;
using Hazelcast.Logging;

namespace Hazelcast.Client.Protocol.Codec
{
    /*
    * This file is auto-generated by the Hazelcast Client Protocol Code Generator.
    * To change this file, edit the templates or the protocol
    * definitions on the https://github.com/hazelcast/hazelcast-client-protocol
    * and regenerate it.
    */

    /// <summary>
    /// TODO DOC
    ///</summary>
    internal static class ClientAddMembershipListenerCodec 
    {
        public const int RequestMessageType = 0x000400;
        public const int ResponseMessageType = 0x000401;
        private const int RequestLocalOnlyFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BooleanSizeInBytes;
        private const int ResponseResponseFieldOffset = CorrelationIdFieldOffset + LongSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + UUIDSizeInBytes;
        private const int EventMembereventTypeFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int EventMemberInitialFrameSize = EventMembereventTypeFieldOffset + IntSizeInBytes;
        private const int EventMemberMessageType = 0x000402;
        private const int EventMemberListInitialFrameSize = PartitionIdFieldOffset + IntSizeInBytes;
        private const int EventMemberListMessageType = 0x000403;
        private const int EventMemberAttributeChangeoperationTypeFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int EventMemberAttributeChangeInitialFrameSize = EventMemberAttributeChangeoperationTypeFieldOffset + IntSizeInBytes;
        private const int EventMemberAttributeChangeMessageType = 0x000404;

        public class RequestParameters 
        {

            /// <summary>
            /// if true only master node sends events, otherwise all registered nodes send all membership
            /// changes.
            ///</summary>
            public bool LocalOnly;
        }

        public static ClientMessage EncodeRequest(bool localOnly) 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "Client.AddMembershipListener";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            EncodeBool(initialFrame.Content, RequestLocalOnlyFieldOffset, localOnly);
            clientMessage.Add(initialFrame);
            return clientMessage;
        }

        public static ClientAddMembershipListenerCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            var initialFrame = iterator.Next();
            request.LocalOnly =  DecodeBool(initialFrame.Content, RequestLocalOnlyFieldOffset);
            return request;
        }

        public class ResponseParameters 
        {

             /// <summary>
            /// Returns the registration id for the listener.
            ///</summary>
            public Guid Response;
        }

        public static ClientMessage EncodeResponse(Guid response) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, ResponseMessageType);
            clientMessage.Add(initialFrame);

            EncodeGuid(initialFrame.Content, ResponseResponseFieldOffset, response);
            return clientMessage;
        }

        public static ClientAddMembershipListenerCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Next();
            response.Response = DecodeGuid(initialFrame.Content, ResponseResponseFieldOffset);
            return response;
        }
    
        public static ClientMessage EncodeMemberEvent(com.hazelcast.cluster.Member member, int eventType) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[EventMemberInitialFrameSize], UnfragmentedMessage);
            initialFrame.Flags |= IsEventFlag;
            EncodeInt(initialFrame.Content, TypeFieldOffset, EventMemberMessageType);
            EncodeInt(initialFrame.Content, EventMembereventTypeFieldOffset, eventType);
            clientMessage.Add(initialFrame);
            MemberCodec.Encode(clientMessage, member);
            return clientMessage;
        }
    
        public static ClientMessage EncodeMemberListEvent(IEnumerable<com.hazelcast.cluster.Member> members) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[EventMemberListInitialFrameSize], UnfragmentedMessage);
            initialFrame.Flags |= IsEventFlag;
            EncodeInt(initialFrame.Content, TypeFieldOffset, EventMemberListMessageType);
            clientMessage.Add(initialFrame);
            ListMultiFrameCodec.Encode(clientMessage, members, MemberCodec.Encode);
            return clientMessage;
        }
    
        public static ClientMessage EncodeMemberAttributeChangeEvent(com.hazelcast.cluster.Member member, IEnumerable<com.hazelcast.cluster.Member> members, string key, int operationType, string value) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[EventMemberAttributeChangeInitialFrameSize], UnfragmentedMessage);
            initialFrame.Flags |= IsEventFlag;
            EncodeInt(initialFrame.Content, TypeFieldOffset, EventMemberAttributeChangeMessageType);
            EncodeInt(initialFrame.Content, EventMemberAttributeChangeoperationTypeFieldOffset, operationType);
            clientMessage.Add(initialFrame);
            MemberCodec.Encode(clientMessage, member);
            ListMultiFrameCodec.Encode(clientMessage, members, MemberCodec.Encode);
            StringCodec.Encode(clientMessage, key);
            CodecUtil.EncodeNullable(clientMessage, value, StringCodec.Encode);
            return clientMessage;
        }

        public abstract class AbstractEventHandler 
        {
            public void Handle(ClientMessage clientMessage) 
            {
                var messageType = clientMessage.MessageType;
                var iterator = clientMessage.GetIterator();
                if (messageType == EventMemberMessageType) {
                    var initialFrame = iterator.Next();
                    int eventType =  DecodeInt(initialFrame.Content, EventMembereventTypeFieldOffset);
                    com.hazelcast.client.impl.MemberImpl member = MemberCodec.Decode(ref iterator);
                    HandleMemberEvent(member, eventType);
                    return;
                }
                if (messageType == EventMemberListMessageType) {
                    //empty initial frame
                    iterator.Next();
                    IEnumerable<com.hazelcast.cluster.Member> members = ListMultiFrameCodec.Decode(ref iterator, MemberCodec.Decode);
                    HandleMemberListEvent(members);
                    return;
                }
                if (messageType == EventMemberAttributeChangeMessageType) {
                    var initialFrame = iterator.Next();
                    int operationType =  DecodeInt(initialFrame.Content, EventMemberAttributeChangeoperationTypeFieldOffset);
                    com.hazelcast.client.impl.MemberImpl member = MemberCodec.Decode(ref iterator);
                    IEnumerable<com.hazelcast.cluster.Member> members = ListMultiFrameCodec.Decode(ref iterator, MemberCodec.Decode);
                    string key = StringCodec.Decode(ref iterator);
                    string value = CodecUtil.DecodeNullable(ref iterator, StringCodec.Decode);
                    HandleMemberAttributeChangeEvent(member, members, key, operationType, value);
                    return;
                }
                Logger.GetLogger(GetType()).Finest("Unknown message type received on event handler :" + messageType);
            }

            public abstract void HandleMemberEvent(com.hazelcast.cluster.Member member, int eventType);

            public abstract void HandleMemberListEvent(IEnumerable<com.hazelcast.cluster.Member> members);

            public abstract void HandleMemberAttributeChangeEvent(com.hazelcast.cluster.Member member, IEnumerable<com.hazelcast.cluster.Member> members, string key, int operationType, string value);
        }
    }
}