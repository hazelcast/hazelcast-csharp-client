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

using System.Collections.Generic;

namespace Hazelcast.Serialization.ConstantSerializers
{
    internal class KeyValuePairSerializer : SingletonSerializerBase<KeyValuePair<object, object>>
    {
        public override int GetTypeId() => SerializationConstants.ConstantTypeSimpleEntry;

        public override KeyValuePair<object, object> Read(IObjectDataInput input)
        {
            return new KeyValuePair<object, object>(input.ReadObject<object>(),input.ReadObject<object>());
        }

        public override void Write(IObjectDataOutput output, KeyValuePair<object, object> obj)
        {
            output.WriteObject(obj.Key);
            output.WriteObject(obj.Value);
        }
    }
}
