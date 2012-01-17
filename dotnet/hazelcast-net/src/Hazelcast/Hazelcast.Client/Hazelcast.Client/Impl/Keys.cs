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
		
		public void writeData(IDataOutput dout){
			int size = (keys == null)?0: keys.Count;	
			dout.writeInt(size);
			if(size > 0){
				foreach(Data key in keys){
					key.writeData(dout);
				}
			}
		}

   		public void readData(IDataInput din){
			int size = din.readInt();
			for(int i=0;i<size;i++){
				Data data = new Data();
				data.readData(din);
				keys.Add(data);
			}
		}
		
		public String javaClassName(){
			return "com.hazelcast.impl.Keys";
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

