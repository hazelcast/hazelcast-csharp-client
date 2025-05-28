// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Messaging;
using Hazelcast.Models;
using Hazelcast.Serialization;

namespace Hazelcast.Protocol.BuiltInCodecs
{
    internal static class DataCodec
    {
        public static void Encode(ClientMessage clientMessage, IData data)
        {
            clientMessage.Append(new Frame(data.ToByteArray()));

            if (data is ICanHaveSchemas { HasSchemas: true } canHaveSchemas)
            {
                var messageSchemas = clientMessage.SchemaIds;
                foreach (var id in canHaveSchemas.SchemaIds)
                    messageSchemas.Add(id);
            }

        }

        public static void EncodeNullable(ClientMessage clientMessage, IData data)
        {
            if (data == null) clientMessage.Append(Frame.CreateNull());
            else Encode(clientMessage, data);
        }

        public static IData Decode(Frame frame)
        {
            return new HeapData(frame.Bytes);
        }

        public static IData Decode(IEnumerator<Frame> iterator)
        {
            return Decode(iterator.Take());
        }

        public static IData DecodeNullable(IEnumerator<Frame> iterator)
        {
            return iterator.SkipNull() ? null : Decode(iterator);
        }
    }
}
