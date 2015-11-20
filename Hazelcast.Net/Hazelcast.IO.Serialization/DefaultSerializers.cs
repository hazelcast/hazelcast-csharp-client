using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Hazelcast.IO.Serialization
{
    class DefaultSerializers
    {
        public sealed class DateSerializer : ConstantSerializers.SingletonSerializer<DateTime>
        {
            private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            public override int GetTypeId()
            {
                return SerializationConstants.DefaultTypeDate;
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override DateTime Read(IObjectDataInput input)
            {
                return FromEpochTime(input.ReadLong());
            }

            /// <exception cref="System.IO.IOException"></exception>
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
