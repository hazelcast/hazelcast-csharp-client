using System;
using System.Collections.Generic;
using System.IO;
using Hazelcast.IO;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    internal class DefaultPortableReader : IPortableReader
    {
        private const char NestedFieldPattern = '.';

        protected internal readonly IClassDefinition cd;

        protected internal readonly PortableSerializer serializer;

        private readonly IBufferObjectDataInput @in;

        private readonly int finalPosition;

        private readonly int offset;

        private bool raw;

        public DefaultPortableReader(PortableSerializer serializer, IBufferObjectDataInput @in, IClassDefinition cd)
        {
            this.@in = @in;
            this.serializer = serializer;
            this.cd = cd;
            int fieldCount;
            try
            {
                // final position after portable is read
                finalPosition = @in.ReadInt();
                // field count
                fieldCount = @in.ReadInt();
            }
            catch (IOException e)
            {
                throw new HazelcastSerializationException(e);
            }
            if (fieldCount != cd.GetFieldCount())
            {
                throw new InvalidOperationException("Field count[" + fieldCount + "] in stream does not match " + cd);
            }
            this.offset = @in.Position();
        }

        public virtual int GetVersion()
        {
            return cd.GetVersion();
        }

        public virtual bool HasField(string fieldName)
        {
            return cd.HasField(fieldName);
        }

        public virtual ICollection<string> GetFieldNames()
        {
            return cd.GetFieldNames();
        }

        public virtual FieldType GetFieldType(string fieldName)
        {
            return cd.GetFieldType(fieldName);
        }

        public virtual int GetFieldClassId(string fieldName)
        {
            return cd.GetFieldClassId(fieldName);
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual int ReadInt(string fieldName)
        {
            int pos = ReadPosition(fieldName, FieldType.Int);
            return @in.ReadInt(pos);
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual long ReadLong(string fieldName)
        {
            int pos = ReadPosition(fieldName, FieldType.Long);
            return @in.ReadLong(pos);
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual string ReadUTF(string fieldName)
        {
            int currentPos = @in.Position();
            try
            {
                int pos = ReadPosition(fieldName, FieldType.Utf);
                @in.Position(pos);
                return @in.ReadUTF();
            }
            finally
            {
                @in.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual bool ReadBoolean(string fieldName)
        {
            int pos = ReadPosition(fieldName, FieldType.Boolean);
            return @in.ReadBoolean(pos);
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual byte ReadByte(string fieldName)
        {
            int pos = ReadPosition(fieldName, FieldType.Byte);
            return @in.ReadByte(pos);
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual char ReadChar(string fieldName)
        {
            int pos = ReadPosition(fieldName, FieldType.Char);
            return @in.ReadChar(pos);
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual double ReadDouble(string fieldName)
        {
            int pos = ReadPosition(fieldName, FieldType.Double);
            return @in.ReadDouble(pos);
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual float ReadFloat(string fieldName)
        {
            int pos = ReadPosition(fieldName, FieldType.Float);
            return @in.ReadFloat(pos);
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual short ReadShort(string fieldName)
        {
            int pos = ReadPosition(fieldName, FieldType.Short);
            return @in.ReadShort(pos);
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual byte[] ReadByteArray(string fieldName)
        {
            int currentPos = @in.Position();
            try
            {
                int pos = ReadPosition(fieldName, FieldType.ByteArray);
                @in.Position(pos);
                return @in.ReadByteArray();
            }
            finally
            {
                @in.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual char[] ReadCharArray(string fieldName)
        {
            int currentPos = @in.Position();
            try
            {
                int pos = ReadPosition(fieldName, FieldType.CharArray);
                @in.Position(pos);
                return @in.ReadCharArray();
            }
            finally
            {
                @in.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual int[] ReadIntArray(string fieldName)
        {
            int currentPos = @in.Position();
            try
            {
                int pos = ReadPosition(fieldName, FieldType.IntArray);
                @in.Position(pos);
                return @in.ReadIntArray();
            }
            finally
            {
                @in.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual long[] ReadLongArray(string fieldName)
        {
            int currentPos = @in.Position();
            try
            {
                int pos = ReadPosition(fieldName, FieldType.LongArray);
                @in.Position(pos);
                return @in.ReadLongArray();
            }
            finally
            {
                @in.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual double[] ReadDoubleArray(string fieldName)
        {
            int currentPos = @in.Position();
            try
            {
                int pos = ReadPosition(fieldName, FieldType.DoubleArray);
                @in.Position(pos);
                return @in.ReadDoubleArray();
            }
            finally
            {
                @in.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual float[] ReadFloatArray(string fieldName)
        {
            int currentPos = @in.Position();
            try
            {
                int pos = ReadPosition(fieldName, FieldType.FloatArray);
                @in.Position(pos);
                return @in.ReadFloatArray();
            }
            finally
            {
                @in.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual short[] ReadShortArray(string fieldName)
        {
            int currentPos = @in.Position();
            try
            {
                int pos = ReadPosition(fieldName, FieldType.ShortArray);
                @in.Position(pos);
                return @in.ReadShortArray();
            }
            finally
            {
                @in.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual P ReadPortable<P>(string fieldName) where P:IPortable
        {
            int currentPos = @in.Position();
            try
            {
                IFieldDefinition fd = cd.GetField(fieldName);
                if (fd == null)
                {
                    throw ThrowUnknownFieldException(fieldName);
                }
                if (fd.GetFieldType() != FieldType.Portable)
                {
                    throw new HazelcastSerializationException("Not a Portable field: " + fieldName);
                }
                int pos = ReadPosition(fd);
                @in.Position(pos);
                bool isNull = @in.ReadBoolean();
                int factoryId = @in.ReadInt();
                int classId = @in.ReadInt();
                CheckFactoryAndClass(fd, factoryId, classId);
                if (!isNull)
                {
                    return (P)serializer.ReadAndInitialize(@in, factoryId, classId);
                }
                return default(P);
            }
            finally
            {
                @in.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual IPortable[] ReadPortableArray(string fieldName)
        {
            int currentPos = @in.Position();
            try
            {
                IFieldDefinition fd = cd.GetField(fieldName);
                if (fd == null)
                {
                    throw ThrowUnknownFieldException(fieldName);
                }
                if (fd.GetFieldType() != FieldType.PortableArray)
                {
                    throw new HazelcastSerializationException("Not a Portable array field: " + fieldName);
                }
                int pos = ReadPosition(fd);
                @in.Position(pos);
                int len = @in.ReadInt();
                int factoryId = @in.ReadInt();
                int classId = @in.ReadInt();
                CheckFactoryAndClass(fd, factoryId, classId);
                IPortable[] portables = new IPortable[len];
                if (len > 0)
                {
                    int offset = @in.Position();
                    for (int i = 0; i < len; i++)
                    {
                        int start = @in.ReadInt(offset + i * Bits.IntSizeInBytes);
                        @in.Position(start);
                        portables[i] = serializer.ReadAndInitialize(@in, factoryId, classId);
                    }
                }
                return portables;
            }
            finally
            {
                @in.Position(currentPos);
            }
        }

        private void CheckFactoryAndClass(IFieldDefinition fd, int factoryId, int classId)
        {
            if (factoryId != fd.GetFactoryId())
            {
                throw new ArgumentException("Invalid factoryId! Expected: " + fd.GetFactoryId() + ", Current: " + factoryId);
            }
            if (classId != fd.GetClassId())
            {
                throw new ArgumentException("Invalid classId! Expected: " + fd.GetClassId() + ", Current: " + classId);
            }
        }

        private HazelcastSerializationException ThrowUnknownFieldException(string fieldName)
        {
            return new HazelcastSerializationException("Unknown field name: '" + fieldName + "' for ClassDefinition {id: " + cd.GetClassId() + ", version: " + cd.GetVersion() + "}");
        }

        /// <exception cref="System.IO.IOException"/>
        private int ReadPosition(string fieldName, FieldType type)
        {
            if (raw)
            {
                throw new HazelcastSerializationException("Cannot read Portable fields after getRawDataInput() is called!");
            }
            IFieldDefinition fd = cd.GetField(fieldName);
            if (fd == null)
            {
                return ReadNestedPosition(fieldName, type);
            }
            if (fd.GetFieldType() != type)
            {
                throw new HazelcastSerializationException("Not a '" + type + "' field: " + fieldName);
            }
            return ReadPosition(fd);
        }

        /// <exception cref="System.IO.IOException"/>
        private int ReadNestedPosition(string fieldName, FieldType type)
        {
            string[] fieldNames = fieldName.Split(NestedFieldPattern);
            if (fieldNames.Length > 1)
            {
                IFieldDefinition fd = null;
                Hazelcast.IO.Serialization.DefaultPortableReader reader = this;
                for (int i = 0; i < fieldNames.Length; i++)
                {
                    fd = reader.cd.GetField(fieldNames[i]);
                    if (fd == null)
                    {
                        break;
                    }
                    if (i == fieldNames.Length - 1)
                    {
                        break;
                    }
                    int pos = reader.ReadPosition(fd);
                    @in.Position(pos);
                    bool isNull = @in.ReadBoolean();
                    if (isNull)
                    {
                        throw new ArgumentNullException("Parent field is null: " + fieldNames[i]);
                    }
                    reader = serializer.CreateReader(@in);
                }
                if (fd == null)
                {
                    throw ThrowUnknownFieldException(fieldName);
                }
                if (fd.GetFieldType() != type)
                {
                    throw new HazelcastSerializationException("Not a '" + type + "' field: " + fieldName);
                }
                return reader.ReadPosition(fd);
            }
            throw ThrowUnknownFieldException(fieldName);
        }

        /// <exception cref="System.IO.IOException"/>
        private int ReadPosition(IFieldDefinition fd)
        {
            int pos = @in.ReadInt(offset + fd.GetIndex() * Bits.IntSizeInBytes);
            short len = @in.ReadShort(pos);
            // name + len + type
            return pos + Bits.ShortSizeInBytes + len + 1;
        }

        /// <exception cref="System.IO.IOException"/>
        public virtual IObjectDataInput GetRawDataInput()
        {
            if (!raw)
            {
                int pos = @in.ReadInt(offset + cd.GetFieldCount() * Bits.IntSizeInBytes);
                @in.Position(pos);
            }
            raw = true;
            return @in;
        }

        /// <exception cref="System.IO.IOException"/>
        internal void End()
        {
            @in.Position(finalPosition);
        }
    }
}
