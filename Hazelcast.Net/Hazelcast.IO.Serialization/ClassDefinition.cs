using System;
using System.Collections.Generic;
using System.Text;

namespace Hazelcast.IO.Serialization
{
    internal class ClassDefinition : BinaryClassDefinition
    {
        private readonly IList<IFieldDefinition> fieldDefinitions = new List<IFieldDefinition>();

        private readonly IDictionary<string, IFieldDefinition> fieldDefinitionsMap = new
            Dictionary<string, IFieldDefinition>();

        public ClassDefinition()
        {
        }

        public ClassDefinition(int factoryId, int classId, int version)
        {
            this.factoryId = factoryId;
            this.classId = classId;
            this.version = version;
        }

        public override IFieldDefinition GetField(string name)
        {
            IFieldDefinition returnedVal;
            fieldDefinitionsMap.TryGetValue(name, out returnedVal);
            return returnedVal;
        }

        public override IFieldDefinition GetField(int fieldIndex)
        {
            return fieldDefinitions[fieldIndex];
        }

        public override bool HasField(string fieldName)
        {
            return fieldDefinitionsMap.ContainsKey(fieldName);
        }

        public override ICollection<string> GetFieldNames()
        {
            return new HashSet<string>(fieldDefinitionsMap.Keys);
        }

        public override FieldType GetFieldType(string fieldName)
        {
            IFieldDefinition fd = GetField(fieldName);
            if (fd != null)
            {
                return fd.GetFieldType();
            }
            throw new ArgumentException("Unknown field: " + fieldName);
        }

        public override int GetFieldClassId(string fieldName)
        {
            IFieldDefinition fd = GetField(fieldName);
            if (fd != null)
            {
                return fd.GetClassId();
            }
            throw new ArgumentException("Unknown field: " + fieldName);
        }

        public override int GetFieldCount()
        {
            return fieldDefinitions.Count;
        }

        internal virtual void AddFieldDef(FieldDefinition fd)
        {
            fieldDefinitions.Add(fd);
            fieldDefinitionsMap.Add(fd.GetName(), fd);
        }

        internal virtual ICollection<IFieldDefinition> GetFieldDefinitions()
        {
            return fieldDefinitions;
        }

        internal virtual void SetVersionIfNotSet(int version)
        {
            if (GetVersion() < 0)
            {
                this.version = version;
            }
        }

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
            var that = (ClassDefinition) o;
            if (classId != that.classId)
            {
                return false;
            }
            if (version != that.version)
            {
                return false;
            }
            if (GetFieldCount() != that.GetFieldCount())
            {
                return false;
            }
            foreach (IFieldDefinition fd in fieldDefinitions)
            {
                IFieldDefinition fd2 = that.GetField(fd.GetName());
                if (fd2 == null)
                {
                    return false;
                }
                if (!fd.Equals(fd2))
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            int result = classId;
            result = 31*result + version;
            return result;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ClassDefinition");
            sb.Append("{factoryId=").Append(factoryId);
            sb.Append(", classId=").Append(classId);
            sb.Append(", version=").Append(version);
            sb.Append(", fieldDefinitions=").Append(fieldDefinitions);
            sb.Append('}');
            return sb.ToString();
        }
    }
}