using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using Hazelcast.Impl.Base;
using Hazelcast.Client.IO;

namespace Hazelcast.Impl.Base 
{
	public class Pairs: DataSerializable
	{
		public List<KeyValue> lsKeyValues = null;
		public Pairs ()
		{
		}
		
		public void writeData(BinaryWriter writer){
			int size = (lsKeyValues == null)?0: lsKeyValues.Count;	
			writer.Write(IPAddress.HostToNetworkOrder(size) );
			if(size > 0){
				foreach(KeyValue kv in lsKeyValues){
					kv.writeData(writer);
				}
			}
		}

   		public void readData(BinaryReader reader){
			int size = IPAddress.NetworkToHostOrder (reader.ReadInt32 ());
			if(lsKeyValues==null){
				lsKeyValues = new List<KeyValue>();
			}
			for(int i=0;i<size;i++){
				KeyValue kv = new KeyValue();
				kv.readData(reader);
				lsKeyValues.Add(kv);
			}
		}
	}
}

