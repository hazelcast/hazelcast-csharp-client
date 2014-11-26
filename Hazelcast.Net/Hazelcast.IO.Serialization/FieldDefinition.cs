using System.Text;

namespace Hazelcast.IO.Serialization
{
    internal class FieldDefinition : IFieldDefinition
    {
        internal int index;
        internal string fieldName;
        internal FieldType type;
        internal int classId;
        internal int factoryId;

        public FieldDefinition(){}

        internal FieldDefinition(int index, string fieldName, FieldType type): 
            this(index, fieldName, type, 0, 0){}

        internal FieldDefinition(int index, string fieldName, FieldType type, int factoryId, int classId)
        {
            this.classId = classId;
            this.type = type;
            this.fieldName = fieldName;
            this.index = index;
            this.factoryId = factoryId;
        }

        public virtual FieldType GetFieldType()
        {
            return type;
        }

        public virtual string GetName()
        {
            return fieldName;
        }

        public virtual int GetIndex()
        {
            return index;
        }

        public virtual int GetFactoryId()
        {
            return factoryId;
        }

        public virtual int GetClassId()
        {
            return classId;
        }

        internal virtual bool IsPortable()
        {
            return type == FieldType.Portable || type == FieldType.PortableArray;
        }

        protected bool Equals(FieldDefinition other)
        {
            return string.Equals(fieldName, other.fieldName) && type == other.type && classId == other.classId && factoryId == other.factoryId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FieldDefinition) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (fieldName != null ? fieldName.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (int) type;
                hashCode = (hashCode*397) ^ classId;
                hashCode = (hashCode*397) ^ factoryId;
                return hashCode;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder("FieldDefinitionImpl{");
            sb.Append("index=").Append(index);
            sb.Append(", fieldName='").Append(fieldName).Append('\'');
            sb.Append(", type=").Append(type);
            sb.Append(", classId=").Append(classId);
            sb.Append(", factoryId=").Append(factoryId);
            sb.Append('}');
            return sb.ToString();
        }
    }
}
