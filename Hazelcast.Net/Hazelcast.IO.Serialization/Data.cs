using System;
using System.Text;

namespace Hazelcast.IO.Serialization
{
    [Serializable]
    public sealed class Data : IIdentifiedDataSerializable
    {
        public const int FactoryId = 0;
        public const int Id = 0;
        public const int NoClassId = 0;
        internal byte[] buffer = null;
        internal IClassDefinition classDefinition = null;
        internal int partitionHash = 0;
        internal int type = SerializationConstants.ConstantTypeData;

        public Data()
        {
        }

        public Data(int type, byte[] bytes)
        {
            // WARNING: IPortable class-id cannot be zero.
            //    transient int hash;
            this.type = type;
            buffer = bytes;
        }

        /// <summary>
        ///     WARNING:
        ///     <p />
        ///     Should be in sync with
        /// </summary>
        /// <exception cref="System.IO.IOException"></exception>
        public void ReadData(IObjectDataInput input)
        {
            type = input.ReadInt();
            int classId = input.ReadInt();
            if (classId != NoClassId)
            {
                int factoryId = input.ReadInt();
                int version = input.ReadInt();
                ISerializationContext context = ((ISerializationContextAware) input).GetSerializationContext();
                classDefinition = context.Lookup(factoryId, classId, version);
                int classDefSize = input.ReadInt();
                if (classDefinition != null)
                {
                    input.SkipBytes(classDefSize);
                }
                else
                {
                    var classDefBytes = new byte[classDefSize];
                    input.ReadFully(classDefBytes);
                    classDefinition = context.CreateClassDefinition(factoryId, classDefBytes);
                }
            }
            int size = input.ReadInt();
            if (size > 0)
            {
                buffer = new byte[size];
                input.ReadFully(buffer);
            }
            partitionHash = input.ReadInt();
        }

        public string GetJavaClassName()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     WARNING:
        ///     <p />
        ///     Should be in sync with
        ///     <p />
        ///     <see cref="TotalSize()">TotalSize()</see>
        ///     should be updated whenever writeData method is changed.
        /// </summary>
        /// <exception cref="System.IO.IOException"></exception>
        public void WriteData(IObjectDataOutput output)
        {
            output.WriteInt(type);
            if (classDefinition != null)
            {
                output.WriteInt(classDefinition.GetClassId());
                output.WriteInt(classDefinition.GetFactoryId());
                output.WriteInt(classDefinition.GetVersion());
                byte[] classDefBytes = ((BinaryClassDefinition) classDefinition).GetBinary();
                output.WriteInt(classDefBytes.Length);
                output.Write(classDefBytes);
            }
            else
            {
                output.WriteInt(NoClassId);
            }
            int size = BufferSize();
            output.WriteInt(size);
            if (size > 0)
            {
                output.Write(buffer);
            }
            output.WriteInt(GetPartitionHash());
        }

        public int GetFactoryId()
        {
            return FactoryId;
        }

        public int GetId()
        {
            return Id;
        }

        public int BufferSize()
        {
            return (buffer == null) ? 0 : buffer.Length;
        }

        /// <summary>Calculates the size of the binary after the Data is serialized.</summary>
        /// <remarks>
        ///     Calculates the size of the binary after the Data is serialized.
        ///     <p />
        ///     WARNING:
        ///     <p />
        ///     Should be in sync with
        /// </remarks>
        public int TotalSize()
        {
            int total = 0;
            total += 4;
            // type
            if (classDefinition != null)
            {
                total += 4;
                // classDefinition-classId
                total += 4;
                // // classDefinition-factory-id
                total += 4;
                // classDefinition-version
                total += 4;
                // classDefinition-binary-length
                byte[] binary = ((BinaryClassDefinition) classDefinition).GetBinary();
                total += binary != null ? binary.Length : 0;
            }
            else
            {
                // classDefinition-binary
                total += 4;
            }
            // no-classId
            total += 4;
            // buffer-size
            total += BufferSize();
            // buffer
            total += 4;
            // partition-hash
            return total;
        }

        public int GetHeapCost()
        {
            int total = 0;
            total += 4;
            // type
            total += 4;
            // cd
            total += 16;
            // buffer array ref (12: array header, 4: length)
            total += BufferSize();
            // buffer itself
            total += 4;
            // partition-hash
            return total;
        }

        public override int GetHashCode()
        {
            //        int h = hash;
            //        if (h == 0 && bufferSize() > 0) {
            //            h = hash = calculateHash(buffer);
            //        }
            //        return h;
            return CalculateHash(buffer);
        }

        private static int CalculateHash(byte[] buffer)
        {
            if (buffer == null)
            {
                return 0;
            }
            // FNV (Fowler/Noll/Vo) Hash "1a"
            int prime = unchecked(0x01000193);
            var hash = unchecked((int) (0x811c9dc5));
            for (int i = buffer.Length - 1; i >= 0; i--)
            {
                hash = (hash ^ buffer[i])*prime;
            }
            return hash;
        }

        public int GetPartitionHash()
        {
            int ph = partitionHash;
            if (ph == 0 && BufferSize() > 0)
            {
                ph = partitionHash = GetHashCode();
            }
            return ph;
        }

        //public int GetType()
        //{
        //    return type;
        //}

        public IClassDefinition GetClassDefinition()
        {
            return classDefinition;
        }

        public byte[] GetBuffer()
        {
            return buffer;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Data))
            {
                return false;
            }
            if (this == obj)
            {
                return true;
            }
            var data = (Data) obj;
            return type == data.type && BufferSize() == data.BufferSize() && Equals(buffer, data.buffer);
        }

        // Same as Arrays.equals(byte[] a, byte[] a2) but loop order is reversed.
        private static bool Equals(byte[] data1, byte[] data2)
        {
            if (data1 == data2)
            {
                return true;
            }
            if (data1 == null || data2 == null)
            {
                return false;
            }
            int length = data1.Length;
            if (data2.Length != length)
            {
                return false;
            }
            for (int i = length - 1; i >= 0; i--)
            {
                if (data1[i] != data2[i])
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsPortable()
        {
            return SerializationConstants.ConstantTypePortable == type;
        }

        public bool IsDataSerializable()
        {
            return SerializationConstants.ConstantTypeData == type;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("Data{");
            sb.Append("type=").Append(type);
            sb.Append(", partitionHash=").Append(GetPartitionHash());
            sb.Append(", bufferSize=").Append(BufferSize());
            sb.Append(", totalSize=").Append(TotalSize());
            sb.Append('}');
            return sb.ToString();
        }
    }
}