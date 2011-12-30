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
		
		public void writeData(BinaryWriter writer){
			int s= size();
			writer.Write(IPAddress.HostToNetworkOrder((int)s) );
			if(s > 0){
				writer.Write(Buffer);

				foreach(byte b in Buffer){
					Console.Write(b + ".");
				}
				Console.WriteLine("");
			}
			Console.WriteLine("Writing partition hash: " + PartitionHash);
			writer.Write(IPAddress.HostToNetworkOrder(PartitionHash));
			//Console.WriteLine("Hashcode was " + partitionHash);
		}

   		public void readData(BinaryReader reader){
			int size = IPAddress.NetworkToHostOrder (reader.ReadInt32 ());
			Buffer = reader.ReadBytes(size);
			PartitionHash = IPAddress.NetworkToHostOrder (reader.ReadInt32 ());
		}		
	}
}

