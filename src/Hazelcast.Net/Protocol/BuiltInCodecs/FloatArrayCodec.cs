// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.Messaging;
namespace Hazelcast.Protocol.BuiltInCodecs
{
    internal class FloatArrayCodec
    {
        public static void Encode(ClientMessage msg, float[] floatArray)
        {
            var floatArrayFrame = new Frame(new byte[floatArray.Length * BytesExtensions.SizeOfFloat]);
            for (var i = 0; i < floatArray.Length; i++)
            {
                floatArrayFrame.Bytes.WriteFloat(i * BytesExtensions.SizeOfFloat, floatArray[i], Endianness.LittleEndian);
            }
            msg.Append(floatArrayFrame);
        }
        public static float[] Decode(IEnumerator<Frame> iterator)
        {
            return Decode(iterator.Take());
        }

        public static float[] Decode(Frame frame)
        {
            var itemCount = frame.Bytes.Length / BytesExtensions.SizeOfFloat;
            var floatArray = new float[itemCount];
            for (var i = 0; i < itemCount; i++)
            {
                floatArray[i] = frame.Bytes.ReadFloat(i * BytesExtensions.SizeOfFloat, Endianness.LittleEndian);
            }

            return floatArray;
        }
    }
}
