using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Hazelcast.IO.Serialization
{
    internal class BinaryClassDefinitionProxy : BinaryClassDefinition, IClassDefinition
    {
        public BinaryClassDefinitionProxy(int factoryId, int classId, int version, byte[] binary)
        {
            this.classId = classId;
            this.version = version;
            this.factoryId = factoryId;
            SetBinary(binary);
        }

        public BinaryClassDefinitionProxy(int factoryId, int classId, int version)
        {
            this.classId = classId;
            this.version = version;
            this.factoryId = factoryId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        public IClassDefinition ToReal(IPortableContext context) 
        {
            IClassDefinition cd = context.Lookup(factoryId, classId, version);
            return cd ?? context.CreateClassDefinition(factoryId, GetBinary());
        }

        public override void WriteData(IObjectDataOutput output)
        {
            throw new NotSupportedException();
        }

        public override void ReadData(IObjectDataInput input)
        {
            throw new NotSupportedException();
        }

        public override IFieldDefinition Get(string name)
        {
            throw new NotSupportedException();
        }

        public override IFieldDefinition Get(int fieldIndex)
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
    }
}
