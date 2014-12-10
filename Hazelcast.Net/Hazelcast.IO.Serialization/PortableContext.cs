using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Hazelcast.Core;
using Hazelcast.Net.Ext;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace Hazelcast.IO.Serialization
{
    internal sealed class PortableContext : IPortableContext
    {
        public const int HEADER_ENTRY_LENGTH = 12;
        public const int HEADER_FACTORY_OFFSET = 0;
        public const int HEADER_CLASS_OFFSET = 4;
        public const int HEADER_VERSION_OFFSET = 8;
        private const int COMPRESSION_BUFFER_LENGTH = 1024;


        private readonly ConcurrentDictionary<int, ClassDefinitionContext> classDefContextMap =
            new ConcurrentDictionary<int, ClassDefinitionContext>();

        private readonly ISerializationService serializationService;
        private readonly int version;

        internal PortableContext(ISerializationService serializationService, int version)
        {
            this.serializationService = serializationService;
            this.version = version;
        }

        public int GetClassVersion(int factoryId, int classId)
        {
            return GetClassDefContext(factoryId).GetClassVersion(classId);
        }

        public void SetClassVersion(int factoryId, int classId, int version)
        {
            GetClassDefContext(factoryId).SetClassVersion(classId, version);
        }

        public IClassDefinition LookupClassDefinition(int factoryId, int classId, int version)
        {
            return GetClassDefContext(factoryId).Lookup(classId, version);
        }

        public IClassDefinition LookupClassDefinition(IData data)
        {
            if (!data.IsPortable())
            {
                throw new ArgumentException("Data is not Portable!");
            }
            ByteOrder byteOrder = serializationService.GetByteOrder();
            return ReadClassDefinition(data, 0, byteOrder);
        }

        public bool HasClassDefinition(IData data)
        {
            if (data.IsPortable())
            {
                return true;
            }
            return data.HeaderSize() > 0;
        }

        public IClassDefinition[] GetClassDefinitions(IData data)
        {
            if (data.HeaderSize() == 0)
            {
                return null;
            }
            int len = data.HeaderSize();
            if (len%HEADER_ENTRY_LENGTH != 0)
            {
                throw new Exception("Header length should be factor of " + HEADER_ENTRY_LENGTH);
            }
            int k = len/HEADER_ENTRY_LENGTH;
            ByteOrder byteOrder = serializationService.GetByteOrder();
            var definitions = new IClassDefinition[k];
            for (int i = 0; i < k; i++)
            {
                definitions[i] = ReadClassDefinition(data, i*HEADER_ENTRY_LENGTH, byteOrder);
            }
            return definitions;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public IClassDefinition CreateClassDefinition(int factoryId, byte[] compressedBinary)
        {
            return GetClassDefContext(factoryId).Create(compressedBinary);
        }

        public IClassDefinition RegisterClassDefinition(IClassDefinition cd)
        {
            return GetClassDefContext(cd.GetFactoryId()).Register(cd);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public IClassDefinition LookupOrRegisterClassDefinition(IPortable p)
        {
            int portableVersion = PortableVersionHelper.GetVersion(p, version);
            IClassDefinition cd = LookupClassDefinition(p.GetFactoryId(), p.GetClassId(), portableVersion);
            if (cd == null)
            {
                var writer = new ClassDefinitionWriter(this, p.GetFactoryId(),p.GetClassId(), portableVersion);
                p.WritePortable(writer);
                cd = writer.RegisterAndGet();
            }
            return cd;
        }

        public IFieldDefinition GetFieldDefinition(IClassDefinition classDef, string name)
        {
            IFieldDefinition fd = classDef.GetField(name);
            if (fd == null)
            {
                string[] fieldNames = name.Split('.');
                if (fieldNames.Length > 1)
                {
                    IClassDefinition currentClassDef = classDef;
                    for (int i = 0; i < fieldNames.Length; i++)
                    {
                        name = fieldNames[i];
                        fd = currentClassDef.GetField(name);
                        if (i == fieldNames.Length - 1)
                        {
                            break;
                        }
                        if (fd == null)
                        {
                            throw new ArgumentException("Unknown field: " + name);
                        }
                        currentClassDef = LookupClassDefinition(fd.GetFactoryId(), fd.GetClassId(), currentClassDef.GetVersion());
                        if (currentClassDef == null)
                        {
                            throw new ArgumentException("Not a registered Portable field: " + fd);
                        }
                    }
                }
            }
            return fd;
        }

        public int GetVersion()
        {
            return version;
        }

        public IManagedContext GetManagedContext()
        {
            return serializationService.GetManagedContext();
        }

        public ByteOrder GetByteOrder()
        {
            return serializationService.GetByteOrder();
        }

        private IClassDefinition ReadClassDefinition(IData data, int start, ByteOrder order)
        {
            int factoryId = data.ReadIntHeader(start + HEADER_FACTORY_OFFSET, order);
            int classId = data.ReadIntHeader(start + HEADER_CLASS_OFFSET, order);
            int version = data.ReadIntHeader(start + HEADER_VERSION_OFFSET, order);
            return LookupClassDefinition(factoryId, classId, version);
        }

        private ClassDefinitionContext GetClassDefContext(int factoryId)
        {
            return classDefContextMap.GetOrAdd(factoryId, theFactoryId => new ClassDefinitionContext(this, theFactoryId));
        }

        /// <summary>Writes a ClassDefinition to a stream.</summary>
        /// <remarks>Writes a ClassDefinition to a stream.</remarks>
        /// <param name="classDefinition">ClassDefinition</param>
        /// <param name="output">stream to write ClassDefinition</param>
        /// <exception cref="System.IO.IOException"></exception>
        private static void WriteClassDefinition(IClassDefinition classDefinition, IObjectDataOutput output)
        {
            var cd = (ClassDefinition) classDefinition;
            output.WriteInt(cd.GetFactoryId());
            output.WriteInt(cd.GetClassId());
            output.WriteInt(cd.GetVersion());
            ICollection<IFieldDefinition> fieldDefinitions = cd.GetFieldDefinitions();
            output.WriteShort(fieldDefinitions.Count);
            foreach (IFieldDefinition fieldDefinition in fieldDefinitions)
            {
                WriteFieldDefinition((FieldDefinition) fieldDefinition, output);
            }
        }

        /// <summary>Reads a ClassDefinition from a stream.</summary>
        /// <remarks>Reads a ClassDefinition from a stream.</remarks>
        /// <param name="input">stream to write ClassDefinition</param>
        /// <returns>ClassDefinition</returns>
        /// <exception cref="System.IO.IOException"></exception>
        private static ClassDefinition ReadClassDefinition(IObjectDataInput input)
        {
            int factoryId = input.ReadInt();
            int classId = input.ReadInt();
            int version = input.ReadInt();
            if (classId == 0)
            {
                throw new ArgumentException("Portable class id cannot be zero!");
            }
            var cd = new ClassDefinition(factoryId, classId, version);
            int len = input.ReadShort();
            for (int i = 0; i < len; i++)
            {
                FieldDefinition fd = ReadFieldDefinition(input);
                cd.AddFieldDef(fd);
            }
            return cd;
        }

        /// <summary>Writes a FieldDefinition to a stream.</summary>
        /// <remarks>Writes a FieldDefinition to a stream.</remarks>
        /// <param name="fd">FieldDefinition</param>
        /// <param name="output">stream to write FieldDefinition</param>
        /// <exception cref="System.IO.IOException"></exception>
        private static void WriteFieldDefinition(FieldDefinition fd, IObjectDataOutput output)
        {
            output.WriteInt(fd.index);
            output.WriteUTF(fd.fieldName);
            output.WriteByte((byte) fd.type);
            output.WriteInt(fd.factoryId);
            output.WriteInt(fd.classId);
        }

        /// <summary>Reads a FieldDefinition from a stream.</summary>
        /// <remarks>Reads a FieldDefinition from a stream.</remarks>
        /// <param name="input">stream to write FieldDefinition</param>
        /// <returns>FieldDefinition</returns>
        /// <exception cref="System.IO.IOException"></exception>
        private static FieldDefinition ReadFieldDefinition(IObjectDataInput input)
        {
            int index = input.ReadInt();
            string name = input.ReadUTF();
            FieldType fieldType = (FieldType) input.ReadByte();
            int factoryId = input.ReadInt();
            int classId = input.ReadInt();
            return new FieldDefinition(index, name, fieldType, factoryId, classId);
        }

        /// <exception cref="System.IO.IOException"></exception>
        private static void Compress(byte[] input, IDataOutput output)
        {
            var deflater = new Deflater();
            deflater.SetLevel(Deflater.DEFAULT_COMPRESSION);
            deflater.SetStrategy(DeflateStrategy.Filtered);
            deflater.SetInput(input);
            deflater.Finish();
            var buf = new byte[COMPRESSION_BUFFER_LENGTH];
            while (!deflater.IsFinished)
            {
                int count = deflater.Deflate(buf);
                output.Write(buf, 0, count);
            }
            deflater.Finish();
        }

        /// <exception cref="System.IO.IOException"></exception>
        private static void Decompress(byte[] compressedData, IDataOutput output)
        {
            var inflater = new Inflater();
            inflater.SetInput(compressedData);
            var buf = new byte[COMPRESSION_BUFFER_LENGTH];
            while (!inflater.IsFinished)
            {
                try
                {
                    int count = inflater.Inflate(buf);
                    output.Write(buf, 0, count);
                }
                catch (SharpZipBaseException e)
                {
                    throw new IOException(e.Message);
                }
            }
        }

        internal static long CombineToLong(int x, int y)
        {
            return ((long) x << 32) | (y & unchecked(0xFFFFFFFL));
        }

        internal static int ExtractInt(long value, bool lowerBits)
        {
            return (lowerBits) ? (int) value : (int) (value >> 32);
        }

        private sealed class ClassDefinitionContext
        {
            private readonly ConcurrentDictionary<int, int> _currentClassVersions = new ConcurrentDictionary<int, int>();

            private readonly int _factoryId;
            private readonly PortableContext _portableContext;

            private readonly ConcurrentDictionary<long, IClassDefinition> _versionedDefinitions =
                new ConcurrentDictionary<long, IClassDefinition>();

            internal ClassDefinitionContext(PortableContext portableContext, int factoryId)
            {
                _portableContext = portableContext;
                _factoryId = factoryId;
            }

            internal int GetClassVersion(int classId)
            {
                int version;
                bool hasValue = _currentClassVersions.TryGetValue(classId, out version);
                return hasValue ? version : -1;
            }

            internal void SetClassVersion(int classId, int version)
            {
                bool hasAdded = _currentClassVersions.TryAdd(classId, version);
                if (!hasAdded)
                {
                    throw new ArgumentException("Class-id: " + classId + " is already registered!");
                }
            }

            internal IClassDefinition Lookup(int classId, int version)
            {
                IClassDefinition cd = null;
                _versionedDefinitions.TryGetValue(CombineToLong(classId, version), out cd);
                if (cd is BinaryClassDefinitionProxy)
                {
                    try
                    {
                        cd = Create(((BinaryClassDefinitionProxy) cd).GetBinary());
                    }
                    catch (IOException e)
                    {
                        throw new HazelcastSerializationException(e);
                    }
                }
                return cd;
            }

            /// <exception cref="System.IO.IOException"></exception>
            internal IClassDefinition Create(byte[] compressedBinary)
            {
                IClassDefinition cd = ToClassDefinition(compressedBinary);
                return Register(cd);
            }

            internal IClassDefinition Register(IClassDefinition cd)
            {
                if (cd == null)
                {
                    return null;
                }
                if (cd.GetFactoryId() != _factoryId)
                {
                    throw new HazelcastSerializationException("Invalid factory-id! " + _factoryId + " -> " + cd);
                }
                if (cd is ClassDefinition)
                {
                    var cdImpl = (ClassDefinition) cd;
                    cdImpl.SetVersionIfNotSet(_portableContext.GetVersion());
                    SetClassDefBinary(cdImpl);
                }
                long versionedClassId = CombineToLong(cd.GetClassId(), cd.GetVersion());
                IClassDefinition currentCd = _versionedDefinitions.GetOrAdd(versionedClassId, cd);
                if (Equals(currentCd, cd))
                {
                    return cd;
                }
                if (currentCd is ClassDefinition)
                {
                    if (!Equals(currentCd, cd))
                    {
                        throw new HazelcastSerializationException(
                            "Incompatible class-definitions with same class-id: " + cd + " VS " + currentCd);
                    }
                    return currentCd;
                }
                _versionedDefinitions.AddOrUpdate(versionedClassId, cd, (key, oldValue) => cd);
                return cd;
            }

            private void SetClassDefBinary(ClassDefinition cd)
            {
                if (cd.GetBinary() == null)
                {
                    try
                    {
                        byte[] binary = ToClassDefinitionBinary(cd);
                        cd.SetBinary(binary);
                    }
                    catch (IOException e)
                    {
                        throw new HazelcastSerializationException(e);
                    }
                }
            }

            /// <exception cref="System.IO.IOException"></exception>
            private byte[] ToClassDefinitionBinary(IClassDefinition cd)
            {
                IBufferObjectDataOutput output = _portableContext.serializationService.Pop();
                try
                {
                    WriteClassDefinition(cd, output);
                    byte[] binary = output.ToByteArray();
                    output.Clear();
                    Compress(binary, output);
                    return output.ToByteArray();
                }
                finally
                {
                    _portableContext.serializationService.Push(output);
                }
            }

            /// <exception cref="System.IO.IOException"></exception>
            private IClassDefinition ToClassDefinition(byte[] compressedBinary)
            {
                if (compressedBinary == null || compressedBinary.Length == 0)
                {
                    throw new IOException("Illegal class-definition binary! ");
                }
                IBufferObjectDataOutput output = _portableContext.serializationService.Pop();
                byte[] binary;
                try
                {
                    Decompress(compressedBinary, output);
                    binary = output.ToByteArray();
                }
                finally
                {
                    _portableContext.serializationService.Push(output);
                }
                ClassDefinition cd =
                    ReadClassDefinition(_portableContext.serializationService.CreateObjectDataInput(binary));
                if (cd.GetVersion() < 0)
                {
                    throw new IOException("ClassDefinition version cannot be negative! -> " + cd);
                }
                cd.SetBinary(compressedBinary);
                return cd;
            }
        }
    }
}