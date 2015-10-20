/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Client.Protocol;
using Hazelcast.Client.Protocol.Util;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using System.Collections.Generic;

namespace Hazelcast.Client.Protocol.Codec
{
    internal sealed class ClientAddDistributedObjectListenerCodec
    {

        public static readonly ClientMessageType RequestType = ClientMessageType.ClientAddDistributedObjectListener;
        public const int ResponseType = 104;
        public const bool Retryable = true;

        //************************ REQUEST *************************//

        public class RequestParameters
        {
            public static readonly ClientMessageType TYPE = RequestType;
            public bool localOnly;

            public static int CalculateDataSize(bool localOnly)
            {
                int dataSize = ClientMessage.HeaderSize;
                dataSize += Bits.BooleanSizeInBytes;
                return dataSize;
            }
        }

        public static ClientMessage EncodeRequest(bool localOnly)
        {
            int requiredDataSize = RequestParameters.CalculateDataSize(localOnly);
            ClientMessage clientMessage = ClientMessage.CreateForEncode(requiredDataSize);
            clientMessage.SetMessageType((int)RequestType);
            clientMessage.SetRetryable(Retryable);
            clientMessage.Set(localOnly);
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
        public abstract class AbstractEventHandler
        {
            public static void Handle(IClientMessage clientMessage, HandleDistributedObject handleDistributedObject)
            {
                int messageType = clientMessage.GetMessageType();
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
                Hazelcast.Logging.Logger.GetLogger(typeof(AbstractEventHandler)).Warning("Unknown message type received on event handler :" + clientMessage.GetMessageType());
            }

            public delegate void HandleDistributedObject(string name, string serviceName, string eventType);
        }

    }
}
