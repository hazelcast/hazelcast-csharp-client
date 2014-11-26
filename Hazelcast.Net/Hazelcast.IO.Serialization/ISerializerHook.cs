using System;

namespace Hazelcast.IO.Serialization
{
	public interface ISerializerHook<T>
	{
		/// <summary>Returns the actual class type of the serialized object</summary>
		/// <returns>the serialized object type</returns>
		Type GetSerializationType();

		/// <summary>Creates a new serializer for the serialization type</summary>
		/// <returns>a new serializer instance</returns>
		ISerializer CreateSerializer();

		/// <summary>
		/// Defines if this serializer can be overridden by defining a custom
		/// serializer in the configurations (codebase or configuration file)
		/// </summary>
		/// <returns>if the serializer is overwritable</returns>
		bool IsOverwritable();
	}
}
