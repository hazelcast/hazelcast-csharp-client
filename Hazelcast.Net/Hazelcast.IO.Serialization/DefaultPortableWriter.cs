using System.Collections.Generic;

namespace Hazelcast.IO.Serialization
{
    internal class DefaultPortableWriter : IPortableWriter
    {
        private readonly int begin;
        private readonly IClassDefinition cd;
        private readonly int offset;
        private readonly IBufferObjectDataOutput @out;
        private readonly PortableSerializer serializer;
        private readonly ISet<string> writtenFields;
        private bool raw;

        /// <exception cref="System.IO.IOException" />
        public DefaultPortableWriter(PortableSerializer serializer, IBufferObjectDataOutput @out, IClassDefinition cd)
        {
            this.serializer = serializer;
            this.@out = @out;
            this.cd = cd;
            writtenFields = new HashSet<string>(); //cd.GetFieldCount()
            begin = @out.Position();
            // room for final offset
            @out.WriteZeroBytes(4);
            @out.WriteInt(cd.GetFieldCount());
            offset = @out.Position();
            // one additional for raw data
            var fieldIndexesLength = (cd.GetFieldCount() + 1)*Bits.IntSizeInBytes;
            @out.WriteZeroBytes(fieldIndexesLength);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteInt(string fieldName, int value)
        {
            SetPosition(fieldName, FieldType.Int);
            @out.WriteInt(value);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteLong(string fieldName, long value)
        {
            SetPosition(fieldName, FieldType.Long);
            @out.WriteLong(value);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteUTF(string fieldName, string str)
        {
            SetPosition(fieldName, FieldType.Utf);
            @out.WriteUTF(str);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteBoolean(string fieldName, bool value)
        {
            SetPosition(fieldName, FieldType.Boolean);
            @out.WriteBoolean(value);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteByte(string fieldName, byte value)
        {
            SetPosition(fieldName, FieldType.Byte);
            @out.WriteByte(value);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteChar(string fieldName, int value)
        {
            SetPosition(fieldName, FieldType.Char);
            @out.WriteChar(value);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteDouble(string fieldName, double value)
        {
            SetPosition(fieldName, FieldType.Double);
            @out.WriteDouble(value);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteFloat(string fieldName, float value)
        {
            SetPosition(fieldName, FieldType.Float);
            @out.WriteFloat(value);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteShort(string fieldName, short value)
        {
            SetPosition(fieldName, FieldType.Short);
            @out.WriteShort(value);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WritePortable(string fieldName, IPortable portable)
        {
            var fd = SetPosition(fieldName, FieldType.Portable);
            var isNull = portable == null;
            @out.WriteBoolean(isNull);
            @out.WriteInt(fd.GetFactoryId());
            @out.WriteInt(fd.GetClassId());
            if (!isNull)
            {
                CheckPortableAttributes(fd, portable);
                serializer.WriteInternal(@out, portable);
            }
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteNullPortable(string fieldName, int factoryId, int classId)
        {
            SetPosition(fieldName, FieldType.Portable);
            @out.WriteBoolean(true);
            @out.WriteInt(factoryId);
            @out.WriteInt(classId);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteBooleanArray(string fieldName, bool[] values)
        {
            SetPosition(fieldName, FieldType.BooleanArray);
            @out.WriteBooleanArray(values);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteByteArray(string fieldName, byte[] values)
        {
            SetPosition(fieldName, FieldType.ByteArray);
            @out.WriteByteArray(values);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteCharArray(string fieldName, char[] values)
        {
            SetPosition(fieldName, FieldType.CharArray);
            @out.WriteCharArray(values);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteIntArray(string fieldName, int[] values)
        {
            SetPosition(fieldName, FieldType.IntArray);
            @out.WriteIntArray(values);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteLongArray(string fieldName, long[] values)
        {
            SetPosition(fieldName, FieldType.LongArray);
            @out.WriteLongArray(values);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteDoubleArray(string fieldName, double[] values)
        {
            SetPosition(fieldName, FieldType.DoubleArray);
            @out.WriteDoubleArray(values);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteFloatArray(string fieldName, float[] values)
        {
            SetPosition(fieldName, FieldType.FloatArray);
            @out.WriteFloatArray(values);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteShortArray(string fieldName, short[] values)
        {
            SetPosition(fieldName, FieldType.ShortArray);
            @out.WriteShortArray(values);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WriteUTFArray(string fieldName, string[] values)
        {
            SetPosition(fieldName, FieldType.UtfArray);
            @out.WriteUTFArray(values);
        }

        /// <exception cref="System.IO.IOException" />
        public virtual void WritePortableArray(string fieldName, IPortable[] portables)
        {
            var fd = SetPosition(fieldName, FieldType.PortableArray);
            var len = portables == null ? Bits.NullArray : portables.Length;
            @out.WriteInt(len);
            @out.WriteInt(fd.GetFactoryId());
            @out.WriteInt(fd.GetClassId());
            if (len > 0)
            {
                var offset = @out.Position();
                @out.WriteZeroBytes(len*4);
                for (var i = 0; i < portables.Length; i++)
                {
                    var portable = portables[i];
                    CheckPortableAttributes(fd, portable);
                    var position = @out.Position();
                    @out.WriteInt(offset + i*Bits.IntSizeInBytes, position);
                    serializer.WriteInternal(@out, portable);
                }
            }
        }

        /// <exception cref="System.IO.IOException" />
        public virtual IObjectDataOutput GetRawDataOutput()
        {
            if (!raw)
            {
                var pos = @out.Position();
                // last index
                var index = cd.GetFieldCount();
                @out.WriteInt(offset + index*Bits.IntSizeInBytes, pos);
            }
            raw = true;
            return @out;
        }

        public virtual int GetVersion()
        {
            return cd.GetVersion();
        }

        private void CheckPortableAttributes(IFieldDefinition fd, IPortable portable)
        {
            if (fd.GetFactoryId() != portable.GetFactoryId())
            {
                throw new HazelcastSerializationException(
                    "Wrong Portable type! Generic portable types are not supported! " + " Expected factory-id: " +
                    fd.GetFactoryId() + ", Actual factory-id: " + portable.GetFactoryId());
            }
            if (fd.GetClassId() != portable.GetClassId())
            {
                throw new HazelcastSerializationException(
                    "Wrong Portable type! Generic portable types are not supported! " + "Expected class-id: " +
                    fd.GetClassId() + ", Actual class-id: " + portable.GetClassId());
            }
        }

        /// <exception cref="System.IO.IOException" />
        private IFieldDefinition SetPosition(string fieldName, FieldType fieldType)
        {
            if (raw)
            {
                throw new HazelcastSerializationException(
                    "Cannot write Portable fields after getRawDataOutput() is called!");
            }
            var fd = cd.GetField(fieldName);
            if (fd == null)
            {
                throw new HazelcastSerializationException("Invalid field name: '" + fieldName +
                                                          "' for ClassDefinition {id: " + cd.GetClassId() +
                                                          ", version: " + cd.GetVersion() + "}");
            }
            if (writtenFields.Add(fieldName))
            {
                var pos = @out.Position();
                var index = fd.GetIndex();
                @out.WriteInt(offset + index*Bits.IntSizeInBytes, pos);
                @out.WriteShort(fieldName.Length);
                @out.WriteBytes(fieldName);
                @out.WriteByte((byte) fieldType);
            }
            else
            {
                throw new HazelcastSerializationException("Field '" + fieldName + "' has already been written!");
            }
            return fd;
        }

        /// <exception cref="System.IO.IOException" />
        internal virtual void End()
        {
            // write final offset
            var position = @out.Position();
            @out.WriteInt(begin, position);
        }
    }
}