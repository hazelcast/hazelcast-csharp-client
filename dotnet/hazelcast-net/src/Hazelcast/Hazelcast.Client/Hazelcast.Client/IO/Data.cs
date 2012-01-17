using System;
using System.IO;
using Hazelcast.Client.IO;
using System.Net;

namespace Hazelcast.IO
{
	public class Data: DataSerializable
	{
		private byte[] buffer = null;
		private int partitionHash = 27;

		public byte[] Buffer {
			get {
				return this.buffer;
			}
			set {
				buffer = value;
			}
		}

		public int PartitionHash {
			get {
				return this.partitionHash;
			}
			set {
				partitionHash = value;
			}
		}	
		
		public String javaClassName(){
			return null;
		}
		
		
		
		public Data ()
		{
		}
		public Data (byte[] bytes)
		{
			this.Buffer = bytes;
		}
		public int size() {
        	return (Buffer == null) ? 0 : Buffer.Length;
    	}
		
		public void writeData(IDataOutput dout){
			int s= size();
			dout.writeInt(s);
			if(s > 0){
				dout.write(Buffer);
			}
			dout.writeInt(PartitionHash);
		}

   		public void readData(IDataInput din){
			int size = din.readInt();
			Buffer = new byte[size];
			din.readFully(Buffer);
			PartitionHash = din.readInt();
		}		
	}
}

