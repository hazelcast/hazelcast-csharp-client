using System;
using System.Collections.Generic;

namespace Hazelcast.IO.Serialization
{
    [Serializable]
    public abstract class BinaryClassDefinition : IClassDefinition
    {
        protected internal int classId;
        protected internal int factoryId;
        protected internal int version = -1;

        [NonSerialized] 
        private byte[] binary;

        public BinaryClassDefinition()
        {
        }

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
        public byte[] GetBinary()
        {
            return binary;
        }
        internal void SetBinary(byte[] binary)
        {
            this.binary = binary;
        }

        public abstract void WriteData(IObjectDataOutput output);
        public abstract void ReadData(IObjectDataInput input);
        public abstract IFieldDefinition Get(string name);
        public abstract IFieldDefinition Get(int fieldIndex);
        public abstract bool HasField(string fieldName);
        public abstract ICollection<string> GetFieldNames();
        public abstract FieldType GetFieldType(string fieldName);
        public abstract int GetFieldClassId(string fieldName);
        public abstract int GetFieldCount();


    }
}