using System;
using System.Collections.Generic;

namespace Hazelcast.IO.Serialization
{
    [Serializable]
    public abstract class BinaryClassDefinition : IClassDefinition
    {
        [NonSerialized] private byte[] binary;
        protected internal int classId;
        protected internal int factoryId;
        protected internal int version = -1;

        public int GetFactoryId()
        {
            return factoryId;
        }


        public int GetClassId()
        {
            return classId;
        }

        public int GetVersion()
        {
            return version;
        }

        public abstract void WriteData(IObjectDataOutput output);
        public abstract void ReadData(IObjectDataInput input);
        public string GetJavaClassName()
        {
            throw new NotImplementedException();
        }

        public abstract IFieldDefinition Get(string name);
        public abstract IFieldDefinition Get(int fieldIndex);
        public abstract bool HasField(string fieldName);
        public abstract ICollection<string> GetFieldNames();
        public abstract FieldType GetFieldType(string fieldName);
        public abstract int GetFieldClassId(string fieldName);
        public abstract int GetFieldCount();

        public byte[] GetBinary()
        {
            return binary;
        }

        internal void SetBinary(byte[] binary)
        {
            this.binary = binary;
        }
    }
}