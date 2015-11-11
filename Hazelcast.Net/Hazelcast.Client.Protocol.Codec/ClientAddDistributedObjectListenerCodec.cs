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

using Hazelcast.IO;
using Hazelcast.Logging;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class ClientAddDistributedObjectListenerCodec
    {
        public const int ResponseType = 104;
        public const bool Retryable = true;

        public static readonly ClientMessageType RequestType = ClientMessageType.ClientAddDistributedObjectListener;

        public static ResponseParameters DecodeResponse(IClientMessage clientMessage)
        {
            var parameters = new ResponseParameters();
            string response = null;
            response = clientMessage.GetStringUtf8();
            parameters.response = response;
            return parameters;
        }

        public static ClientMessage EncodeRequest(bool localOnly)
        {
            var requiredDataSize = RequestParameters.CalculateDataSize(localOnly);
            var clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int) RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(localOnly);
            clientMessage.UpdateFrameLength();
            return clientMessage;
        }

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly ClientMessageType TYPE = RequestType;
            public bool localOnly;

            public static int CalculateDataSize(bool localOnly)
            {
                var dataSize = ClientMessage.HeaderSize;
                dataSize += Bits.BooleanSizeInBytes;
                return dataSize;
            }
        }

        //************************ RESPONSE *************************//


        public class ResponseParameters
        {
            public string response;
        }


        //************************ EVENTS *************************//
        public abstract class AbstractEventHandler
        {
            public delegate void HandleDistributedObject(string name, string serviceName, string eventType);

            public static void Handle(IClientMessage clientMessage, HandleDistributedObject handleDistributedObject)
            {
                var messageType = clientMessage.GetMessageType();
                if (messageType == EventMessageConst.EventDistributedObject)
                {
                    string name = null;
                    name = clientMessage.GetStringUtf8();
                    string serviceName = null;
                    serviceName = clientMessage.GetStringUtf8();
                    string eventType = null;
                    eventType = clientMessage.GetStringUtf8();
                    handleDistributedObject(name, serviceName, eventType);
                    return;
                }
                Logger.GetLogger(typeof (AbstractEventHandler))
                    .Warning("Unknown message type received on event handler :" + clientMessage.GetMessageType());
            }
        }
    }
}