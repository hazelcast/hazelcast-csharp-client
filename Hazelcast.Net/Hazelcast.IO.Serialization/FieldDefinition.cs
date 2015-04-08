using System.Text;

namespace Hazelcast.IO.Serialization
{
    internal class FieldDefinition : IFieldDefinition
    {
        internal int classId;
        internal int factoryId;
        internal string fieldName;
        internal int index;
        internal FieldType type;

        internal FieldDefinition()
        {
        }

        internal FieldDefinition(int index, string fieldName, FieldType type)
            : this(index, fieldName, type, 0, 0)
        {
        }

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

        //CHECKSTYLE:OFF
        //Generated equals method has too high NPath Complexity
        public override bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            if (o == null || GetType() != o.GetType())
            {
                return false;
            }
            var that = (FieldDefinition) o;
            if (classId != that.classId)
            {
                return false;
            }
            if (factoryId != that.factoryId)
            {
                return false;
            }
            if (fieldName != null ? !fieldName.Equals(that.fieldName) : that.fieldName != null)
            {
                return false;
            }
            if (type != that.type)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            var result = fieldName != null ? fieldName.GetHashCode() : 0;
            result = 31*result + (type != null ? type.GetHashCode() : 0);
            result = 31*result + classId;
            result = 31*result + factoryId;
            return result;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("FieldDefinition{");
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