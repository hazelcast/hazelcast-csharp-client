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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using Hazelcast.Core;
using Bits = Hazelcast.Messaging.Portability;

namespace Hazelcast.Serialization
{
    class DefaultSerializers
    {
        internal class JavaClassSerializer : ConstantSerializers.SingletonSerializer<JavaClass>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.JavaDefaultTypeClass;
            }

            public override JavaClass Read(IObjectDataInput input)
            {
                return new JavaClass(input.ReadUtf());
            }

            public override void Write(IObjectDataOutput output, JavaClass obj)
            {
                output.WriteUtf(obj.Name);
            }
        }

        internal sealed class HazelcastJsonValueSerializer : ConstantSerializers.SingletonSerializer<HazelcastJsonValue>
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

        internal class DateSerializer : ConstantSerializers.SingletonSerializer<DateTime>
        {
            private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            public override int GetTypeId()
            {
                return SerializationConstants.JavaDefaultTypeDate;
            }

            public override DateTime Read(IObjectDataInput input)
            {
                return FromEpochTime(input.ReadLong());
            }

            public override void Write(IObjectDataOutput output, DateTime obj)
            {
                output.WriteLong(ToEpochDateTime(obj));
            }

            private static DateTime FromEpochTime(long sinceEpoxMillis)
            {
                return Epoch.AddMilliseconds(sinceEpoxMillis);
            }

            private static long ToEpochDateTime(DateTime dateTime)
            {
                return (long) dateTime.Subtract(Epoch).TotalMilliseconds;
            }
        }

        internal class BigIntegerSerializer : ConstantSerializers.SingletonSerializer<BigInteger>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.JavaDefaultTypeBigInteger;
            }

            public override BigInteger Read(IObjectDataInput input)
            {
                var bytes = input.ReadByteArray();
                Array.Reverse(bytes);
                return new BigInteger(bytes);
            }

            public override void Write(IObjectDataOutput output, BigInteger obj)
            {
                var bytes = obj.ToByteArray();
                Array.Reverse(bytes);
                output.WriteByteArray(bytes);
            }
        }

        // TODO: BigDecimal

        internal class ListSerializer<T> : ConstantSerializers.SingletonSerializer<List<T>>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.JavaDefaultTypeArrayList;
            }

            public override List<T> Read(IObjectDataInput input)
            {
                var size = input.ReadInt();
                if (size <= Bits.NullArray) return null;

                var list = new List<T>(size);
                for (var i = 0; i < size; i++)
                {
                    list.Add(input.ReadObject<T>());
                }
                return list;
            }

            public override void Write(IObjectDataOutput output, List<T> obj)
            {
                var size = obj == null ? Bits.NullArray : obj.Count;
                output.WriteInt(size);
                foreach (var o in obj)
                {
                    output.WriteObject(o);
                }
            }
        }

        internal class LinkedListSerializer<T> : ConstantSerializers.SingletonSerializer<LinkedList<T>>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.JavaDefaultTypeLinkedList;
            }

            public override LinkedList<T> Read(IObjectDataInput input)
            {
                var size = input.ReadInt();
                if (size <= Bits.NullArray) return null;

                var list = new LinkedList<T>();
                for (var i = 0; i < size; i++)
                {
                    list.AddLast(input.ReadObject<T>());
                }
                return list;
            }

            public override void Write(IObjectDataOutput output, LinkedList<T> obj)
            {
                var size = obj == null ? Bits.NullArray : obj.Count;
                output.WriteInt(size);
                foreach (var o in obj)
                {
                    output.WriteObject(o);
                }
            }
        }

        /// <summary>
        /// Serialize using default .NET serialization
        /// </summary>
        internal class SerializableSerializer : ConstantSerializers.SingletonSerializer<object>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.CsharpClrSerializationType;
            }

            public override object Read(IObjectDataInput input)
            {
                var formatter = new BinaryFormatter();
                var stream = new MemoryStream(input.ReadByteArray());
                return formatter.Deserialize(stream);
            }

            public override void Write(IObjectDataOutput output, object obj)
            {
                var formatter = new BinaryFormatter();
                var stream = new MemoryStream();
                formatter.Serialize(stream, obj);
                output.WriteByteArray(stream.GetBuffer());
            }
        }

        internal class ArrayStreamSerializer : ConstantSerializers.SingletonSerializer<object[]>
        {
            public override int GetTypeId() => SerializationConstants.JavaDefaultTypeArray;

            public override object[] Read(IObjectDataInput input)
            {
                var length = input.ReadInt();
                var objects = new object[length];
                for (var i = 0; i < length; i++)
                {
                    objects[i] = input.ReadObject<object>();
                }
                return objects;
            }

            public override void Write(IObjectDataOutput output, object[] obj)
            {
                output.WriteInt(obj.Length);
                foreach (var t in obj)
                {
                    output.WriteObject(t);
                }
            }
        }

        internal class HashMapStreamSerializer : AbstractDictStreamSerializer<Dictionary<object, object>>
        {
            public override int GetTypeId() => SerializationConstants.JavaDefaultTypeHashMap;

            public override Dictionary<object, object> Read(IObjectDataInput input)
            {
                var size = input.ReadInt();
                var dict = new Dictionary<object, object>(size);
                return DeserializeEntries(input, size, dict);
            }
        }

        internal class ConcurrentHashMapStreamSerializer : AbstractDictStreamSerializer<ConcurrentDictionary<object, object>>
        {
            private static readonly int DefaultConcurrencyLevel = Environment.ProcessorCount;

            public override int GetTypeId() => SerializationConstants.JavaDefaultTypeConcurrentHashMap;

            public override ConcurrentDictionary<object, object> Read(IObjectDataInput input)
            {
                var size = input.ReadInt();
                var dict = new ConcurrentDictionary<object, object>(DefaultConcurrencyLevel, size);
                return DeserializeEntries(input, size, dict);
            }
        }

        internal class HashSetStreamSerializer : AbstractCollectionStreamSerializer<HashSet<object>>
        {
            public override int GetTypeId() => SerializationConstants.JavaDefaultTypeHashSet;

            public override HashSet<object> Read(IObjectDataInput input)
            {
                var size = input.ReadInt();
                var set = new HashSet<object>();
                return DeserializeEntries(input, size, set);
            }
        }

        internal abstract class AbstractCollectionStreamSerializer<CollectionType> : IStreamSerializer<CollectionType>
            where CollectionType : ISet<object>
        {
            public void Destroy()
            {
            }

            public abstract int GetTypeId();

            public abstract CollectionType Read(IObjectDataInput input);

            public void Write(IObjectDataOutput output, CollectionType obj)
            {
                var size = obj.Count;
                output.WriteInt(size);
                if (size > 0)
                {
                    foreach (var o in obj)
                    {
                        output.WriteObject(o);
                    }
                }
            }

            protected CollectionType DeserializeEntries(IObjectDataInput input, int size, CollectionType collection)
            {
                for (var i = 0; i < size; i++)
                {
                    collection.Add(input.ReadObject<object>());
                }
                return collection;
            }
        }

        internal abstract class AbstractDictStreamSerializer<DType> : IStreamSerializer<DType>
            where DType : IDictionary<object, object>
        {
            public abstract int GetTypeId();

            public void Destroy()
            {
            }

            public abstract DType Read(IObjectDataInput input);

            public void Write(IObjectDataOutput output, DType obj)
            {
                var size = obj.Count;
                output.WriteInt(size);
                if (size > 0)
                {
                    foreach (var kvp in obj)
                    {
                        output.WriteObject(kvp.Key);
                        output.WriteObject(kvp.Value);
                    }
                }
            }

            protected DType DeserializeEntries(IObjectDataInput input, int size, DType result)
            {
                for (int i = 0; i < size; i++)
                {
                    result.Add(input.ReadObject<object>(), input.ReadObject<object>());
                }
                return result;
            }
        }
    }
}