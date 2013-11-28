using System.Collections;
using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Spi
{
	
	[System.Serializable]
	public sealed class SerializableCollection : IIdentifiedDataSerializable, IEnumerable<Data>
	{
		private ICollection<Data> collection;

		public SerializableCollection()
		{
		}

		public SerializableCollection(ICollection<Data> collection)
		{
			this.collection = collection;
		}

		public ICollection<Data> GetCollection()
		{
			return collection;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public void WriteData(IObjectDataOutput output)
		{
			if (collection == null)
			{
				output.WriteInt(-1);
				return;
			}
			output.WriteInt(collection.Count);
			foreach (Data data in collection)
			{
				data.WriteData(output);
			}
		}

		/// <exception cref="System.IO.IOException"></exception>
		public void ReadData(IObjectDataInput input)
		{
			int size = input.ReadInt();
			if (size == -1)
			{
				return;
			}
			collection = new List<Data>(size);
			for (int i = 0; i < size; i++)
			{
				collection.Add(IOUtil.ReadData(input));
			}
		}

		public int GetFactoryId()
		{
			return SpiDataSerializerHook.FId;
		}

		public int GetId()
		{
			return SpiDataSerializerHook.Collection;
		}

		public IEnumerator<Data> GetEnumerator()
		{
            return collection == null ? new List<Data>().GetEnumerator() : collection.GetEnumerator();
		}

		public int Size()
		{
			return collection == null ? 0 : collection.Count;
		}

	    IEnumerator IEnumerable.GetEnumerator()
	    {
	        return GetEnumerator();
	    }
	}
}
