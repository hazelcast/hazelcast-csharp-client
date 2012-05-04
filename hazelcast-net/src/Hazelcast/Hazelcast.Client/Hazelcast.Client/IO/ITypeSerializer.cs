using System;
using System.IO;
namespace Hazelcast.Client.IO
{
	public interface ITypeSerializer: IComparable{
		int priority();
	    bool isSuitable(Object obj);
	    byte getTypeId();
		void write(BinaryWriter writer, Object obj);
		Object read(BinaryReader reader);
	}
	
	public interface ITypeSerializer<T>:ITypeSerializer
	{
		void write(BinaryWriter writer, T obj);
		T read(BinaryReader reader);
	}
}

