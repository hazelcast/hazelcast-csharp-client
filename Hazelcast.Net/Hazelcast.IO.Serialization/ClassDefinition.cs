using System;
using System.Collections.Generic;
using System.Text;

namespace Hazelcast.IO.Serialization
{
    [Serializable]
    internal class ClassDefinition : BinaryClassDefinition
    {
        private readonly IList<IFieldDefinition> fieldDefinitions = new List<IFieldDefinition>();

        private readonly IDictionary<string, IFieldDefinition> fieldDefinitionsMap =
            new Dictionary<string, IFieldDefinition>();

        private readonly ICollection<IClassDefinition> nestedClassDefinitions = new HashSet<IClassDefinition>();

        public ClassDefinition()
        {
        }

        public ClassDefinition(int factoryId, int classId)
        {
            this.factoryId = factoryId;
            this.classId = classId;
        }

        public virtual void AddFieldDef(IFieldDefinition fd)
        {
            fieldDefinitions.Add(fd);
            fieldDefinitionsMap.Add(fd.GetName(), fd);
        }

        public virtual void AddClassDef(IClassDefinition cd)
        {
            nestedClassDefinitions.Add(cd);
        }

        public override IFieldDefinition Get(string name)
        {
            IFieldDefinition rtn;
            fieldDefinitionsMap.TryGetValue(name, out rtn);
            return rtn;
        }

        public override IFieldDefinition Get(int fieldIndex)
        {
            return fieldDefinitions[fieldIndex];
        }

        public virtual ICollection<IClassDefinition> GetNestedClassDefinitions()
        {
            return nestedClassDefinitions;
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
            IFieldDefinition fd = Get(fieldName);
            if (fd != null)
            {
                return fd.GetFieldType();
            }
            throw new ArgumentException();
        }

        public override int GetFieldClassId(string fieldName)
        {
            IFieldDefinition fd = Get(fieldName);
            if (fd != null)
            {
                return fd.GetClassId();
            }
            throw new ArgumentException();
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void WriteData(IObjectDataOutput dataOutput)
        {
            dataOutput.WriteInt(factoryId);
            dataOutput.WriteInt(classId);
            dataOutput.WriteInt(version);
            dataOutput.WriteInt(fieldDefinitions.Count);
            foreach (IFieldDefinition fieldDefinition in fieldDefinitions)
            {
                fieldDefinition.WriteData(dataOutput);
            }
            dataOutput.WriteInt(nestedClassDefinitions.Count);
            foreach (IClassDefinition classDefinition in nestedClassDefinitions)
            {
                classDefinition.WriteData(dataOutput);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public override void ReadData(IObjectDataInput input)
        {
            factoryId = input.ReadInt();
            classId = input.ReadInt();
            version = input.ReadInt();
            int size = input.ReadInt();
            for (int i = 0; i < size; i++)
            {
                var fieldDefinition = new FieldDefinition();
                fieldDefinition.ReadData(input);
                AddFieldDef(fieldDefinition);
            }
            size = input.ReadInt();
            for (int i_1 = 0; i_1 < size; i_1++)
            {
                var classDefinition = new ClassDefinition();
                classDefinition.ReadData(input);
                AddClassDef(classDefinition);
            }
        }

        public override int GetFieldCount()
        {
            return fieldDefinitions.Count;
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
            sb.Append("IClassDefinition");
            sb.Append("{factoryId=").Append(factoryId);
            sb.Append(", classId=").Append(classId);
            sb.Append(", version=").Append(version);
            sb.Append(", fieldDefinitions=").Append(fieldDefinitions);
            sb.Append('}');
            return sb.ToString();
        }
    }
}