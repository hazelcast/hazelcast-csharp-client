using System.Collections.Generic;

namespace Hazelcast.IO.Serialization
{
    /// <summary>ClassDefinition defines a class schema for Portable classes.</summary>
    /// <remarks>
    /// ClassDefinition defines a class schema for Portable classes. It allows to query field names, types, class id etc.
    /// It can be created manually using
    /// <see cref="ClassDefinitionBuilder"/>
    /// or ondemand during serialization phase.
    /// </remarks>
    /// <seealso cref="Portable"/>
    /// <seealso cref="ClassDefinitionBuilder"/>
    public interface IClassDefinition
    {
        /// <param name="name">name of the field</param>
        /// <returns>field definition by given name or null</returns>
        IFieldDefinition GetField(string name);

        /// <param name="fieldIndex">index of the field</param>
        /// <returns>field definition by given index</returns>
        /// <exception cref="System.IndexOutOfRangeException"/>
        IFieldDefinition GetField(int fieldIndex);

        /// <param name="fieldName">field name</param>
        /// <returns>true if this class definition contains a field named by given name</returns>
        bool HasField(string fieldName);

        /// <returns>all field names contained in this class definition</returns>
        ICollection<string> GetFieldNames();

        /// <param name="fieldName">name of the field</param>
        /// <returns>type of given field</returns>
        /// <exception cref="System.ArgumentException"/>
        FieldType GetFieldType(string fieldName);

        /// <param name="fieldName">name of the field</param>
        /// <returns>class id of given field</returns>
        /// <exception cref="System.ArgumentException"/>
        int GetFieldClassId(string fieldName);

        /// <returns>total field count</returns>
        int GetFieldCount();

        /// <returns>factory id</returns>
        int GetFactoryId();

        /// <returns>class id</returns>
        int GetClassId();

        /// <returns>version</returns>
        int GetVersion();
    }
}
