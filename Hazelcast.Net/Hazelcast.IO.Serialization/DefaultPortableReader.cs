using System.Collections.Generic;
using System.IO;

namespace Hazelcast.IO.Serialization
{
    public class DefaultPortableReader : IPortableReader
    {
        protected internal readonly IClassDefinition cd;

        private readonly int finalPosition;
        private readonly IBufferObjectDataInput input;

        private readonly int offset;
        private readonly PortableSerializer serializer;

        private bool raw;

        public DefaultPortableReader(PortableSerializer serializer, IBufferObjectDataInput input, IClassDefinition cd)
        {
            this.input = input;
            this.serializer = serializer;
            this.cd = cd;
            try
            {
                finalPosition = input.ReadInt();
            }
            catch (IOException e)
            {
                // final position after portable is read
                throw new HazelcastSerializationException(e);
            }
            offset = input.Position();
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

        /// <exception cref="System.IO.IOException"></exception>
        public virtual int ReadInt(string fieldName)
        {
            int pos = GetPosition(fieldName);
            return input.ReadInt(pos);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual long ReadLong(string fieldName)
        {
            int pos = GetPosition(fieldName);
            return input.ReadLong(pos);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual string ReadUTF(string fieldName)
        {
            int currentPos = input.Position();
            try
            {
                int pos = GetPosition(fieldName);
                input.Position(pos);
                return input.ReadUTF();
            }
            finally
            {
                input.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual bool ReadBoolean(string fieldName)
        {
            int pos = GetPosition(fieldName);
            return input.ReadBoolean(pos);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual byte ReadByte(string fieldName)
        {
            int pos = GetPosition(fieldName);
            return input.ReadByte(pos);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual char ReadChar(string fieldName)
        {
            int pos = GetPosition(fieldName);
            return input.ReadChar(pos);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual double ReadDouble(string fieldName)
        {
            int pos = GetPosition(fieldName);
            return input.ReadDouble(pos);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual float ReadFloat(string fieldName)
        {
            int pos = GetPosition(fieldName);
            return input.ReadFloat(pos);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual short ReadShort(string fieldName)
        {
            int pos = GetPosition(fieldName);
            return input.ReadShort(pos);
        }

        //public P ReadPortable<P>(string fieldName) where P : IPortable
        //{
        //    throw new System.NotImplementedException();
        //}

        /// <exception cref="System.IO.IOException"></exception>
        public virtual byte[] ReadByteArray(string fieldName)
        {
            int currentPos = input.Position();
            try
            {
                int pos = GetPosition(fieldName);
                input.Position(pos);
                return IOUtil.ReadByteArray(input);
            }
            finally
            {
                input.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual char[] ReadCharArray(string fieldName)
        {
            int currentPos = input.Position();
            try
            {
                int pos = GetPosition(fieldName);
                input.Position(pos);
                return input.ReadCharArray();
            }
            finally
            {
                input.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual int[] ReadIntArray(string fieldName)
        {
            int currentPos = input.Position();
            try
            {
                int pos = GetPosition(fieldName);
                input.Position(pos);
                return input.ReadIntArray();
            }
            finally
            {
                input.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual long[] ReadLongArray(string fieldName)
        {
            int currentPos = input.Position();
            try
            {
                int pos = GetPosition(fieldName);
                input.Position(pos);
                return input.ReadLongArray();
            }
            finally
            {
                input.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual double[] ReadDoubleArray(string fieldName)
        {
            int currentPos = input.Position();
            try
            {
                int pos = GetPosition(fieldName);
                input.Position(pos);
                return input.ReadDoubleArray();
            }
            finally
            {
                input.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual float[] ReadFloatArray(string fieldName)
        {
            int currentPos = input.Position();
            try
            {
                int pos = GetPosition(fieldName);
                input.Position(pos);
                return input.ReadFloatArray();
            }
            finally
            {
                input.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual short[] ReadShortArray(string fieldName)
        {
            int currentPos = input.Position();
            try
            {
                int pos = GetPosition(fieldName);
                input.Position(pos);
                return input.ReadShortArray();
            }
            finally
            {
                input.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual IPortable[] ReadPortableArray(string fieldName)
        {
            IFieldDefinition fd = cd.Get(fieldName);
            if (fd == null)
            {
                throw ThrowUnknownFieldException(fieldName);
            }
            int currentPos = input.Position();
            try
            {
                int pos = GetPosition(fd);
                input.Position(pos);
                int len = input.ReadInt();
                var portables = new IPortable[len];
                if (len > 0)
                {
                    int offset = input.Position();
                    var ctxIn = (PortableContextAwareInputStream) input;
                    try
                    {
                        ctxIn.SetFactoryId(fd.GetFactoryId());
                        ctxIn.SetClassId(fd.GetClassId());
                        for (int i = 0; i < len; i++)
                        {
                            int start = input.ReadInt(offset + i*4);
                            input.Position(start);
                            portables[i] = serializer.ReadAndInitialize(input);
                        }
                    }
                    finally
                    {
                        ctxIn.SetFactoryId(cd.GetFactoryId());
                        ctxIn.SetClassId(cd.GetClassId());
                    }
                }
                return portables;
            }
            finally
            {
                input.Position(currentPos);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual IObjectDataInput GetRawDataInput()
        {
            if (!raw)
            {
                int pos = input.ReadInt(offset + cd.GetFieldCount()*4);
                input.Position(pos);
            }
            raw = true;
            return input;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public virtual P ReadPortable<P>(string fieldName) where P : IPortable
        {
            IFieldDefinition fd = cd.Get(fieldName);
            if (fd == null)
            {
                throw ThrowUnknownFieldException(fieldName);
            }
            int currentPos = input.Position();
            try
            {
                int pos = GetPosition(fd);
                input.Position(pos);
                bool Null = input.ReadBoolean();
                if (!Null)
                {
                    var ctxIn = (PortableContextAwareInputStream) input;
                    try
                    {
                        ctxIn.SetFactoryId(fd.GetFactoryId());
                        ctxIn.SetClassId(fd.GetClassId());
                        return (P) serializer.ReadAndInitialize(input);
                    }
                    finally
                    {
                        ctxIn.SetFactoryId(cd.GetFactoryId());
                        ctxIn.SetClassId(cd.GetClassId());
                    }
                }
                return default(P);
            }
            finally
            {
                input.Position(currentPos);
            }
        }

        private HazelcastSerializationException ThrowUnknownFieldException(string fieldName)
        {
            return
                new HazelcastSerializationException("Unknown field name: '" + fieldName + "' for IClassDefinition {id: " +
                                                    cd.GetClassId() + ", version: " + cd.GetVersion() + "}");
        }

        /// <exception cref="System.IO.IOException"></exception>
        protected internal virtual int GetPosition(string fieldName)
        {
            if (raw)
            {
                throw new HazelcastSerializationException(
                    "Cannot read IPortable fields after getRawDataInput() is called!");
            }
            IFieldDefinition fd = cd.Get(fieldName);
            if (fd == null)
            {
                throw ThrowUnknownFieldException(fieldName);
            }
            return GetPosition(fd);
        }

        /// <exception cref="System.IO.IOException"></exception>
        protected internal virtual int GetPosition(IFieldDefinition fd)
        {
            return input.ReadInt(offset + fd.GetIndex()*4);
        }

        /// <exception cref="System.IO.IOException"></exception>
        internal virtual void End()
        {
            input.Position(finalPosition);
        }
    }
}