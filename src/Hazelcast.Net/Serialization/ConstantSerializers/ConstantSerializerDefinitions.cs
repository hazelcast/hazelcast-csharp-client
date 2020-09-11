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

using System;
using System.Collections.Generic;

namespace Hazelcast.Serialization.ConstantSerializers
{
    internal class ConstantSerializerDefinitions : ISerializerDefinitions
    {
        public void AddSerializers(SerializationService service)
        {
            service.AddConstantSerializer<byte>(new ByteSerializer());
            service.AddConstantSerializer<bool>(new BooleanSerializer());
            service.AddConstantSerializer<char>(new CharSerializer());
            service.AddConstantSerializer<short>(new ShortSerializer());
            service.AddConstantSerializer<int>(new IntegerSerializer());
            service.AddConstantSerializer<long>(new LongSerializer());
            service.AddConstantSerializer<float>(new FloatSerializer());
            service.AddConstantSerializer<double>(new DoubleSerializer());
            service.AddConstantSerializer<string>(new StringSerializer());

            service.AddConstantSerializer<byte[]>(new ByteArraySerializer());
            service.AddConstantSerializer<bool[]>(new BooleanArraySerializer());
            service.AddConstantSerializer<char[]>(new CharArraySerializer());
            service.AddConstantSerializer<short[]>(new ShortArraySerializer());
            service.AddConstantSerializer<int[]>(new IntegerArraySerializer());
            service.AddConstantSerializer<long[]>(new LongArraySerializer());
            service.AddConstantSerializer<float[]>(new FloatArraySerializer());
            service.AddConstantSerializer<double[]>(new DoubleArraySerializer());
            service.AddConstantSerializer<string[]>(new StringArraySerializer());

            service.AddConstantSerializer<Guid>(new GuidSerializer());
            service.AddConstantSerializer<KeyValuePair<object, object>>(new KeyValuePairSerializer());
        }
    }
}
