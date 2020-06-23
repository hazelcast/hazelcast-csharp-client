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
