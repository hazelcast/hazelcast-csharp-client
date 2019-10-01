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

using Hazelcast.CP;
using Hazelcast.IO;
using static Hazelcast.Client.Protocol.ClientMessage;
using static Hazelcast.Client.Protocol.BuiltIn.FixedSizeTypesCodec;

namespace Hazelcast.Client.Protocol.Codec.BuiltIn
{
    internal static class RaftGroupIdCodec
    {
        private const int SeedOffset = 0;
        private const int CommitIndexOffset = SeedOffset + Bits.LongSizeInBytes;
        private const int InitialFrameSize = CommitIndexOffset + Bits.LongSizeInBytes;

        public static void Encode(ClientMessage clientMessage, RaftGroupId groupId)
        {
            clientMessage.add(BeginFrame);

            var initialFrame = new ClientMessage.Frame(new byte[InitialFrameSize]);
            EncodeLong(initialFrame.Content, SeedOffset, groupId.Seed);
            EncodeLong(initialFrame.Content, CommitIndexOffset, groupId.id);
            clientMessage.add(initialFrame);

            StringCodec.Encode(clientMessage, groupId.Name);

            clientMessage.add(EndFrame);
        }

        public static RaftGroupId Decode(ref ClientMessage.FrameIterator iterator)
        {
            // begin frame
            iterator.Next();

            ref var initialFrame = iterator.Next();
            long seed = DecodeLong(initialFrame.content, SeedOffset);
            long commitIndex = DecodeLong(initialFrame.content, CommitIndexOffset);

            var name = StringCodec.Decode(iterator);

            CodecUtil.FastForwardToEndFrame(ref iterator);

            return new RaftGroupId(name, seed, commitIndex);
        }
    }
}
