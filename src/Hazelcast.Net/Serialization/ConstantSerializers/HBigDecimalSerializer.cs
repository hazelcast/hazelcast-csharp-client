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
using System.Numerics;
using Hazelcast.Models;

namespace Hazelcast.Serialization.ConstantSerializers
{
    internal class HBigDecimalSerializer : SingletonSerializerBase<HBigDecimal>
    {
        public override int TypeId => SerializationConstants.JavaDefaultTypeBigDecimal;

        public override HBigDecimal Read(IObjectDataInput input)
        {
            var body = input.ReadByteArray();
            var scale = input.ReadInt();

#if NETSTANDARD2_0
            Array.Reverse(body);
            var unscaled = new BigInteger(body);
#else
            var unscaled = new BigInteger(body, isUnsigned: false, isBigEndian: true);
#endif

            return new HBigDecimal(unscaled, scale);
        }

        public override void Write(IObjectDataOutput output, HBigDecimal obj)
        {
            var body = obj.UnscaledValue.ToByteArray();
            Array.Reverse(body);

            output.WriteByteArray(body);
            output.WriteInt(obj.Scale);
        }
    }
}
