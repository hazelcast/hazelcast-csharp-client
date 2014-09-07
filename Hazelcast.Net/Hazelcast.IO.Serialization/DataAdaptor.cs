using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    internal class DataAdapter : SocketWritable, SocketReadable
    {
        private const int StType = 1;
        private const int StClassId = 2;
        private const int StFactoryId = 3;
        private const int StVersion = 4;
        private const int StClassDefSize = 5;
        private const int StClassDef = 6;
        private const int StSize = 7;
        private const int StValue = 8;
        private const int StHash = 9;
        private const int StAll = 10;

        protected internal Data data;
        
        private ByteBuffer buffer;
        private int factoryId;
        private int classId;
        private int version;
        private int classDefSize;
        private bool skipClassDef;

        [System.NonSerialized]
        private short status = 0;

        [System.NonSerialized]
        private readonly IPortableContext context;

        internal DataAdapter(Data data)
        {
            this.data = data;
        }

        internal DataAdapter(IPortableContext context)
        {
            this.context = context;
        }
        internal DataAdapter(Data data, IPortableContext context)
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
            if (!IsStatusSet(StType))
            {
                if (destination.Remaining() < 4)
                {
                    return false;
                }
                destination.PutInt(data.type);
                SetStatus(StType);
            }
            if (!IsStatusSet(StClassId))
            {
                if (destination.Remaining() < 4)
                {
                    return false;
                }
                classId = data.classDefinition == null ? Data.NoClassId : data.classDefinition.GetClassId();
                destination.PutInt(classId);
                if (classId == Data.NoClassId)
                {
                    SetStatus(StFactoryId);
                    SetStatus(StVersion);
                    SetStatus(StClassDefSize);
                    SetStatus(StClassDef);
                }
                SetStatus(StClassId);
            }
            if (!IsStatusSet(StFactoryId))
            {
                if (destination.Remaining() < 4)
                {
                    return false;
                }
                destination.PutInt(data.classDefinition.GetFactoryId());
                SetStatus(StFactoryId);
            }
            if (!IsStatusSet(StVersion))
            {
                if (destination.Remaining() < 4)
                {
                    return false;
                }
                int version = data.classDefinition.GetVersion();
                destination.PutInt(version);
                SetStatus(StVersion);
            }
            if (!IsStatusSet(StClassDefSize))
            {
                if (destination.Remaining() < 4)
                {
                    return false;
                }
                BinaryClassDefinition cd = (BinaryClassDefinition)data.classDefinition;
                byte[] binary = cd.GetBinary();
                classDefSize = binary == null ? 0 : binary.Length;
                destination.PutInt(classDefSize);
                SetStatus(StClassDefSize);
                if (classDefSize == 0)
                {
                    SetStatus(StClassDef);
                }
                else
                {
                    buffer = ByteBuffer.Wrap(binary);
                }
            }
            if (!IsStatusSet(StClassDef))
            {
                IOUtil.CopyToHeapBuffer(buffer, destination);
                if (buffer.HasRemaining())
                {
                    return false;
                }
                SetStatus(StClassDef);
            }
            if (!IsStatusSet(StSize))
            {
                if (destination.Remaining() < 4)
                {
                    return false;
                }
                int size = data.BufferSize();
                destination.PutInt(size);
                SetStatus(StSize);
                if (size <= 0)
                {
                    SetStatus(StValue);
                }
                else
                {
                    buffer = ByteBuffer.Wrap(data.buffer);
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
            if (!IsStatusSet(StHash))
            {
                if (destination.Remaining() < 4)
                {
                    return false;
                }
                destination.PutInt(data.GetPartitionHash());
                SetStatus(StHash);
            }
            SetStatus(StAll);
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
            if (!IsStatusSet(StType))
            {
                if (source.Remaining() < 4)
                {
                    return false;
                }
                data.type = source.GetInt();
                SetStatus(StType);
            }
            if (!IsStatusSet(StClassId))
            {
                if (source.Remaining() < 4)
                {
                    return false;
                }
                classId = source.GetInt();
                SetStatus(StClassId);
                if (classId == Data.NoClassId)
                {
                    SetStatus(StFactoryId);
                    SetStatus(StVersion);
                    SetStatus(StClassDefSize);
                    SetStatus(StClassDef);
                }
            }
            if (!IsStatusSet(StFactoryId))
            {
                if (source.Remaining() < 4)
                {
                    return false;
                }
                factoryId = source.GetInt();
                SetStatus(StFactoryId);
            }
            if (!IsStatusSet(StVersion))
            {
                if (source.Remaining() < 4)
                {
                    return false;
                }
                version = source.GetInt();
                SetStatus(StVersion);
            }
            if (!IsStatusSet(StClassDef))
            {
                IClassDefinition cd;
                if (!skipClassDef && (cd = context.Lookup(factoryId, classId, version)) != null)
                {
                    data.classDefinition = cd;
                    skipClassDef = true;
                }
                if (!IsStatusSet(StClassDefSize))
                {
                    if (source.Remaining() < 4)
                    {
                        return false;
                    }
                    classDefSize = source.GetInt();
                    SetStatus(StClassDefSize);
                }
                if (!IsStatusSet(StClassDef))
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
                    SetStatus(StClassDef);
                }
            }
            if (!IsStatusSet(StSize))
            {
                if (source.Remaining() < 4)
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
                data.buffer = ((byte[])buffer.Array());
                SetStatus(StValue);
            }
            if (!IsStatusSet(StHash))
            {
                if (source.Remaining() < 4)
                {
                    return false;
                }
                data.partitionHash = source.GetInt();
                SetStatus(StHash);
            }
            SetStatus(StAll);
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
            return IsStatusSet(StAll);
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
