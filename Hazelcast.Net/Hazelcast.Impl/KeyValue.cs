using System;
using System.IO;
using Hazelcast.IO;

namespace Hazelcast.Impl.Base
{
	public class KeyValue:DataSerializable
	{
		public Data key;
		public Data value;
		
		public KeyValue()
		{
			
		}
		public KeyValue (Data key, Data value)
		{
			this.key = key;
			this.value = value;
		}
		
		public void writeData(IDataOutput dout){
			key.writeData(dout);
			bool gotValue = (value != null && value.size() > 0);
			dout.writeBoolean(gotValue);
			if(gotValue)
				value.writeData(dout);
		}

   		public void readData(IDataInput din){
			key = new Data();
			key.readData(din);
			bool gotValue = din.readBoolean();
			if(gotValue){
				value = new Data();
				value.readData(din);
			}
		}
		
		public String javaClassName(){
			return "com.hazelcast.impl.base.KeyValue";
		}
		
		
	}
}

