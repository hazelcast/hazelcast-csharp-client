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

using System;
using System.Collections.Concurrent;

namespace Hazelcast.Serialization.ConstantSerializers
{
    internal class ConcurrentHashMapStreamSerializer : DictStreamSerializerBase<ConcurrentDictionary<object, object>>
    {
        private static readonly int DefaultConcurrencyLevel = Environment.ProcessorCount;

        public override int TypeId => SerializationConstants.JavaDefaultTypeConcurrentHashMap;

        public override ConcurrentDictionary<object, object> Read(IObjectDataInput input)
        {
            var size = input.ReadInt();
            var dict = new ConcurrentDictionary<object, object>(DefaultConcurrencyLevel, size);
            return DeserializeEntries(input, size, dict);
        }
    }
}
