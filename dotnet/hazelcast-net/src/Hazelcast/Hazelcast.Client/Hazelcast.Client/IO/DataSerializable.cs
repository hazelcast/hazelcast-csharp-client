using System;
using System.IO;
namespace Hazelcast.IO
{
	public interface DataSerializable
	{
		 void writeData(IDataOutput dout);

   		 void readData(IDataInput din);
		
		 String javaClassName();
	}
}

