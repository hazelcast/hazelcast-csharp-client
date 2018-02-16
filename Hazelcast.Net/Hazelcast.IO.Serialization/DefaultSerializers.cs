// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Hazelcast.IO.Serialization
{
    class DefaultSerializers
    {
        public class JavaClassSerializer : ConstantSerializers.SingletonSerializer<JavaClass>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.DefaultTypeJavaClass;
            }

            public override JavaClass Read(IObjectDataInput input)
            {
                return new JavaClass(input.ReadUTF());
            }

            public override void Write(IObjectDataOutput output, JavaClass obj)
            {
                output.WriteUTF(obj.Name);
            }
        }

        public class DateSerializer : ConstantSerializers.SingletonSerializer<DateTime>
        {
            private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            public override int GetTypeId()
            {
                return SerializationConstants.DefaultTypeDate;
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
                return (long)dateTime.Subtract(Epoch).TotalMilliseconds;
            }
        }

        public class BigIntegerSerializer : ConstantSerializers.SingletonSerializer<BigInteger>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.DefaultTypeBigInteger;
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

        public class JavaEnumSerializer : ConstantSerializers.SingletonSerializer<JavaEnum>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.DefaultTypeJavaEnum;
            }

            public override JavaEnum Read(IObjectDataInput input)
            {
                return new JavaEnum(input.ReadUTF(), input.ReadUTF());
            }

            public override void Write(IObjectDataOutput output, JavaEnum obj)
            {
                output.WriteUTF(obj.Type);
                output.WriteUTF(obj.Value);
            }
        }

        public class ListSerializer<T> : ConstantSerializers.SingletonSerializer<List<T>>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.DefaultTypeArrayList;
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

        public class LinkedListSerializer<T> : ConstantSerializers.SingletonSerializer<LinkedList<T>>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.DefaultTypeLinkedList;
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
        public class SerializableSerializer : ConstantSerializers.SingletonSerializer<object>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.DefaultTypeSerializable;
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
    }
}
