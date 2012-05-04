using System;

namespace Hazelcast.IO
{
	public interface ITypeConverter
	{
		string getJavaName(Type type);
		
		Type getType(String javaName);
	}
}

