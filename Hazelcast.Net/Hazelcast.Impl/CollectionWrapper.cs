/* using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using Hazelcast.Client.IO;

namespace Hazelcast.Client.Impl
{
	public class CollectionWrapper<K> : DataSerializable
	{
		List<K> entries;
		
		public CollectionWrapper ()
		{
			entries = new List<K>();
			
		}
		
		public void writeData(BinaryWriter writer){
			int size = (entries == null)?0: entries.Count;	
			writer.Write(IPAddress.HostToNetworkOrder(size) );
			if(size > 0){
				foreach(K entry in entries){
					if(entry is DataSerializable){
						((DataSerializable)entry).writeData(writer);		
					}
					else{
						//Throw exception
						throw new Exception("The entry is not serializable " + entry.GetType());
					}
			
				}
			}
		}

   		public void readData(BinaryReader reader){
			int size = IPAddress.NetworkToHostOrder (reader.ReadInt32 ());
			entries = new List<K>(size);
			List<byte[]> list = new List<byte[]>(size);
			for(int i=0;i<size;i++){
				int length = IPAddress.NetworkToHostOrder(reader.ReadInt32());
				byte[] bytes = reader.ReadBytes(length); //buffer of data
				reader.ReadInt32();//partition hash of  data
				list.Add(bytes);
			}
			for(int i=0;i<size;i++){
				K k = (K)IOUtil.toObject(list[i]);
				entries.Add(k);
			}
		}
		public List<K> getEntries(){
			return entries;
		}
	}
}

*/
