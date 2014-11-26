using System.Collections.Generic;

namespace Hazelcast.IO.Serialization
{
	internal interface IPortableHook
	{
		int GetFactoryId();

		IPortableFactory CreateFactory();

		ICollection<IClassDefinition> GetBuiltinDefinitions();
	}
}
