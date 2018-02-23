// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

namespace Hazelcast.IO.Serialization
{
    internal sealed class ClassDefinitionWriter : IPortableWriter
    {
        private readonly ClassDefinitionBuilder _builder;
        private readonly IPortableContext _context;

        internal ClassDefinitionWriter(IPortableContext context, int factoryId, int classId, int version)
        {
            _context = context;
            _builder = new ClassDefinitionBuilder(factoryId, classId, version);
        }

        internal ClassDefinitionWriter(IPortableContext context, ClassDefinitionBuilder builder)
        {
            _context = context;
            _builder = builder;
        }

        public void WriteInt(string fieldName, int value)
        {
            _builder.AddIntField(fieldName);
        }

        public void WriteLong(string fieldName, long value)
        {
            _builder.AddLongField(fieldName);
        }

        public void WriteUTF(string fieldName, string str)
        {
            _builder.AddUTFField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteBoolean(string fieldName, bool value)
        {
            _builder.AddBooleanField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteByte(string fieldName, byte value)
        {
            _builder.AddByteField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteChar(string fieldName, int value)
        {
            _builder.AddCharField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteDouble(string fieldName, double value)
        {
            _builder.AddDoubleField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteFloat(string fieldName, float value)
        {
            _builder.AddFloatField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteShort(string fieldName, short value)
        {
            _builder.AddShortField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteBooleanArray(string fieldName, bool[] bools)
        {
            _builder.AddBooleanArrayField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteByteArray(string fieldName, byte[] bytes)
        {
            _builder.AddByteArrayField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteCharArray(string fieldName, char[] chars)
        {
            _builder.AddCharArrayField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteIntArray(string fieldName, int[] ints)
        {
            _builder.AddIntArrayField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteLongArray(string fieldName, long[] longs)
        {
            _builder.AddLongArrayField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteDoubleArray(string fieldName, double[] values)
        {
            _builder.AddDoubleArrayField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteFloatArray(string fieldName, float[] values)
        {
            _builder.AddFloatArrayField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteShortArray(string fieldName, short[] values)
        {
            _builder.AddShortArrayField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteUTFArray(string fieldName, string[] strings)
        {
            _builder.AddUTFArrayField(fieldName);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WritePortable(string fieldName, IPortable portable)
        {
            if (portable == null)
            {
                throw new HazelcastSerializationException("Cannot write null portable without explicitly "
                                                          + "registering class definition!");
            }
            var version = PortableVersionHelper.GetVersion(portable, _context.GetVersion());
            var nestedClassDef = CreateNestedClassDef(portable, new ClassDefinitionBuilder
                (portable.GetFactoryId(), portable.GetClassId(), version));
            _builder.AddPortableField(fieldName, nestedClassDef);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WriteNullPortable(string fieldName, int factoryId, int classId)
        {
            var nestedClassDef = _context.LookupClassDefinition(factoryId, classId
                , _context.GetVersion());
            if (nestedClassDef == null)
            {
                throw new HazelcastSerializationException("Cannot write null portable without explicitly "
                                                          + "registering class definition!");
            }
            _builder.AddPortableField(fieldName, nestedClassDef);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void WritePortableArray(string fieldName, IPortable[] portables)
        {
            if (portables == null || portables.Length == 0)
            {
                throw new HazelcastSerializationException("Cannot write null portable array without explicitly "
                                                          + "registering class definition!");
            }
            var p = portables[0];
            var classId = p.GetClassId();
            for (var i = 1; i < portables.Length; i++)
            {
                if (portables[i].GetClassId() != classId)
                {
                    throw new ArgumentException("Detected different class-ids in portable array!");
                }
            }
            var version = PortableVersionHelper.GetVersion(p, _context.GetVersion());
            var nestedClassDef = CreateNestedClassDef(p, new ClassDefinitionBuilder
                (p.GetFactoryId(), classId, version));
            _builder.AddPortableArrayField(fieldName, nestedClassDef);
        }

        public IObjectDataOutput GetRawDataOutput()
        {
            return new EmptyObjectDataOutput();
        }

        internal IClassDefinition RegisterAndGet()
        {
            var cd = _builder.Build();
            return _context.RegisterClassDefinition(cd);
        }

        /// <exception cref="System.IO.IOException"></exception>
        private IClassDefinition CreateNestedClassDef(IPortable portable, ClassDefinitionBuilder
            nestedBuilder)
        {
            var writer = new ClassDefinitionWriter(_context, nestedBuilder);
            portable.WritePortable(writer);
            return _context.RegisterClassDefinition(nestedBuilder.Build());
        }
    }
}