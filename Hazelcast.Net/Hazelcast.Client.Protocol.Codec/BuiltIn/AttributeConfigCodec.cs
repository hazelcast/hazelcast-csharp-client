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

//package com.hazelcast.client.impl.protocol.codec.builtin;

//import com.hazelcast.client.impl.protocol.ClientMessage;
//import com.hazelcast.config.AttributeConfig;

//import java.util.ListIterator;

//import static com.hazelcast.client.impl.protocol.ClientMessage.BeginFrame;
//import static com.hazelcast.client.impl.protocol.ClientMessage.EndFrame;
//import static com.hazelcast.client.impl.protocol.codec.builtin.CodecUtil.fastForwardToEndFrame;

namespace Hazelcast.Client.Protocol.Codec.BuiltIn
{
    internal static class AttributeConfigCodec
    {
        public static void Encode(ClientMessage clientMessage, AttributeConfig config)
        {
            clientMessage.add(BeginFrame);

            StringCodec.Encode(clientMessage, config.getName());
            StringCodec.Encode(clientMessage, config.getExtractorClassName());

            clientMessage.add(EndFrame);
        }

        public static AttributeConfig Decode(ListIterator<ClientMessage.Frame> iterator)
        {
            // begin frame
            iterator.next();

            String name = StringCodec.Decode(iterator);
            String extractorClassName = StringCodec.Decode(iterator);

            fastForwardToEndFrame(iterator);

            return new AttributeConfig(name, extractorClassName);
        }
    }
}
