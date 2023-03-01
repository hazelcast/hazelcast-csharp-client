// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Serialization;

namespace Hazelcast.Protocol.BuiltInCodecs
{
    /// <summary>
    /// Codec for the list of data which allows optional items.
    /// </summary>
    internal static class ListCNDataCodec
    {
        public static void Encode(ClientMessage clientMessage, IEnumerable<IData> collection)
        {
            ListMultiFrameCodec.Encode(clientMessage, collection, DataCodec.EncodeNullable);
        }

        public static IList<IData> Decode(IEnumerator<Frame> iterator)
        {
            return ListMultiFrameCodec.Decode(iterator, DataCodec.DecodeNullable);
        }
    }
}
