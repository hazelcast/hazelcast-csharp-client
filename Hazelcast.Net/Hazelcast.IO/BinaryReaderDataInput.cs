using System;
using System.IO;
using System.Net;

namespace Hazelcast.IO
{
	public class BinaryReaderDataInput : IDataInput
	{
		private BinaryReader reader;
		public BinaryReaderDataInput (BinaryReader reader)
		{
			this.reader = reader;
		}
		
		public void readFully(byte[] b){
			reader.Read(b, 0, b.Length);	
		}
	    public void readFully(byte[] b, int off, int len){
			reader.Read(b, off, len);	
		}

    	public int skipBytes(int n){
			byte[] b = reader.ReadBytes(n);
			return b.Length;
		}

    	public bool readBoolean(){
			return reader.ReadBoolean();
		}

    	public byte readByte(){
			return reader.ReadByte();
		}

    	public int readUnsignedByte(){
			throw new Exception("Not supported");	
		}

    	public short readShort(){
			return reader.ReadInt16();
		}
 
    	public int readUnsignedShort(){
			return reader.ReadUInt16();
		}

    	public char readChar(){
			return reader.ReadChar();	
		}

		public int readInt(){
			return IPAddress.NetworkToHostOrder (reader.ReadInt32 ());
		}

		//IPAddress.NetworkToHostOrder(reader.ReadInt64());
    	public long readLong(){
			return IPAddress.NetworkToHostOrder(reader.ReadInt64());
		}

    	public float readFloat(){
			return reader.ReadSingle();
		}
		
    	public double readDouble(){
			return reader.ReadDouble();
		}

    	public String readLine(){
			throw new Exception("Not supported!");
		}

    	public String readUTF(){
			return Hazelcast.Client.IO.IOUtil.readUTF(reader);
		}		
	}
}

