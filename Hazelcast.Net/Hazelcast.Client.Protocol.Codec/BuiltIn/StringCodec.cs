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

using System.Text;
using static Hazelcast.Client.Protocol.ClientMessage;
using static Hazelcast.IO.Bits;

namespace Hazelcast.Client.Protocol.Codec.BuiltIn
{
    internal static class StringCodec
    {
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

        public static void Encode(ClientMessage clientMessage, string value)
        {
            clientMessage.Add(new ClientMessage.Frame(Utf8.GetBytes(value)));
        }

        public static String Decode(ref ClientMessage.FrameIterator iterator)
        {
            return Decode(iterator.Next());
        }

        public static String Decode(ClientMessage.Frame frame)
        {
            return Utf8.GetString(frame.Content);
        }
    }
}
