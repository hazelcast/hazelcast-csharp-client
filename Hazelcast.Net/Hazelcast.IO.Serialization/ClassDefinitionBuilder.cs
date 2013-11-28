using System;
using System.Collections.Generic;

namespace Hazelcast.IO.Serialization
{
    public sealed class ClassDefinitionBuilder
    {
        private readonly int classId;
        private readonly int factoryId;

        private readonly IList<IFieldDefinition> fieldDefinitions = new List<IFieldDefinition>();

        private readonly ICollection<IClassDefinition> nestedClassDefinitions = new HashSet<IClassDefinition>();

        private bool done;
        private int index;

        public ClassDefinitionBuilder(int factoryId, int classId)
        {
            this.factoryId = factoryId;
            this.classId = classId;
        }

        public ClassDefinitionBuilder AddIntField(string fieldName)
        {
            Check();
            fieldDefinitions.Add(new FieldDefinition(index++, fieldName, FieldType.Int));
            return this;
        }

        public ClassDefinitionBuilder AddLongField(string fieldName)
        {
            Check();
            fieldDefinitions.Add(new FieldDefinition(index++, fieldName, FieldType.Long));
            return this;
        }

        public ClassDefinitionBuilder AddUTFField(string fieldName)
        {
            Check();
            fieldDefinitions.Add(new FieldDefinition(index++, fieldName, FieldType.Utf));
            return this;
        }

        public ClassDefinitionBuilder AddBooleanField(string fieldName)
        {
            Check();
            fieldDefinitions.Add(new FieldDefinition(index++, fieldName, FieldType.Boolean));
            return this;
        }

        public ClassDefinitionBuilder AddByteField(string fieldName)
        {
            Check();
            fieldDefinitions.Add(new FieldDefinition(index++, fieldName, FieldType.Byte));
            return this;
        }

        public ClassDefinitionBuilder AddCharField(string fieldName)
        {
            Check();
            fieldDefinitions.Add(new FieldDefinition(index++, fieldName, FieldType.Char));
            return this;
        }

        public ClassDefinitionBuilder AddDoubleField(string fieldName)
        {
            Check();
            fieldDefinitions.Add(new FieldDefinition(index++, fieldName, FieldType.Double));
            return this;
        }

        public ClassDefinitionBuilder AddFloatField(string fieldName)
        {
            Check();
            fieldDefinitions.Add(new FieldDefinition(index++, fieldName, FieldType.Float));
            return this;
        }

        public ClassDefinitionBuilder AddShortField(string fieldName)
        {
            Check();
            fieldDefinitions.Add(new FieldDefinition(index++, fieldName, FieldType.Short));
            return this;
        }

        public ClassDefinitionBuilder AddByteArrayField(string fieldName)
        {
            Check();
            fieldDefinitions.Add(new FieldDefinition(index++, fieldName, FieldType.ByteArray));
            return this;
        }

        public ClassDefinitionBuilder AddCharArrayField(string fieldName)
        {
            Check();
            fieldDefinitions.Add(new FieldDefinition(index++, fieldName, FieldType.CharArray));
            return this;
        }

        public ClassDefinitionBuilder AddIntArrayField(string fieldName)
        {
            Check();
            fieldDefinitions.Add(new FieldDefinition(index++, fieldName, FieldType.IntArray));
            return this;
        }

        public ClassDefinitionBuilder AddLongArrayField(string fieldName)
        {
            Check();
            fieldDefinitions.Add(new FieldDefinition(index++, fieldName, FieldType.LongArray));
            return this;
        }

        public ClassDefinitionBuilder AddDoubleArrayField(string fieldName)
        {
            Check();
            fieldDefinitions.Add(new FieldDefinition(index++, fieldName, FieldType.DoubleArray));
            return this;
        }

        public ClassDefinitionBuilder AddFloatArrayField(string fieldName)
        {
            Check();
            fieldDefinitions.Add(new FieldDefinition(index++, fieldName, FieldType.FloatArray));
            return this;
        }

        public ClassDefinitionBuilder AddShortArrayField(string fieldName)
        {
            Check();
            fieldDefinitions.Add(new FieldDefinition(index++, fieldName, FieldType.ShortArray));
            return this;
        }

        public ClassDefinitionBuilder AddPortableField(string fieldName, IClassDefinition def)
        {
            Check();
            if (def.GetClassId() == Data.NoClassId)
            {
                throw new ArgumentException("IPortable class id cannot be zero!");
            }
            fieldDefinitions.Add(new FieldDefinition(index++, fieldName, FieldType.Portable, def.GetFactoryId(),
                def.GetClassId()));
            nestedClassDefinitions.Add(def);
            return this;
        }

        public ClassDefinitionBuilder AddPortableArrayField(string fieldName,
            IClassDefinition def)
        {
            Check();
            if (def.GetClassId() == Data.NoClassId)
            {
                throw new ArgumentException("IPortable class id cannot be zero!");
            }
            fieldDefinitions.Add(new FieldDefinition(index++, fieldName, FieldType.PortableArray, def.GetFactoryId(),
                def.GetClassId()));
            nestedClassDefinitions.Add(def);
            return this;
        }

        public IClassDefinition Build()
        {
            done = true;
            var cd = new ClassDefinition(factoryId, classId);
            foreach (IFieldDefinition fd in fieldDefinitions)
            {
                cd.AddFieldDef(fd);
            }
            foreach (IClassDefinition nestedCd in nestedClassDefinitions)
            {
                cd.AddClassDef(nestedCd);
            }
            return cd;
        }

        private void Check()
        {
            if (done)
            {
                throw new HazelcastSerializationException("IClassDefinition is already built for " + classId);
            }
        }
    }
}