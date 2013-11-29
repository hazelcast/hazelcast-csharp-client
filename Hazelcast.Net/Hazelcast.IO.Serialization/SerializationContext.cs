using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Hazelcast.Core;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace Hazelcast.IO.Serialization
{
    internal sealed class SerializationContext : ISerializationContext
    {
        internal readonly IDictionary<int, PortableContext> portableContextMap;

        internal readonly SerializationService serializationService;
        internal readonly int version;

        internal SerializationContext(SerializationService serializationService,
            ICollection<int> portableFactories, int version)
        {
            this.serializationService = serializationService;
            this.version = version;
            IDictionary<int, PortableContext> portableMap = new Dictionary<int, PortableContext>();
            foreach (int factoryId in portableFactories)
            {
                portableMap.Add(factoryId, new PortableContext(this));
            }
            portableContextMap = portableMap;
        }

        // do not modify!
        public IClassDefinition Lookup(int factoryId, int classId)
        {
            return GetPortableContext(factoryId).Lookup(classId, version);
        }

        public IClassDefinition Lookup(int factoryId, int classId, int version)
        {
            return GetPortableContext(factoryId).Lookup(classId, version);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public IClassDefinition CreateClassDefinition(int factoryId, byte[] compressedBinary)
        {
            return GetPortableContext(factoryId).CreateClassDefinition(compressedBinary);
        }

        public IClassDefinition RegisterClassDefinition(IClassDefinition cd)
        {
            return GetPortableContext(cd.GetFactoryId()).RegisterClassDefinition(cd);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public IClassDefinition LookupOrRegisterClassDefinition(IPortable p)
        {
            IClassDefinition cd = Lookup(p.GetFactoryId(), p.GetClassId());
            if (cd == null)
            {
                var classDefinitionWriter = new ClassDefinitionWriter(this, p.GetFactoryId(), p.GetClassId());
                p.WritePortable(classDefinitionWriter);
                cd = classDefinitionWriter.RegisterAndGet();
            }
            return cd;
        }

        public int GetVersion()
        {
            return version;
        }

        public IManagedContext GetManagedContext()
        {
            return serializationService.GetManagedContext();
        }

        private void RegisterNestedDefinitions(ClassDefinition cd)
        {
            ICollection<IClassDefinition> nestedDefinitions = cd.GetNestedClassDefinitions();
            foreach (IClassDefinition classDefinition in nestedDefinitions)
            {
                var nestedCD = (ClassDefinition) classDefinition;
                RegisterClassDefinition(nestedCD);
                RegisterNestedDefinitions(nestedCD);
            }
        }

        private PortableContext GetPortableContext(int factoryId)
        {
            PortableContext ctx = null;
            portableContextMap.TryGetValue(factoryId, out ctx);
            if (ctx == null)
            {
                throw new HazelcastSerializationException("Could not find IPortableFactory for factoryId: " + factoryId);
            }
            return ctx;
        }

        /// <exception cref="System.IO.IOException"></exception>
        //internal static void Compress(byte[] decompressedData, IBufferObjectDataOutput output)
        //{
        //    var compressedStream = new MemoryStream();
        //    var compressionStream = new DeflateStream(compressedStream, CompressionMode.Compress);
        //    compressionStream.Write(decompressedData,0,decompressedData.Length);
        //    compressionStream.Close();
        //    byte[] compressedData = compressedStream.ToArray();
        //    output.Write(compressedData);
        //    compressedStream.Close();
        //}
        internal static void CompressZip(byte[] decompressedData, IBufferObjectDataOutput output)
        {
            var compressedStream = new MemoryStream();
            var compressionStream = new DeflaterOutputStream(compressedStream);

            compressionStream.Write(decompressedData, 0, decompressedData.Length);
            compressionStream.Close();

            byte[] compressedData = compressedStream.ToArray();

            output.Write(compressedData);
            compressedStream.Close();
        }

        /// <exception cref="System.IO.IOException"></exception>
        //internal static void Decompress(byte[] compressedData, IBufferObjectDataOutput output)
        //{
        //    var decompressedStream = new MemoryStream();
        //    var decompressionStream = new DeflateStream(decompressedStream, CompressionMode.Decompress);
        //    decompressionStream.Write(compressedData,0,compressedData.Length);
        //    decompressionStream.Close();
        //    byte[] decompressedData = decompressedStream.ToArray();
        //    output.Write(decompressedData);
        //    decompressedStream.Close();
        //}
        internal static void DecompressZip(byte[] compressedData, IBufferObjectDataOutput output)
        {
            //var decompressedStream = new MemoryStream();
            //var decompressionStream =new InflaterInputStream(decompressedStream);
            //decompressionStream.IsStreamOwner = false;

            //decompressionStream.Read(compressedData,0,compressedData.Length);

            //decompressionStream.Close();
            //byte[] decompressedData = decompressedStream.ToArray();
            //output.Write(decompressedData);
            //decompressedStream.Close();


            byte[] resBuffer = null;

            var mInStream = new MemoryStream(compressedData);
            var mOutStream = new MemoryStream(compressedData.Length);
            var infStream = new InflaterInputStream(mInStream);

            mInStream.Position = 0;

            try
            {
                var tmpBuffer = new byte[compressedData.Length];
                int read = 0;
                do
                {
                    read = infStream.Read(tmpBuffer, 0, tmpBuffer.Length);
                    if (read > 0)
                    {
                        mOutStream.Write(tmpBuffer, 0, read);
                    }
                } while (read > 0);

                resBuffer = mOutStream.ToArray();
            }
            finally
            {
                infStream.Close();
                mInStream.Close();
                mOutStream.Close();
            }

            output.Write(resBuffer);
        }

        internal static long CombineToLong(int x, int y)
        {
            return ((long) x << 32) | (y & unchecked(0xFFFFFFFL));
        }

        internal static int ExtractInt(long value, bool lowerBits)
        {
            return (lowerBits) ? (int) value : (int) (value >> 32);
        }

        internal class PortableContext
        {
            private readonly SerializationContext _enclosing;

            internal readonly ConcurrentDictionary<long, ClassDefinition> versionedDefinitions =
                new ConcurrentDictionary<long, ClassDefinition>();

            internal PortableContext(SerializationContext _enclosing)
            {
                this._enclosing = _enclosing;
            }

            internal virtual IClassDefinition Lookup(int classId, int version)
            {
                ClassDefinition retVal = null;
                versionedDefinitions.TryGetValue(CombineToLong(classId, version), out retVal);
                return retVal;
            }

            /// <exception cref="System.IO.IOException"></exception>
            internal virtual IClassDefinition CreateClassDefinition(byte[] compressedBinary)
            {
                if (compressedBinary == null || compressedBinary.Length == 0)
                {
                    throw new IOException("Illegal class-definition binary! ");
                }
                IBufferObjectDataOutput output = _enclosing.serializationService.Pop();
                byte[] binary;
                try
                {
                    DecompressZip(compressedBinary, output);
                    //Decompress(compressedBinary, output);
                    binary = output.ToByteArray();
                }
                finally
                {
                    _enclosing.serializationService.Push(output);
                }
                var cd = new ClassDefinition();
                cd.ReadData(_enclosing.serializationService.CreateObjectDataInput(binary));
                cd.SetBinary(compressedBinary);
                _enclosing.RegisterNestedDefinitions(cd);
                ClassDefinition currentCd =
                    versionedDefinitions.GetOrAdd(CombineToLong(cd.classId, _enclosing.GetVersion()), cd);
                return currentCd ?? cd;
            }

            internal virtual IClassDefinition RegisterClassDefinition(IClassDefinition cd)
            {
                if (cd == null)
                {
                    return null;
                }
                var cdImpl = (ClassDefinition) cd;
                if (cdImpl.GetVersion() < 0)
                {
                    cdImpl.version = _enclosing.GetVersion();
                }
                if (cdImpl.GetBinary() == null)
                {
                    IBufferObjectDataOutput output = _enclosing.serializationService.Pop();
                    try
                    {
                        cdImpl.WriteData(output);
                        byte[] binary = output.ToByteArray();
                        output.Clear();
                        CompressZip(binary, output);
                        //Compress(binary, output);
                        cdImpl.SetBinary(output.ToByteArray());
                    }
                    catch (IOException e)
                    {
                        throw new HazelcastSerializationException(e);
                    }
                    finally
                    {
                        _enclosing.serializationService.Push(output);
                    }
                }
                long versionedClassId = CombineToLong(cdImpl.GetClassId(), cdImpl.GetVersion());
                _enclosing.RegisterNestedDefinitions(cdImpl);
                ClassDefinition currentCd = versionedDefinitions.GetOrAdd(versionedClassId, cdImpl);
                return currentCd ?? cdImpl;
            }
        }
    }
}