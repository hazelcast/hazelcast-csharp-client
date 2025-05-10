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
using System;
using System.Collections.Generic;
using Hazelcast.Messaging;
using Hazelcast.Models;
using Hazelcast.Protocol.Models;

namespace Hazelcast.Protocol.BuiltInCodecs
{
    internal delegate T DecodeDelegate<out T>(IEnumerator<Frame> iterator);

    internal delegate T DecodeBytesDelegate<out T>(byte[] bytes, int position);

    internal static class CodecUtil
    {
        public static void EncodeNullable<T>(ClientMessage clientMessage, T value, Action<ClientMessage, T> encode)
        {
            if (value == null)
            {
                clientMessage.Append(Frame.CreateNull());
            }
            else
            {
                encode(clientMessage, value);
            }
        }

        public static T DecodeNullable<T>(IEnumerator<Frame> iterator, DecodeDelegate<T> decode) where T : class
        {
            return iterator.SkipNull() ? null : decode(iterator);
        }
        public static VectorValues ToVectorValues(List<VectorPairHolder> vectors)
        {
            if (vectors.Count == 1 && VectorPairHolder.SingleVectorName.Equals(vectors[0].Name, StringComparison.Ordinal))
            {
                if (vectors[0].Type == VectorPairHolder.DenseFloatVector)
                {
                    return VectorValues.Of(vectors[0].Vector);
                }
                throw new ArgumentException("Unsupported vector type: " + vectors[0].Type);
            }

            var indexNameToVector = new Dictionary<string, float[]>();
            foreach (var vector in vectors)
            {
                if (vector.Type == VectorPairHolder.DenseFloatVector)
                {
                    indexNameToVector[vector.Name] = vector.Vector;
                }
                else
                {
                    throw new ArgumentException("Unsupported vector type: " + vector.Type);
                }
            }
            return VectorValues.Of(indexNameToVector);
        }
    }
}
