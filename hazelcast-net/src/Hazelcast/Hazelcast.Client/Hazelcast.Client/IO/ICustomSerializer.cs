using System;
using System.IO;
namespace Hazelcast.Client.IO
{
	public interface ICustomSerializer
	{
		void write(BinaryWriter writer, Object obj);

    	Object read(BinaryReader reader);
	}	
}

