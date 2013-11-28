using System.Collections.Generic;
using Hazelcast.Client.Request.Base;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Map
{
	public class MapGetAllRequest : IPortable, IRetryableRequest
	{
		protected internal string name;

		private ICollection<Data> keys = new HashSet<Data>();

		public MapGetAllRequest()
		{
		}

		public MapGetAllRequest(string name, ICollection<Data> keys)
		{
			this.name = name;
			this.keys = keys;
		}

		public virtual int GetFactoryId()
		{
			return MapPortableHook.FId;
		}

		public virtual int GetClassId()
		{
			return MapPortableHook.GetAll;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void WritePortable(IPortableWriter writer)
		{
			writer.WriteUTF("n", name);
			writer.WriteInt("size", keys.Count);
            if (keys.Count >0)
			{
				IObjectDataOutput output = writer.GetRawDataOutput();
				foreach (Data key in keys)
				{
					key.WriteData(output);
				}
			}
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void ReadPortable(IPortableReader reader)
		{
			name = reader.ReadUTF("n");
			int size = reader.ReadInt("size");
			if (size > 0)
			{
				IObjectDataInput input = reader.GetRawDataInput();
				for (int i = 0; i < size; i++)
				{
					Data key = new Data();
					key.ReadData(input);
					keys.Add(key);
				}
			}
		}
	}
}
