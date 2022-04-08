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
using Hazelcast.Models;

namespace Hazelcast.Serialization.DefaultSerializers
{
    internal class DefaultSerializerDefinitions : ISerializerDefinitions
    {
        public void AddSerializers(SerializationService service)
        {
            //TODO: proper support for generic types
            service.RegisterConstantSerializer<JavaClass>(new JavaClassSerializer());
            service.RegisterConstantSerializer<DateTime>(new DateSerializer());
            service.RegisterConstantSerializer<BigInteger>(new BigIntegerSerializer());

            service.RegisterConstantSerializer<object[]>(new ArrayStreamSerializer());

            //TODO map server side collection types.
            service.RegisterConstantSerializer<List<object>>(new ListSerializer<object>());
            service.RegisterConstantSerializer<LinkedList<object>>(new LinkedListSerializer<object>());

            service.RegisterConstantSerializer<Dictionary<object, object>>(new HashMapStreamSerializer());
            service.RegisterConstantSerializer<ConcurrentDictionary<object, object>>(new ConcurrentHashMapStreamSerializer());

            service.RegisterConstantSerializer<HashSet<object>>(new HashSetStreamSerializer());

            service.RegisterConstantSerializer<HazelcastJsonValue>(new HazelcastJsonValueSerializer());

            service.RegisterConstantSerializer<HLocalDate>(new HLocalDateSerializer());
            service.RegisterConstantSerializer<HLocalTime>(new HLocalTimeSerializer());
            service.RegisterConstantSerializer<HLocalDateTime>(new HLocalDateTimeSerializer());
            service.RegisterConstantSerializer<HOffsetDateTime>(new HOffsetDateTimeSerializer());

            service.RegisterConstantSerializer<HBigDecimal>(new HBigDecimalSerializer());
        }
    }
}
