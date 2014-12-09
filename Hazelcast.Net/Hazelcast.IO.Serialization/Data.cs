using System.Text;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.IO.Serialization
{
	internal sealed class Data : IMutableData
	{
		private int type = SerializationConstants.ConstantTypeNull;
		private byte[] header;
		private byte[] data;
		private int partitionHash;

		public Data(){}

		public Data(int type, byte[] data)
		{
			this.data = data;
			this.type = type;
		}

		public Data(int type, byte[] data, int partitionHash)
		{
			this.data = data;
			this.partitionHash = partitionHash;
			this.type = type;
		}

		public Data(int type, byte[] data, int partitionHash, byte[] header)
		{
			this.type = type;
			this.data = data;
			this.partitionHash = partitionHash;
			this.header = header;
		}

		public int DataSize()
		{
			return data != null ? data.Length : 0;
		}

		public int GetPartitionHash()
		{
			return partitionHash != 0 ? partitionHash : GetHashCode();
		}

		public bool HasPartitionHash()
		{
			return partitionHash != 0;
		}

		public int HeaderSize()
		{
			return header != null ? header.Length : 0;
		}

		public byte[] GetHeader()
		{
			return header;
		}

		public byte[] GetData()
		{
			return data;
		}

		public void SetData(byte[] array)
		{
			this.data = array;
		}

		public void SetPartitionHash(int partitionHash)
		{
			this.partitionHash = partitionHash;
		}

		public int GetType()
		{
			return type;
		}

		public void SetType(int type)
		{
			this.type = type;
		}

		public void SetHeader(byte[] header)
		{
			this.header = header;
		}

		public int ReadIntHeader(int offset, ByteOrder order)
		{
			return Bits.ReadInt(header, offset, order == ByteOrder.BigEndian);
		}

		public int GetHeapCost()
		{
			int integerSizeInBytes = 4;
			int arrayHeaderSizeInBytes = 16;
			int total = 0;
			// type
			total += integerSizeInBytes;
			if (header != null)
			{
				// metadata array ref (12: array header, 4: length)
				total += arrayHeaderSizeInBytes;
				total += header.Length;
			}
			else
			{
				total += integerSizeInBytes;
			}
			if (data != null)
			{
				// buffer array ref (12: array header, 4: length)
				total += arrayHeaderSizeInBytes;
				// data itself
				total += data.Length;
			}
			else
			{
				total += integerSizeInBytes;
			}
			// partition-hash
			total += integerSizeInBytes;
			return total;
		}

		public override bool Equals(object o)
		{
			if (this == o)
			{
				return true;
			}
			if (o == null)
			{
				return false;
			}
			if (!(o is Data))
			{
				return false;
			}
			Data data = (Data)o;
			if (GetType() != data.GetType())
			{
				return false;
			}
			int dataSize = DataSize();
			if (dataSize != data.DataSize())
			{
				return false;
			}
			return dataSize == 0 || Equals(this.data, data.GetData());
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

		public override int GetHashCode()
		{
			return HashUtil.MurmurHash3_x86_32(data, 0, DataSize());
		}

		public long Hash64()
		{
			return HashUtil.MurmurHash3_x64_64(data, 0, DataSize());
		}

		public bool IsPortable()
		{
			return SerializationConstants.ConstantTypePortable == type;
		}

		public override string ToString()
		{
			var sb = new StringBuilder("HeapData{");
			sb.Append("type=").Append(GetType());
			sb.Append(", hashCode=").Append(GetHashCode());
			sb.Append(", partitionHash=").Append(GetPartitionHash());
			sb.Append(", dataSize=").Append(DataSize());
			sb.Append(", heapCost=").Append(GetHeapCost());
			sb.Append('}');
			return sb.ToString();
		}
	}
}
