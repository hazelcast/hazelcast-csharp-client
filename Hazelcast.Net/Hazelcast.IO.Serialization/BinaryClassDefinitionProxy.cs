using System;
using System.Collections.Generic;

namespace Hazelcast.IO.Serialization
{
    internal sealed class BinaryClassDefinitionProxy : BinaryClassDefinition, IClassDefinition
    {
        public BinaryClassDefinitionProxy(int factoryId, int classId, int version)
        {
            this.classId = classId;
            this.version = version;
            this.factoryId = factoryId;
        }

        public BinaryClassDefinitionProxy(int factoryId, int classId, int version, byte[] binary)
        {
            this.classId = classId;
            this.version = version;
            this.factoryId = factoryId;
            SetBinary(binary);
        }

        public override IFieldDefinition GetField(string name)
        {
            throw new NotSupportedException();
        }

        public override IFieldDefinition GetField(int fieldIndex)
        {
            throw new NotSupportedException();
        }

        public override bool HasField(string fieldName)
        {
            throw new NotSupportedException();
        }

        public override ICollection<string> GetFieldNames()
        {
            throw new NotSupportedException();
        }

        public override FieldType GetFieldType(string fieldName)
        {
            throw new NotSupportedException();
        }

        public override int GetFieldClassId(string fieldName)
        {
            throw new NotSupportedException();
        }

        public override int GetFieldCount()
        {
            throw new NotSupportedException();
        }

        public int GetFieldVersion(string fieldName)
        {
            throw new NotSupportedException();
        }
    }
}