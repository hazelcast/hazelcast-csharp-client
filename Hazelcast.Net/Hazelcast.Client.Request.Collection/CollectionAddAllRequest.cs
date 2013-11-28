using System.Collections.Generic;
using Hazelcast.Client.Request.Collection;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;
using Hazelcast.Serialization.Hook;


namespace Hazelcast.Client.Request.Collection
{
	public class CollectionAddAllRequest : CollectionRequest
	{
		protected internal IList<Data> valueList;

		public CollectionAddAllRequest()
		{
		}

		public CollectionAddAllRequest(string name, IList<Data> valueList) : base(name)
		{
			this.valueList = valueList;
		}

		public override int GetClassId()
		{
			return CollectionPortableHook.CollectionAddAll;
		}

		/// <exception cref="System.IO.IOException"></exception>
		public override void WritePortable(IPortableWriter writer)
		{
			base.WritePortable(writer);
			IObjectDataOutput output = writer.GetRawDataOutput();
			output.WriteInt(valueList.Count);
			foreach (Data value in valueList)
			{
				value.WriteData(output);
			}
		}

		/// <exception cref="System.IO.IOException"></exception>
		public override void ReadPortable(IPortableReader reader)
		{
			base.ReadPortable(reader);
			IObjectDataInput input = reader.GetRawDataInput();
			int size = input.ReadInt();
			valueList = new List<Data>(size);
			for (int i = 0; i < size; i++)
			{
				Data value = new Data();
				value.ReadData(input);
				valueList.Add(value);
			}
		}
	}
}
