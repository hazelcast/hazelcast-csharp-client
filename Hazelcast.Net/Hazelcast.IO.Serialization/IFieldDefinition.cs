namespace Hazelcast.IO.Serialization
{
	/// <summary>FieldDefinition defines name, type, index of a field</summary>
	public interface IFieldDefinition
	{
		/// <returns>field type</returns>
		FieldType GetFieldType();

		/// <returns>field name</returns>
		string GetName();

		/// <returns>field index</returns>
		int GetIndex();

		/// <returns>class id of this field's class</returns>
		int GetClassId();

		/// <returns>factory id of this field's class</returns>
		int GetFactoryId();
	}
}
