using System;

namespace Hazelcast.IO.Serialization
{
    internal sealed class ClassDefinitionWriter : IPortableWriter
    {
        private readonly ClassDefinitionBuilder builder;
        private readonly ISerializationContext context;

        internal ClassDefinitionWriter(ISerializationContext context, int factoryId, int classId)
        {
            this.context = context;
            builder = new ClassDefinitionBuilder(factoryId, classId);
        }

        internal ClassDefinitionWriter(ISerializationContext context, ClassDefinitionBuilder builder)
        {
            this.context = context;
            this.builder = builder;
        }

        public int GetVersion()
        {
            return context.GetVersion();
        }

        public void WriteInt(string fieldName, int value)
        {
            builder.AddIntField(fieldName);
        }

        public void WriteLong(string fieldName, long value)
        {
            builder.AddLongField(fieldName);
        }

        public void WriteUTF(string fieldName, string str)
        {
            builder.AddUTFField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteBoolean(string fieldName, bool value)
        {
            builder.AddBooleanField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteByte(string fieldName, byte value)
        {
            builder.AddByteField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteChar(string fieldName, int value)
        {
            builder.AddCharField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteDouble(string fieldName, double value)
        {
            builder.AddDoubleField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteFloat(string fieldName, float value)
        {
            builder.AddFloatField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteShort(string fieldName, short value)
        {
            builder.AddShortField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteByteArray(string fieldName, byte[] bytes)
        {
            builder.AddByteArrayField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteCharArray(string fieldName, char[] chars)
        {
            builder.AddCharArrayField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteIntArray(string fieldName, int[] ints)
        {
            builder.AddIntArrayField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteLongArray(string fieldName, long[] longs)
        {
            builder.AddLongArrayField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteDoubleArray(string fieldName, double[] values)
        {
            builder.AddDoubleArrayField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteFloatArray(string fieldName, float[] values)
        {
            builder.AddFloatArrayField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteShortArray(string fieldName, short[] values)
        {
            builder.AddShortArrayField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WritePortable(string fieldName, IPortable portable)
        {
            if (portable == null)
            {
                throw new HazelcastSerializationException("Cannot write null portable without explicitly " +
                                                          "registering class definition!");
            }
            WritePortable(fieldName, portable.GetFactoryId(), portable.GetClassId(), portable);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteNullPortable(string fieldName, int factoryId, int classId)
        {
            WritePortable(fieldName, factoryId, classId, null);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WritePortableArray(string fieldName, IPortable[] portables)
        {
            if (portables == null || portables.Length == 0)
            {
                throw new HazelcastSerializationException("Cannot write null portable array without explicitly " +
                                                          "registering class definition!");
            }
            IPortable p = portables[0];
            int classId = p.GetClassId();
            for (int i = 1; i < portables.Length; i++)
            {
                if (portables[i].GetClassId() != classId)
                {
                    throw new ArgumentException("Detected different class-ids in portable array!");
                }
            }
            IClassDefinition nestedClassDef = CreateNestedClassDef(p,
                new ClassDefinitionBuilder(p.GetFactoryId(), classId));
            builder.AddPortableArrayField(fieldName, nestedClassDef);
        }

        public IObjectDataOutput GetRawDataOutput()
        {
            return new EmptyObjectDataOutput();
        }

        /// <exception cref="System.IO.IOException"></exception>
        private void WritePortable(string fieldName, int factoryId, int classId, IPortable portable)
        {
            IClassDefinition nestedClassDef;
            if (portable != null)
            {
                nestedClassDef = CreateNestedClassDef(portable, new ClassDefinitionBuilder(factoryId, classId));
            }
            else
            {
                nestedClassDef = context.Lookup(factoryId, classId);
                if (nestedClassDef == null)
                {
                    throw new HazelcastSerializationException("Cannot write null portable without explicitly " +
                                                              "registering class definition!");
                }
            }
            builder.AddPortableField(fieldName, nestedClassDef);
        }

        /// <exception cref="System.IO.IOException"></exception>
        private IClassDefinition CreateNestedClassDef(IPortable portable, ClassDefinitionBuilder nestedBuilder)
        {
            var nestedWriter = new ClassDefinitionWriter(context, nestedBuilder);
            portable.WritePortable(nestedWriter);
            return context.RegisterClassDefinition(nestedBuilder.Build());
        }

        internal IClassDefinition RegisterAndGet()
        {
            IClassDefinition cd = builder.Build();
            return context.RegisterClassDefinition(cd);
        }
    }
}