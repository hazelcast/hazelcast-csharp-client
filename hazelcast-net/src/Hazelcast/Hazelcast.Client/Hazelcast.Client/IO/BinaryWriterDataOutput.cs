using System;
using System.IO;
using System.Net;

namespace Hazelcast.IO
{
	public class BinaryWriterDataOutput : IDataOutput
	{
		
		private BinaryWriter writer;
		public BinaryWriterDataOutput (BinaryWriter writer)
		{
			this.writer = writer;
		}
			
		public void write(int b){
			writer.Write(b);
		}
		
		public void write(byte[] b){
			writer.Write(b);
		}
		
		public void write(byte[] b, int off, int len){}
		
		public void writeBoolean(bool v){
			writer.Write(v);
		}
		
		public void writeByte(int v){
			writer.Write((byte)v);
		}
		
		public void writeShort(int v){
			writer.Write(IPAddress.HostToNetworkOrder(v));
		}
		
		public void writeChar(int v){
			writer.Write((char)v);
		}
		
		public void writeInt(int v){
			writer.Write(IPAddress.HostToNetworkOrder(v));
		}
		
		public void writeLong(long v){
			writer.Write(IPAddress.HostToNetworkOrder(v));
		}
		
		public void writeFloat(float v){
			writer.Write(v);
		}
		
		public void writeDouble(double v){
			byte[] b = BitConverter.GetBytes(v);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(b);
			writer.Write(b);
		}
		
		public void writeBytes(String s){}
		
		public void writeChars(String s){
		}
		
		public void writeUTF(String s){
			Hazelcast.Client.IO.IOUtil.writeUTF(writer, s);
		}

	}
}

