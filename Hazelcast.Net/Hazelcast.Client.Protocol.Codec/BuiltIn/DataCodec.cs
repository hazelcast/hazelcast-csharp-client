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
//import com.hazelcast.internal.serialization.impl.HeapData;
//import com.hazelcast.nio.serialization.Data;

//import java.util.ListIterator;

using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Protocol.Codec.BuiltIn
{
    internal static class DataCodec
    {
        public static void Encode(ClientMessage clientMessage, IData data)
        {
            clientMessage.Add(new ClientMessage.Frame(data.ToByteArray()));
        }

        public static IData Decode(ref ClientMessage.Frame frame)
        {
            return new HeapData(frame.Content);
        }

        public static IData Decode(ref ClientMessage.FrameIterator iterator)
        {
            return Decode(ref iterator.Next());
        }
    }
}