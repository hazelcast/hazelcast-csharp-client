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
            service.AddConstantSerializer<Guid>(new GuidSerializer());
            service.AddConstantSerializer<KeyValuePair<object, object>>(new KeyValuePairSerializer());
            service.AddConstantSerializer<bool[]>(new BooleanArraySerializer());
            service.AddConstantSerializer<byte[]>(new ByteArraySerializer());
            service.AddConstantSerializer<char[]>(new CharArraySerializer());
            service.AddConstantSerializer<short[]>(new ShortArraySerializer());
            service.AddConstantSerializer<int[]>(new IntegerArraySerializer());
            service.AddConstantSerializer<long[]>(new LongArraySerializer());
            service.AddConstantSerializer<float[]>(new FloatArraySerializer());
            service.AddConstantSerializer<double[]>(new DoubleArraySerializer());
            service.AddConstantSerializer<string[]>(new StringArraySerializer());
        }
    }
}
