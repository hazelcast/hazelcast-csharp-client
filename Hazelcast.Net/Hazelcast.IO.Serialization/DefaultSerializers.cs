using System;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    internal sealed class DefaultSerializers
    {
        public sealed class DateSerializer : SingletonSerializer<DateTime>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.DefaultTypeDate;
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override DateTime Read(IObjectDataInput input)
            {
                return new DateTime().CreateDateTime(input.ReadLong());
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override void Write(IObjectDataOutput output, DateTime obj)
            {
                output.WriteLong(obj.Subtract(obj.EpoxDateTime()).Ticks);
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