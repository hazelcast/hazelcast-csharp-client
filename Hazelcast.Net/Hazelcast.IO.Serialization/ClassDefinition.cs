using System;
using System.Collections.Generic;
using System.Text;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    internal class ClassDefinition : IClassDefinition
    {
        private int factoryId;

        private int classId;

        private int version = -1;

        private readonly IDictionary<string, IFieldDefinition> fieldDefinitionsMap = new Dictionary<string, IFieldDefinition>();

        public ClassDefinition()
        {
        }

        public ClassDefinition(int factoryId, int classId, int version)
        {
            this.factoryId = factoryId;
            this.classId = classId;
            this.version = version;
        }

        internal virtual void AddFieldDef(FieldDefinition fd)
        {
            fieldDefinitionsMap[fd.GetName()] = fd;
        }

        public virtual IFieldDefinition GetField(string name)
        {
            IFieldDefinition val;
            return fieldDefinitionsMap.TryGetValue(name, out val) ? val : null;
        }

        public virtual IFieldDefinition GetField(int fieldIndex)
        {
            if (fieldIndex < 0 || fieldIndex >= fieldDefinitionsMap.Count)
            {
                throw new IndexOutOfRangeException("Index: " + fieldIndex + ", Size: " + fieldDefinitionsMap.Count);
            }
            foreach (IFieldDefinition fieldDefinition in fieldDefinitionsMap.Values)
            {
                if (fieldIndex == fieldDefinition.GetIndex())
                {
                    return fieldDefinition;
                }
            }
            throw new IndexOutOfRangeException("Index: " + fieldIndex + ", Size: " + fieldDefinitionsMap.Count);
        }

        public virtual bool HasField(string fieldName)
        {
            return fieldDefinitionsMap.ContainsKey(fieldName);
        }

        public virtual ICollection<string> GetFieldNames()
        {
            return new HashSet<string>(fieldDefinitionsMap.Keys);
        }

        public virtual FieldType GetFieldType(string fieldName)
        {
            IFieldDefinition fd = GetField(fieldName);
            if (fd != null)
            {
                return fd.GetFieldType();
            }
            throw new ArgumentException("Unknown field: " + fieldName);
        }

        public virtual int GetFieldClassId(string fieldName)
        {
            IFieldDefinition fd = GetField(fieldName);
            if (fd != null)
            {
                return fd.GetClassId();
            }
            throw new ArgumentException("Unknown field: " + fieldName);
        }

        internal virtual ICollection<IFieldDefinition> GetFieldDefinitions()
        {
            return fieldDefinitionsMap.Values;
        }

        public virtual int GetFieldCount()
        {
            return fieldDefinitionsMap.Count;
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
            ClassDefinition that = (ClassDefinition)o;
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
            foreach (IFieldDefinition fd in fieldDefinitionsMap.Values)
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
            result = 31 * result + version;
            return result;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ClassDefinition");
            sb.Append("{factoryId=").Append(factoryId);
            sb.Append(", classId=").Append(classId);
            sb.Append(", version=").Append(version);
            sb.Append(", fieldDefinitions=").Append(fieldDefinitionsMap.Values);
            sb.Append('}');
            return sb.ToString();
        }
    }
}
