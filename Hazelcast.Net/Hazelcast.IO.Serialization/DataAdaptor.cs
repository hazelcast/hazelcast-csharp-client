using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    internal class DataAdapter : ISocketWritable, ISocketReadable
    {
        private const int StType = 1;
        private const int StSize = 2;
        private const int StValue = 3;
        private const int StHash = 4;
        private const int StAll = 5;

        protected internal IData data;

        protected internal IPortableContext context;

        private short status;

        private ByteBuffer buffer;

        private ClassDefinitionSerializer classDefinitionSerializer;

        public DataAdapter(IPortableContext context)
        {
            this.context = context;
        }

        public DataAdapter(IData data, IPortableContext context)
        {
            this.data = data;
            this.context = context;
        }

        public virtual bool IsUrgent()
        {
            return false;
        }

        public virtual bool WriteTo(ByteBuffer destination)
        {
            if (!IsStatusSet(StType))
            {
                if (destination.Remaining() < Bits.INT_SIZE_IN_BYTES + 1)
                {
                    return false;
                }
                int type = data.GetType();
                destination.PutInt(type);
                bool hasClassDefinition = context.HasClassDefinition(data);
                destination.Put(unchecked((byte)(hasClassDefinition ? 1 : 0)));
                if (hasClassDefinition)
                {
                    classDefinitionSerializer = new ClassDefinitionSerializer(data, context);
                }
                SetStatus(StType);
            }
            if (classDefinitionSerializer != null)
            {
                if (!classDefinitionSerializer.Write(destination))
                {
                    return false;
                }
            }
            if (!IsStatusSet(StHash))
            {
                if (destination.Remaining() < Bits.INT_SIZE_IN_BYTES)
                {
                    return false;
                }
                destination.PutInt(data.HasPartitionHash() ? data.GetPartitionHash() : 0);
                SetStatus(StHash);
            }
            if (!IsStatusSet(StSize))
            {
                if (destination.Remaining() < Bits.INT_SIZE_IN_BYTES)
                {
                    return false;
                }
                int size = data.DataSize();
                destination.PutInt(size);
                SetStatus(StSize);
                if (size <= 0)
                {
                    SetStatus(StValue);
                }
                else
                {
                    buffer = ByteBuffer.Wrap(data.GetData());
                }
            }
            if (!IsStatusSet(StValue))
            {
                IOUtil.CopyToHeapBuffer(buffer, destination);
                if (buffer.HasRemaining())
                {
                    return false;
                }
                SetStatus(StValue);
            }
            SetStatus(StAll);
            return true;
        }

        public virtual bool ReadFrom(ByteBuffer source)
        {
            if (data == null)
            {
                data = new Data();
            }
            if (!IsStatusSet(StType))
            {
                if (source.Remaining() < Bits.INT_SIZE_IN_BYTES + 1)
                {
                    return false;
                }
                int type = source.GetInt();
                ((Data)data).SetType(type);
                SetStatus(StType);
                bool hasClassDefinition = source.Get() != 0;
                if (hasClassDefinition)
                {
                    classDefinitionSerializer = new ClassDefinitionSerializer(data, context);
                }
            }
            if (classDefinitionSerializer != null)
            {
                if (!classDefinitionSerializer.Read(source))
                {
                    return false;
                }
            }
            if (!IsStatusSet(StHash))
            {
                if (source.Remaining() < Bits.INT_SIZE_IN_BYTES)
                {
                    return false;
                }
                ((Data)data).SetPartitionHash(source.GetInt());
                SetStatus(StHash);
            }
            if (!IsStatusSet(StSize))
            {
                if (source.Remaining() < Bits.INT_SIZE_IN_BYTES)
                {
                    return false;
                }
                int size = source.GetInt();
                buffer = ByteBuffer.Allocate(size);
                SetStatus(StSize);
            }
            if (!IsStatusSet(StValue))
            {
                IOUtil.CopyToHeapBuffer(source, buffer);
                if (buffer.HasRemaining())
                {
                    return false;
                }
                buffer.Flip();
                ((Data)data).SetData(((byte[])buffer.Array()));
                SetStatus(StValue);
            }
            SetStatus(StAll);
            return true;
        }

        protected internal void SetStatus(int bit)
        {
            unchecked
            {
                status |= (short)(1 << bit);
            }
        }

        protected internal bool IsStatusSet(int bit)
        {
            return (status & 1 << bit) != 0;
        }

        public IData GetData()
        {
            return data;
        }

        public void SetData(IData data)
        {
            this.data = data;
        }

        public virtual bool Done()
        {
            return IsStatusSet(StAll);
        }

        public virtual void Reset()
        {
            buffer = null;
            data = null;
            status = 0;
            classDefinitionSerializer = null;
        }

        public static int GetDataSize(IData data, IPortableContext context)
        {
            // type
            int total = Bits.INT_SIZE_IN_BYTES;
            // class def flag
            total += 1;
            if (context.HasClassDefinition(data))
            {
                IClassDefinition[] classDefinitions = context.GetClassDefinitions(data);
                if (classDefinitions == null || classDefinitions.Length == 0)
                {
                    throw new HazelcastSerializationException("ClassDefinition could not be found!");
                }
                // class definitions count
                total += Bits.INT_SIZE_IN_BYTES;
                foreach (IClassDefinition classDef in classDefinitions)
                {
                    // classDefinition-classId
                    total += Bits.INT_SIZE_IN_BYTES;
                    // classDefinition-factory-id
                    total += Bits.INT_SIZE_IN_BYTES;
                    // classDefinition-version
                    total += Bits.INT_SIZE_IN_BYTES;
                    // classDefinition-binary-length
                    total += Bits.INT_SIZE_IN_BYTES;
                    byte[] bytes = ((BinaryClassDefinition)classDef).GetBinary();
                    // classDefinition-binary
                    total += bytes.Length;
                }
            }
            // partition-hash
            total += Bits.INT_SIZE_IN_BYTES;
            // data-size
            total += Bits.INT_SIZE_IN_BYTES;
            // data
            total += data.DataSize();
            return total;
        }
    }
}
