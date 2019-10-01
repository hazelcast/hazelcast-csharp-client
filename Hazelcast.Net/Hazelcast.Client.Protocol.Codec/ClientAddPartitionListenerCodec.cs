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
    internal static class ClientAddPartitionListenerCodec 
    {
        public const int RequestMessageType = 0x001200;
        public const int ResponseMessageType = 0x001201;
        private const int RequestInitialFrameSize = PartitionIdFieldOffset + IntSizeInBytes;
        private const int ResponseInitialFrameSize = CorrelationIdFieldOffset + LongSizeInBytes;
        private const int EventPartitionspartitionStateVersionFieldOffset = PartitionIdFieldOffset + IntSizeInBytes;
        private const int EventPartitionsInitialFrameSize = EventPartitionspartitionStateVersionFieldOffset + IntSizeInBytes;
        private const int EventPartitionsMessageType = 0x001202;

        public class RequestParameters 
        {
        }

        public static ClientMessage EncodeRequest() 
        {
            var clientMessage = CreateForEncode();
            clientMessage.IsRetryable = false;
            clientMessage.AcquiresResource = false;
            clientMessage.OperationName = "Client.AddPartitionListener";
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, RequestMessageType);
            clientMessage.Add(initialFrame);
            return clientMessage;
        }

        public static ClientAddPartitionListenerCodec.RequestParameters DecodeRequest(ClientMessage clientMessage) 
        {
            var iterator = clientMessage.GetIterator();
            var request = new RequestParameters();
            //empty initial frame
            iterator.Next();
            return request;
        }

        public class ResponseParameters 
        {
        }

        public static ClientMessage EncodeResponse() 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], UnfragmentedMessage);
            EncodeInt(initialFrame.Content, TypeFieldOffset, ResponseMessageType);
            clientMessage.Add(initialFrame);

            return clientMessage;
        }

        public static ClientAddPartitionListenerCodec.ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            var iterator = clientMessage.GetIterator();
            var response = new ResponseParameters();
            //empty initial frame
            iterator.Next();
            return response;
        }
    
        public static ClientMessage EncodePartitionsEvent(IEnumerable<KeyValuePair<com.hazelcast.nio.Address, IEnumerable<int>>> partitions, int partitionStateVersion) 
        {
            var clientMessage = CreateForEncode();
            var initialFrame = new Frame(new byte[EventPartitionsInitialFrameSize], UnfragmentedMessage);
            initialFrame.Flags |= IsEventFlag;
            EncodeInt(initialFrame.Content, TypeFieldOffset, EventPartitionsMessageType);
            EncodeInt(initialFrame.Content, EventPartitionspartitionStateVersionFieldOffset, partitionStateVersion);
            clientMessage.Add(initialFrame);
            EntryListAddressListIntegerCodec.Encode(clientMessage, partitions);
            return clientMessage;
        }

        public abstract class AbstractEventHandler 
        {
            public void Handle(ClientMessage clientMessage) 
            {
                var messageType = clientMessage.MessageType;
                var iterator = clientMessage.GetIterator();
                if (messageType == EventPartitionsMessageType) {
                    var initialFrame = iterator.Next();
                    int partitionStateVersion =  DecodeInt(initialFrame.Content, EventPartitionspartitionStateVersionFieldOffset);
                    IEnumerable<KeyValuePair<com.hazelcast.nio.Address, IEnumerable<int>>> partitions = EntryListAddressListIntegerCodec.Decode(ref iterator);
                    HandlePartitionsEvent(partitions, partitionStateVersion);
                    return;
                }
                Logger.GetLogger(GetType()).Finest("Unknown message type received on event handler :" + messageType);
            }

            public abstract void HandlePartitionsEvent(IEnumerable<KeyValuePair<com.hazelcast.nio.Address, IEnumerable<int>>> partitions, int partitionStateVersion);
        }
    }
}