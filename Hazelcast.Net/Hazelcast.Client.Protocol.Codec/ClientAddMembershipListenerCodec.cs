// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Codec.BuiltIn;
using Hazelcast.Client.Protocol.Codec.Custom;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using static Hazelcast.Client.Protocol.Codec.BuiltIn.FixedSizeTypesCodec;
using static Hazelcast.Client.Protocol.ClientMessage;
using static Hazelcast.IO.Bits;
using Hazelcast.Logging;

namespace Hazelcast.Client.Protocol.Codec
{
    // This file is auto-generated by the Hazelcast Client Protocol Code Generator.
    // To change this file, edit the templates or the protocol
    // definitions on the https://github.com/hazelcast/hazelcast-client-protocol
    // and regenerate it.

    /// <summary>
    /// TODO DOC
    ///</summary>
    internal static class ClientAddMembershipListenerCodec 
    {
        //hex: 0x000300
        public const int RequestMessageType = 768;
        //hex: 0x000301
        public const int ResponseMessageType = 769;
        private const int RequestLocalOnlyFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BoolSizeInBytes;
        private const int ResponseResponseFieldOffset = ResponseBackupAcksFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + GuidSizeInBytes;
        private const int EventMemberEventTypeFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int EventMemberInitialFrameSize = EventMemberEventTypeFieldOffset + IntSizeInBytes;
        // hex: 0x000302
        private const int EventMemberMessageType = 770;
        private const int EventMemberListInitialFrameSize = PartitionIdFieldOffset + IntSizeInBytes;
        // hex: 0x000303
        private const int EventMemberListMessageType = 771;
        private const int EventMemberAttributeChangeOperationTypeFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int EventMemberAttributeChangeInitialFrameSize = EventMemberAttributeChangeOperationTypeFieldOffset + IntSizeInBytes;
        // hex: 0x000304
        private const int EventMemberAttributeChangeMessageType = 772;

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
    
        public static ClientMessage EncodeMemberEvent(Core.Member member, int eventType) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[EventMemberInitialFrameSize], UnfragmentedMessage);
            initialFrame.Flags |= IsEventFlag;
            EncodeInt(initialFrame.Content, TypeFieldOffset, EventMemberMessageType);
            EncodeInt(initialFrame.Content, EventMemberEventTypeFieldOffset, eventType);
            clientMessage.Add(initialFrame);
            MemberCodec.Encode(clientMessage, member);
            return clientMessage;
        }
    
        public static ClientMessage EncodeMemberListEvent(IEnumerable<Core.Member> members) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[EventMemberListInitialFrameSize], UnfragmentedMessage);
            initialFrame.Flags |= IsEventFlag;
            EncodeInt(initialFrame.Content, TypeFieldOffset, EventMemberListMessageType);
            clientMessage.Add(initialFrame);
            ListMultiFrameCodec.Encode(clientMessage, members, MemberCodec.Encode);
            return clientMessage;
        }
    
        public static ClientMessage EncodeMemberAttributeChangeEvent(Core.Member member, IEnumerable<Core.Member> members, string key, int operationType, string value) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[EventMemberAttributeChangeInitialFrameSize], UnfragmentedMessage);
            initialFrame.Flags |= IsEventFlag;
            EncodeInt(initialFrame.Content, TypeFieldOffset, EventMemberAttributeChangeMessageType);
            EncodeInt(initialFrame.Content, EventMemberAttributeChangeOperationTypeFieldOffset, operationType);
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
                    int eventType =  DecodeInt(initialFrame.Content, EventMemberEventTypeFieldOffset);
                    Core.Member member = MemberCodec.Decode(ref iterator);
                    HandleMemberEvent(member, eventType);
                    return;
                }
                if (messageType == EventMemberListMessageType) {
                    //empty initial frame
                    iterator.Next();
                    IList<Core.Member> members = ListMultiFrameCodec.Decode(ref iterator, MemberCodec.Decode);
                    HandleMemberListEvent(members);
                    return;
                }
                if (messageType == EventMemberAttributeChangeMessageType) {
                    var initialFrame = iterator.Next();
                    int operationType =  DecodeInt(initialFrame.Content, EventMemberAttributeChangeOperationTypeFieldOffset);
                    Core.Member member = MemberCodec.Decode(ref iterator);
                    IList<Core.Member> members = ListMultiFrameCodec.Decode(ref iterator, MemberCodec.Decode);
                    string key = StringCodec.Decode(ref iterator);
                    string value = CodecUtil.DecodeNullable(ref iterator, StringCodec.Decode);
                    HandleMemberAttributeChangeEvent(member, members, key, operationType, value);
                    return;
                }
                Logger.GetLogger(GetType()).Finest("Unknown message type received on event handler :" + messageType);
            }

            public abstract void HandleMemberEvent(Core.Member member, int eventType);

            public abstract void HandleMemberListEvent(IEnumerable<Core.Member> members);

            public abstract void HandleMemberAttributeChangeEvent(Core.Member member, IEnumerable<Core.Member> members, string key, int operationType, string value);
        }
    }
}