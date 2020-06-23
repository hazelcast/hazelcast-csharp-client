// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Core;

namespace Hazelcast.Serialization.DefaultSerializers
{
    internal sealed class HazelcastJsonValueSerializer : ConstantSerializers.SingletonSerializerBase<HazelcastJsonValue>
    {
        public override int GetTypeId()
        {
            return SerializationConstants.JavascriptJsonSerializationType;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override HazelcastJsonValue Read(IObjectDataInput input)
        {
            return new HazelcastJsonValue(input.ReadUtf());
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void Write(IObjectDataOutput output, HazelcastJsonValue obj)
        {
            output.WriteUtf(obj.ToString());
        }
    }
}