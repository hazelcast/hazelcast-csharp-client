using System.Collections;
using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Multimap
{
	
	public class PortableEntrySetResponse : IPortable
	{
        internal ICollection<KeyValuePair<Data,Data>> entrySet = null;

		public PortableEntrySetResponse()
		{
		}

        public PortableEntrySetResponse(ICollection<KeyValuePair<Data, Data>> entrySet)
		{
			this.entrySet = entrySet;
		}

        public virtual ICollection<KeyValuePair<Data, Data>> GetEntrySet()
		{
			return entrySet;
		}

		public virtual int GetFactoryId()
		{
			return MultiMapPortableHook.FId;
		}

		public virtual int GetClassId()
		{
			return MultiMapPortableHook.EntrySetResponse;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void WritePortable(IPortableWriter writer)
		{
			writer.WriteInt("s", entrySet.Count);
			IObjectDataOutput output = writer.GetRawDataOutput();
            foreach (KeyValuePair<Data, Data> entry in entrySet)
			{
				Data key = entry.Key;
				Data value = entry.Value;
				key.WriteData(output);
				value.WriteData(output);
			}
		}

		/// <exception cref="System.IO.IOException"></exception>
		public virtual void ReadPortable(IPortableReader reader)
		{
			int size = reader.ReadInt("s");
			IObjectDataInput input = reader.GetRawDataInput();
            entrySet = new HashSet<KeyValuePair<Data, Data>>();
			for (int i = 0; i < size; i++)
			{
				Data key = new Data();
				Data value = new Data();
				key.ReadData(input);
				value.ReadData(input);
                entrySet.Add(new KeyValuePair<Data, Data>(key, value));
			}
		}
	}
}
