using System;
using System.Collections.Generic;

namespace Hazelcast.IO.Serialization
{
    internal abstract class BinaryClassDefinition : IClassDefinition
    {
        [NonSerialized] 
        private byte[] binary;
        internal int classId;
        internal int factoryId;

        internal int version = -1;

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

        #region IClassDefinition

        public abstract IFieldDefinition GetField(string name);
        public abstract IFieldDefinition GetField(int fieldIndex);
        public abstract bool HasField(string fieldName);
        public abstract ICollection<string> GetFieldNames();
        public abstract FieldType GetFieldType(string fieldName);
        public abstract int GetFieldClassId(string fieldName);
        public abstract int GetFieldCount();

        #endregion
    }
}