using System;
using Hazelcast.IO;
using System.Collections.Generic;

namespace Hazelcast.Impl.Base
{
	public class Values: Hazelcast.IO.DataSerializable
	{
		
		List<Data> lsValues;
		
		public Values ()
		{
		}
		
		public void writeData(IDataOutput dout){
			int size = (lsValues == null) ? 0 : lsValues.Count;
	        dout.writeInt(size);
	        if (size > 0) {
	            foreach (Data data in lsValues) {
	                data.writeData(dout);
	            }
	        }
		}

   		public void readData(IDataInput din){
			int size = din.readInt();
        	lsValues = new List<Data>(size);
	        for (int i = 0; i < size; i++) {
	            Data data = new Data();
	            data.readData(din);
	            lsValues.Add(data);
	        }
		}
	
		public System.Collections.Generic.ICollection<V> getCollection<V>(){
			List<V> list = new List<V>();
			foreach( Data d in lsValues){
				Object o = Hazelcast.Client.IO.IOUtil.toObject(d.Buffer);
				list.Add((V)o);
			}
			return list;
		}
	}
}

