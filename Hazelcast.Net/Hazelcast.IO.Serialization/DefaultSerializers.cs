using System;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    internal sealed class DefaultSerializers
    {
        public sealed class DateSerializer : SingletonSerializer<DateTime>
        {
            private readonly static DateTime Epoch = new DateTime(1970,1,1, 0,0,0, DateTimeKind.Utc);
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

        internal abstract class SingletonSerializer<T> : IStreamSerializer<T>
        {
            public virtual void Destroy()
            {
            }

            public abstract int GetTypeId();

            public abstract T Read(IObjectDataInput arg1);

            public abstract void Write(IObjectDataOutput arg1, T arg2);
        }
    }
}