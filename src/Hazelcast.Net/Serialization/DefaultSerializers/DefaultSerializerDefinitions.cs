// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.Numerics;
using Hazelcast.Core;

namespace Hazelcast.Serialization.DefaultSerializers
{
    internal class DefaultSerializerDefinitions : ISerializerDefinitions
    {
        public void AddSerializers(SerializationService service)
        {
            //TODO: proper support for generic types
            service.AddConstantSerializer<JavaClass>(new JavaClassSerializer());
            service.AddConstantSerializer<DateTime>(new DateSerializer());
            service.AddConstantSerializer<BigInteger>(new BigIntegerSerializer());

            service.AddConstantSerializer<object[]>(new ArrayStreamSerializer());

            //TODO map server side collection types.
            service.AddConstantSerializer<List<object>>(new ListSerializer<object>());
            service.AddConstantSerializer<LinkedList<object>>(new LinkedListSerializer<object>());

            service.AddConstantSerializer<Dictionary<object, object>>(new HashMapStreamSerializer());
            service.AddConstantSerializer<ConcurrentDictionary<object, object>>(new ConcurrentHashMapStreamSerializer());

            service.AddConstantSerializer<HashSet<object>>(new HashSetStreamSerializer());

            service.AddConstantSerializer<HazelcastJsonValue>(new HazelcastJsonValueSerializer());
        }
    }
}
