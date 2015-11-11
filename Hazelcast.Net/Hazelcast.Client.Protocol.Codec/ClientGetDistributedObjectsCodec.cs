// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class ClientGetDistributedObjectsCodec
    {
        public const int ResponseType = 110;
        public const bool Retryable = false;

        public static readonly ClientMessageType RequestType = ClientMessageType.ClientGetDistributedObjects;

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            ISet<DistributedObjectInfo> infoCollection = null;
            var infoCollection_size = clientMessage.GetInt();
            infoCollection = new HashSet<DistributedObjectInfo>();
            for (var infoCollection_index = 0; infoCollection_index < infoCollection_size; infoCollection_index++)
            {
                DistributedObjectInfo infoCollection_item;
                infoCollection_item = DistributedObjectInfoCodec.Decode(clientMessage);
                infoCollection.Add(infoCollection_item);
            }
            parameters.infoCollection = infoCollection;
            return parameters;
        }

        public static ClientMessage EncodeRequest()
        {
            var requiredDataSize = RequestParameters.CalculateDataSize();
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly ClientMessageType TYPE = RequestType;

            public static int CalculateDataSize()
            {
                var dataSize = ClientMessage.HeaderSize;
                return dataSize;
            }
        }

        //************************ RESPONSE *************************//


        public class ResponseParameters
        {
            public ISet<DistributedObjectInfo> infoCollection;
        }
    }
}