using System;
using System.IO;
using Hazelcast.IO;
using Hazelcast.Client.IO;

namespace Hazelcast.Impl.Base
{
	public class KeyValue:DataSerializable
	{
		public Data key;
		public Data value;
		public KeyValue ()
		{
		}
		
		public void writeData(BinaryWriter writer){
			key.writeData(writer);
			bool gotValue = (value != null && value.size() > 0);
			writer.Write((bool)gotValue);
			if(gotValue){
				value.writeData(writer);
			}
		}

   		public void readData(BinaryReader reader){
			key = new Data();
			key.readData(reader);
			bool gotValue = reader.ReadBoolean();
			if(gotValue){
				value = new Data();
				value.readData(reader);
			}
		}
	}
}

