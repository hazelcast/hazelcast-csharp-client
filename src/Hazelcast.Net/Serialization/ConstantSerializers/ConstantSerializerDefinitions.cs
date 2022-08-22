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
using System.Collections.Generic;

namespace Hazelcast.Serialization.ConstantSerializers
{
    internal class ConstantSerializerDefinitions : ISerializerDefinitions
    {
        public void AddSerializers(SerializationService service)
        {
            service.RegisterConstantSerializer<byte>(new ByteSerializer());
            service.RegisterConstantSerializer<bool>(new BooleanSerializer());
            service.RegisterConstantSerializer<char>(new CharSerializer());
            service.RegisterConstantSerializer<short>(new ShortSerializer());
            service.RegisterConstantSerializer<int>(new IntegerSerializer());
            service.RegisterConstantSerializer<long>(new LongSerializer());
            service.RegisterConstantSerializer<float>(new FloatSerializer());
            service.RegisterConstantSerializer<double>(new DoubleSerializer());
            service.RegisterConstantSerializer<string>(new StringSerializer());

            service.RegisterConstantSerializer<byte[]>(new ByteArraySerializer());
            service.RegisterConstantSerializer<bool[]>(new BooleanArraySerializer());
            service.RegisterConstantSerializer<char[]>(new CharArraySerializer());
            service.RegisterConstantSerializer<short[]>(new ShortArraySerializer());
            service.RegisterConstantSerializer<int[]>(new IntegerArraySerializer());
            service.RegisterConstantSerializer<long[]>(new LongArraySerializer());
            service.RegisterConstantSerializer<float[]>(new FloatArraySerializer());
            service.RegisterConstantSerializer<double[]>(new DoubleArraySerializer());
            service.RegisterConstantSerializer<string[]>(new StringArraySerializer());

            service.RegisterConstantSerializer<Guid>(new GuidSerializer());
            service.RegisterConstantSerializer<KeyValuePair<object, object>>(new KeyValuePairSerializer());
        }
    }
}
