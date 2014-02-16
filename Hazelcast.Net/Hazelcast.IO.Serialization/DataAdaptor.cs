using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    internal class DataAdapter : SocketWritable, SocketReadable
    {
        protected internal static int stBit = 0;

        private static readonly int stType = stBit++;

        private static readonly int stClassId = stBit++;

        private static readonly int stFactoryId = stBit++;

        private static readonly int stVersion = stBit++;

        private static readonly int stClassDefSize = stBit++;

        private static readonly int stClassDef = stBit++;

        private static readonly int stSize = stBit++;

        private static readonly int stValue = stBit++;

        private static readonly int stHash = stBit++;

        private static readonly int stAll = stBit++;

        private ByteBuffer buffer;

        private int factoryId = 0;

        private int classId = 0;

        private int version = 0;

        private int classDefSize = 0;

        private bool skipClassDef = false;

        protected internal Data data;

        [System.NonSerialized]
        private short status = 0;

        [System.NonSerialized]
        private SerializationContext context;

        internal DataAdapter(Data data)
        {
            this.data = data;
        }

        internal DataAdapter(SerializationContext context)
        {
            this.context = context;
        }
        internal DataAdapter(ISerializationContext context)
        {
            this.context = context as SerializationContext;
        }

        internal DataAdapter(Data data, SerializationContext context)
        {
            this.data = data;
            this.context = context;
        }

        /// <summary>
        /// WARNING:
        /// Should be in sync with
        /// <see cref="Data#writeData(com.hazelcast.nio.ObjectDataOutput)">Data#writeData(com.hazelcast.nio.ObjectDataOutput)</see>
        /// </summary>
        public virtual bool WriteTo(ByteBuffer destination)
        {
            if (!IsStatusSet(stType))
            {
                if (destination.Remaining() < 4)
                {
                    return false;
                }
                destination.PutInt(data.type);
                SetStatus(stType);
            }
            if (!IsStatusSet(stClassId))
            {
                if (destination.Remaining() < 4)
                {
                    return false;
                }
                classId = data.classDefinition == null ? Data.NoClassId : data.classDefinition.GetClassId();
                destination.PutInt(classId);
                if (classId == Data.NoClassId)
                {
                    SetStatus(stFactoryId);
                    SetStatus(stVersion);
                    SetStatus(stClassDefSize);
                    SetStatus(stClassDef);
                }
                SetStatus(stClassId);
            }
            if (!IsStatusSet(stFactoryId))
            {
                if (destination.Remaining() < 4)
                {
                    return false;
                }
                destination.PutInt(data.classDefinition.GetFactoryId());
                SetStatus(stFactoryId);
            }
            if (!IsStatusSet(stVersion))
            {
                if (destination.Remaining() < 4)
                {
                    return false;
                }
                int version = data.classDefinition.GetVersion();
                destination.PutInt(version);
                SetStatus(stVersion);
            }
            if (!IsStatusSet(stClassDefSize))
            {
                if (destination.Remaining() < 4)
                {
                    return false;
                }
                BinaryClassDefinition cd = (BinaryClassDefinition)data.classDefinition;
                byte[] binary = cd.GetBinary();
                classDefSize = binary == null ? 0 : binary.Length;
                destination.PutInt(classDefSize);
                SetStatus(stClassDefSize);
                if (classDefSize == 0)
                {
                    SetStatus(stClassDef);
                }
                else
                {
                    buffer = ByteBuffer.Wrap(binary);
                }
            }
            if (!IsStatusSet(stClassDef))
            {
                IOUtil.CopyToHeapBuffer(buffer, destination);
                if (buffer.HasRemaining())
                {
                    return false;
                }
                SetStatus(stClassDef);
            }
            if (!IsStatusSet(stSize))
            {
                if (destination.Remaining() < 4)
                {
                    return false;
                }
                int size = data.BufferSize();
                destination.PutInt(size);
                SetStatus(stSize);
                if (size <= 0)
                {
                    SetStatus(stValue);
                }
                else
                {
                    buffer = ByteBuffer.Wrap(data.buffer);
                }
            }
            if (!IsStatusSet(stValue))
            {
                IOUtil.CopyToHeapBuffer(buffer, destination);
                if (buffer.HasRemaining())
                {
                    return false;
                }
                SetStatus(stValue);
            }
            if (!IsStatusSet(stHash))
            {
                if (destination.Remaining() < 4)
                {
                    return false;
                }
                destination.PutInt(data.GetPartitionHash());
                SetStatus(stHash);
            }
            SetStatus(stAll);
            return true;
        }

        /// <summary>
        /// WARNING:
        /// Should be in sync with
        /// <see cref="Data#readData(com.hazelcast.nio.ObjectDataInput)">Data#readData(com.hazelcast.nio.ObjectDataInput)</see>
        /// </summary>
        public virtual bool ReadFrom(ByteBuffer source)
        {
            if (data == null)
            {
                data = new Data();
            }
            if (!IsStatusSet(stType))
            {
                if (source.Remaining() < 4)
                {
                    return false;
                }
                data.type = source.GetInt();
                SetStatus(stType);
            }
            if (!IsStatusSet(stClassId))
            {
                if (source.Remaining() < 4)
                {
                    return false;
                }
                classId = source.GetInt();
                SetStatus(stClassId);
                if (classId == Data.NoClassId)
                {
                    SetStatus(stFactoryId);
                    SetStatus(stVersion);
                    SetStatus(stClassDefSize);
                    SetStatus(stClassDef);
                }
            }
            if (!IsStatusSet(stFactoryId))
            {
                if (source.Remaining() < 4)
                {
                    return false;
                }
                factoryId = source.GetInt();
                SetStatus(stFactoryId);
            }
            if (!IsStatusSet(stVersion))
            {
                if (source.Remaining() < 4)
                {
                    return false;
                }
                version = source.GetInt();
                SetStatus(stVersion);
            }
            if (!IsStatusSet(stClassDef))
            {
                IClassDefinition cd;
                if (!skipClassDef && (cd = context.Lookup(factoryId, classId, version)) != null)
                {
                    data.classDefinition = cd;
                    skipClassDef = true;
                }
                if (!IsStatusSet(stClassDefSize))
                {
                    if (source.Remaining() < 4)
                    {
                        return false;
                    }
                    classDefSize = source.GetInt();
                    SetStatus(stClassDefSize);
                }
                if (!IsStatusSet(stClassDef))
                {
                    if (source.Remaining() < classDefSize)
                    {
                        return false;
                    }
                    if (skipClassDef)
                    {
                        source.Position = classDefSize + source.Position;
                    }
                    else
                    {
                        byte[] binary = new byte[classDefSize];
                        source.Get(binary);
                        data.classDefinition = new BinaryClassDefinitionProxy(factoryId, classId, version, binary);
                    }
                    SetStatus(stClassDef);
                }
            }
            if (!IsStatusSet(stSize))
            {
                if (source.Remaining() < 4)
                {
                    return false;
                }
                int size = source.GetInt();
                buffer = ByteBuffer.Allocate(size);
                SetStatus(stSize);
            }
            if (!IsStatusSet(stValue))
            {
                IOUtil.CopyToHeapBuffer(source, buffer);
                if (buffer.HasRemaining())
                {
                    return false;
                }
                buffer.Flip();
                data.buffer = ((byte[])buffer.Array());
                SetStatus(stValue);
            }
            if (!IsStatusSet(stHash))
            {
                if (source.Remaining() < 4)
                {
                    return false;
                }
                data.partitionHash = source.GetInt();
                SetStatus(stHash);
            }
            SetStatus(stAll);
            return true;
        }

        protected internal void SetStatus(int bit)
        {
            status |= (short)(1 << bit);
        }

        protected internal bool IsStatusSet(int bit)
        {
            return (status & 1 << bit) != 0;
        }

        public Data GetData()
        {
            data.PostConstruct(context);
            return data;
        }

        public void SetData(Data data)
        {
            this.data = data;
        }

        public virtual bool Done()
        {
            return IsStatusSet(stAll);
        }

        public virtual void OnEnqueue()
        {
        }

        public virtual void Reset()
        {
            buffer = null;
            classId = 0;
            version = 0;
            classDefSize = 0;
            data = null;
            status = 0;
        }
    }
}
