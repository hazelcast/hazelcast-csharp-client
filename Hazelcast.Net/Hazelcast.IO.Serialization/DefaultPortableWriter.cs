using System.Collections.Generic;

namespace Hazelcast.IO.Serialization
{
    internal class DefaultPortableWriter : IPortableWriter
    {
        private readonly int begin;
        private readonly IClassDefinition cd;

        private readonly int offset;
        private readonly IBufferObjectDataOutput output;
        private readonly PortableSerializer serializer;

        private readonly ICollection<string> writtenFields;

        private bool raw;

        public DefaultPortableWriter(PortableSerializer serializer, IBufferObjectDataOutput output, IClassDefinition cd)
        {
            this.serializer = serializer;
            this.output = output;
            this.cd = cd;
            writtenFields = new HashSet<string>(new string[cd.GetFieldCount()]);
            begin = output.Position();

            output.WriteZeroBytes(4);  // room for final offset

            offset = output.Position();
            int fieldIndexesLength = (cd.GetFieldCount() + 1)*4;// one additional for raw data
            output.WriteZeroBytes(fieldIndexesLength);  // room for final offset
        }

        public virtual int GetVersion()
        {
            return cd.GetVersion();
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteInt(string fieldName, int value)
        {
            SetPosition(fieldName);
            output.WriteInt(value);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteLong(string fieldName, long value)
        {
            SetPosition(fieldName);
            output.WriteLong(value);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteUTF(string fieldName, string str)
        {
            SetPosition(fieldName);
            output.WriteUTF(str);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteBoolean(string fieldName, bool value)
        {
            SetPosition(fieldName);
            output.WriteBoolean(value);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteByte(string fieldName, byte value)
        {
            SetPosition(fieldName);
            output.WriteByte(value);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteChar(string fieldName, int value)
        {
            SetPosition(fieldName);
            output.WriteChar(value);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteDouble(string fieldName, double value)
        {
            SetPosition(fieldName);
            output.WriteDouble(value);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteFloat(string fieldName, float value)
        {
            SetPosition(fieldName);
            output.WriteFloat(value);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteShort(string fieldName, short value)
        {
            SetPosition(fieldName);
            output.WriteShort(value);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortable(string fieldName, IPortable portable)
        {
            SetPosition(fieldName);
            bool Null = portable == null;
            output.WriteBoolean(Null);
            if (!Null)
            {
                serializer.Write(output, portable);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteNullPortable(string fieldName, int factoryId, int classId)
        {
            SetPosition(fieldName);
            bool Null = true;
            output.WriteBoolean(Null);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteByteArray(string fieldName, byte[] values)
        {
            SetPosition(fieldName);
            IOUtil.WriteByteArray(output, values);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteCharArray(string fieldName, char[] values)
        {
            SetPosition(fieldName);
            output.WriteCharArray(values);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteIntArray(string fieldName, int[] values)
        {
            SetPosition(fieldName);
            output.WriteIntArray(values);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteLongArray(string fieldName, long[] values)
        {
            SetPosition(fieldName);
            output.WriteLongArray(values);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteDoubleArray(string fieldName, double[] values)
        {
            SetPosition(fieldName);
            output.WriteDoubleArray(values);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteFloatArray(string fieldName, float[] values)
        {
            SetPosition(fieldName);
            output.WriteFloatArray(values);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WriteShortArray(string fieldName, short[] values)
        {
            SetPosition(fieldName);
            output.WriteShortArray(values);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual void WritePortableArray(string fieldName, IPortable[] portables)
        {
            SetPosition(fieldName);
            int len = portables == null ? 0 : portables.Length;
            output.WriteInt(len);
            if (len > 0)
            {
                int offset = output.Position();
                output.WriteZeroBytes(len * 4);
                for (int i = 0; i < portables.Length; i++)
                {
                    output.WriteInt(offset + i*4, output.Position());
                    IPortable portable = portables[i];
                    serializer.Write(output, portable);
                }
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual IObjectDataOutput GetRawDataOutput()
        {
            if (!raw)
            {
                int pos = output.Position();
                int index = cd.GetFieldCount();
                // last index
                output.WriteInt(offset + index*4, pos);
            }
            raw = true;
            return output;
        }

        /// <exception cref="System.IO.IOException"></exception>
        private void SetPosition(string fieldName)
        {
            if (raw)
            {
                throw new HazelcastSerializationException(
                    "Cannot write IPortable fields after getRawDataOutput() is called!");
            }
            IFieldDefinition fd = cd.Get(fieldName);
            if (fd == null)
            {
                throw new HazelcastSerializationException("Invalid field name: '" + fieldName +
                                                          "' for IClassDefinition {id: " + cd.GetClassId() +
                                                          ", version: " + cd.GetVersion() + "}");
            }
            writtenFields.Add(fieldName);
            if (writtenFields.Contains(fieldName))
            {
                int pos = output.Position();
                int index = fd.GetIndex();
                output.WriteInt(offset + index*4, pos);
            }
            else
            {
                throw new HazelcastSerializationException("Field '" + fieldName + "' has already been written!");
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        internal virtual void End()
        {
            output.WriteInt(begin, output.Position());
        }

        // write final offset
    }
}