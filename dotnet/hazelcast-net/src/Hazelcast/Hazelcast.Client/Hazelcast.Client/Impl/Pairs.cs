using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using Hazelcast.Impl.Base;
using Hazelcast.IO;

namespace Hazelcast.Impl.Base 
{
	public class Pairs: DataSerializable
	{
		public List<KeyValue> lsKeyValues = new List<KeyValue>();
		public Pairs ()
		{
		}
		
		public void writeData(IDataOutput dout){
			int size = (lsKeyValues == null)?0: lsKeyValues.Count;	
			dout.writeInt(size);
			if(size > 0){
				foreach(KeyValue kv in lsKeyValues){
					kv.writeData(dout);
				}
			}
		}

   		public void readData(IDataInput din){
			int size = din.readInt();
			if(lsKeyValues==null){
				lsKeyValues = new List<KeyValue>();
			}
			for(int i=0;i<size;i++){
				KeyValue kv = new KeyValue();
				kv.readData(din);
				lsKeyValues.Add(kv);
			}
		}
		
		public void addKeyValue(KeyValue kv){
			lsKeyValues.Add(kv);	
		}
	}
}

