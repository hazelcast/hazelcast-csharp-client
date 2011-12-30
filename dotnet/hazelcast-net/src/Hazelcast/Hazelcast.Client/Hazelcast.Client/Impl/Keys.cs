using System;
using Hazelcast.Client.IO;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Hazelcast.IO;

namespace Hazelcast.Impl
{
	public class Keys: DataSerializable
	{
		List<Data> keys;
		
		public Keys ()
		{
			keys = new List<Data>();
			
		}
		
		public void writeData(BinaryWriter writer){
			int size = (keys == null)?0: keys.Count;	
			writer.Write(IPAddress.HostToNetworkOrder(size) );
			if(size > 0){
				foreach(Data key in keys){
					key.writeData(writer);
				}
			}
		}

   		public void readData(BinaryReader reader){
			int size = IPAddress.NetworkToHostOrder (reader.ReadInt32 ());
			for(int i=0;i<size;i++){
				Data data = new Data();
				data.readData(reader);
				keys.Add(data);
			}
		}
		
		public void Add(Data key){
			keys.Add(key);
		}
		public int Count(){
			return keys.Count;
		}
		public Data Get(int index){
			return keys[index];
		}
	}
}

