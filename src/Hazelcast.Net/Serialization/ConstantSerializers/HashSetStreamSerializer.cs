// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Serialization.ConstantSerializers
{
    internal class HashSetStreamSerializer : CollectionStreamSerializerBase<HashSet<object>>
    {
        public override int TypeId => SerializationConstants.JavaDefaultTypeHashSet;

        public override HashSet<object> Read(IObjectDataInput input)
        {
            var size = input.ReadInt();
            var set = new HashSet<object>();
            return DeserializeEntries(input, size, set);
        }
    }
}
