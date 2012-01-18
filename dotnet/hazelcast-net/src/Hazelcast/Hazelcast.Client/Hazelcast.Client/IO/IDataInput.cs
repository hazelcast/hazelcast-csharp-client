using System;

namespace Hazelcast.IO
{
	interface IDataInput
	{
		void readFully(byte[] b);
		
		void readFully(byte[] b, int off, int len);
		
    	int skipBytes(int n);

    	bool readBoolean();
			
    	byte readByte();
			
    	int readUnsignedByte();
			
    	short readShort();
			
    	int readUnsignedShort();
			
    	char readChar();
			
		int readInt();
			
    	long readLong();
			
    	float readFloat();
			
    	double readDouble();
			
    	String readLine();
			
    	String readUTF();
	}    
}

