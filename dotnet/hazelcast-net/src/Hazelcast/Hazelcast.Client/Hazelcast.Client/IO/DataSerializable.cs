using System;
using System.IO;
namespace Hazelcast.Client.IO
{
	public interface DataSerializable
	{
		 void writeData(BinaryWriter writer);

   		 void readData(BinaryReader reader);
	}
}

