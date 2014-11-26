using System;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    /// <summary>Serializes/De-serializes ClassDefinitions to/from buffer and streams.</summary>
    /// <remarks>
    ///     Serializes/De-serializes ClassDefinitions to/from buffer and streams.
    ///     <p />
    ///     Read/write from/to buffer methods are not thread safe.
    /// </remarks>
    internal class ClassDefinitionSerializer
    {
        private const int CLASS_DEF_HEADER_SIZE = 16;
        private const int StPrepared = 1;
        private const int StHeader = 2;
        private const int StData = 3;
        private const int StSkipData = 4;

        private readonly IPortableContext context;
        private readonly IData data;
        private ByteBuffer buffer;

        private int classDefCount;
        private int classDefIndex;

        private BinaryClassDefinition classDefProxy;
        private int classDefSize;
        private IClassDefinition[] classDefinitions;

        private byte[] metadata;
        private byte status;

        public ClassDefinitionSerializer(IData data, IPortableContext context)
        {
            // common fields
            // write fields
            // read fields
            this.data = data;
            this.context = context;
        }

        /// <summary>Writes a ClassDefinition to a buffer.</summary>
        /// <remarks>Writes a ClassDefinition to a buffer.</remarks>
        /// <param name="destination">buffer to write ClassDefinition</param>
        /// <returns>
        ///     true if ClassDefinition is fully written to the buffer,
        ///     false otherwise
        /// </returns>
        public virtual bool Write(ByteBuffer destination)
        {
            if (!IsStatusSet(StPrepared))
            {
                if (destination.Remaining() < Bits.INT_SIZE_IN_BYTES)
                {
                    return false;
                }
                classDefinitions = context.GetClassDefinitions(data);
                classDefCount = classDefinitions.Length;
                destination.PutInt(classDefCount);
                SetStatus(StPrepared);
            }
            if (!WriteAll(destination))
            {
                return false;
            }
            return true;
        }

        private bool WriteAll(ByteBuffer destination)
        {
            for (; classDefIndex < classDefCount; classDefIndex++)
            {
                var cd = (ClassDefinition) classDefinitions[classDefIndex];
                if (!WriteHeader(cd, destination))
                {
                    return false;
                }
                if (!WriteData(cd, destination))
                {
                    return false;
                }
                ClearStatus(StHeader);
                ClearStatus(StData);
            }
            return true;
        }

        private bool WriteHeader(ClassDefinition cd, ByteBuffer destination)
        {
            if (IsStatusSet(StHeader))
            {
                return true;
            }
            if (destination.Remaining() < CLASS_DEF_HEADER_SIZE)
            {
                return false;
            }
            destination.PutInt(cd.GetFactoryId());
            destination.PutInt(cd.GetClassId());
            destination.PutInt(cd.GetVersion());
            byte[] binary = cd.GetBinary();
            destination.PutInt(binary.Length);
            SetStatus(StHeader);
            return true;
        }

        private bool WriteData(ClassDefinition cd, ByteBuffer destination)
        {
            if (IsStatusSet(StData))
            {
                return true;
            }
            if (buffer == null)
            {
                buffer = ByteBuffer.Wrap(cd.GetBinary());
            }
            if (!FlushBuffer(destination))
            {
                return false;
            }
            buffer = null;
            SetStatus(StData);
            return true;
        }

        private bool FlushBuffer(ByteBuffer destination)
        {
            if (buffer.HasRemaining())
            {
                IOUtil.CopyToHeapBuffer(buffer, destination);
                if (buffer.HasRemaining())
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>Reads ClassDefinition from a buffer.</summary>
        /// <remarks>Reads ClassDefinition from a buffer.</remarks>
        /// <param name="source">buffer to read ClassDefinition from</param>
        /// <returns>
        ///     true if ClassDefinition is fully read from the buffer,
        ///     false otherwise
        /// </returns>
        public virtual bool Read(ByteBuffer source)
        {
            if (!IsStatusSet(StPrepared))
            {
                if (source.Remaining() < Bits.INT_SIZE_IN_BYTES)
                {
                    return false;
                }
                classDefCount = source.GetInt();
                metadata = new byte[classDefCount*PortableContext.HEADER_ENTRY_LENGTH];
                ((IMutableData) data).SetHeader(metadata);
                SetStatus(StPrepared);
            }
            if (!ReadAll(source))
            {
                return false;
            }
            return true;
        }

        private bool ReadAll(ByteBuffer source)
        {
            for (; classDefIndex < classDefCount; classDefIndex++)
            {
                if (!ReadHeader(source))
                {
                    return false;
                }
                if (!ReadData(source))
                {
                    return false;
                }
                ClearStatus(StHeader);
                ClearStatus(StData);
            }
            return true;
        }

        private bool ReadHeader(ByteBuffer source)
        {
            if (IsStatusSet(StHeader))
            {
                return true;
            }
            if (source.Remaining() < CLASS_DEF_HEADER_SIZE)
            {
                return false;
            }
            int factoryId = source.GetInt();
            int classId = source.GetInt();
            int version = source.GetInt();
            classDefSize = source.GetInt();
            bool bigEndian = context.GetByteOrder() == ByteOrder.BigEndian;
            Bits.WriteInt(metadata,
                classDefIndex*PortableContext.HEADER_ENTRY_LENGTH + PortableContext.HEADER_FACTORY_OFFSET, factoryId,
                bigEndian);
            Bits.WriteInt(metadata,
                classDefIndex*PortableContext.HEADER_ENTRY_LENGTH + PortableContext.HEADER_CLASS_OFFSET, classId,
                bigEndian);
            Bits.WriteInt(metadata,
                classDefIndex*PortableContext.HEADER_ENTRY_LENGTH + PortableContext.HEADER_VERSION_OFFSET, version,
                bigEndian);
            IClassDefinition cd = context.LookupClassDefinition(factoryId, classId, version);
            if (cd == null)
            {
                classDefProxy = new BinaryClassDefinitionProxy(factoryId, classId, version);
                ClearStatus(StSkipData);
            }
            else
            {
                SetStatus(StSkipData);
            }
            SetStatus(StHeader);
            return true;
        }

        private bool ReadData(ByteBuffer source)
        {
            if (IsStatusSet(StData))
            {
                return true;
            }
            if (IsStatusSet(StSkipData))
            {
                int skip = Math.Min(classDefSize, source.Remaining());
                source.Position = skip + source.Position;
                classDefSize -= skip;
                if (classDefSize > 0)
                {
                    return false;
                }
                ClearStatus(StSkipData);
            }
            else
            {
                if (buffer == null)
                {
                    buffer = ByteBuffer.Allocate(classDefSize);
                }
                IOUtil.CopyToHeapBuffer(source, buffer);
                if (buffer.HasRemaining())
                {
                    return false;
                }
                classDefProxy.SetBinary(buffer.Array());
                context.RegisterClassDefinition(classDefProxy);
                buffer = null;
                classDefProxy = null;
            }
            SetStatus(StData);
            return true;
        }

        private void SetStatus(int bit)
        {
            status = Bits.SetBit(status, bit);
        }

        private void ClearStatus(int bit)
        {
            status = Bits.ClearBit(status, bit);
        }

        private bool IsStatusSet(int bit)
        {
            return Bits.IsBitSet(status, bit);
        }
    }
}