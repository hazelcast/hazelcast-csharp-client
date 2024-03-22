// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Concurrent;
using Hazelcast.Core;

namespace Hazelcast.Serialization
{
    internal sealed partial class PortableContext : IPortableContext
    {
        private readonly ConcurrentDictionary<int, ClassDefinitionContext> _classDefContextMap =
            new ConcurrentDictionary<int, ClassDefinitionContext>();

        private readonly SerializationService _serializationService;
        private readonly int _version;

        internal PortableContext(SerializationService serializationService, int version)
        {
            _serializationService = serializationService;
            _version = version;
        }

        public int GetClassVersion(int factoryId, int classId)
        {
            return GetClassDefContext(factoryId).GetClassVersion(classId);
        }

        public void SetClassVersion(int factoryId, int classId, int version)
        {
            GetClassDefContext(factoryId).SetClassVersion(classId, version);
        }

        public IClassDefinition LookupClassDefinition(int factoryId, int classId, int version)
        {
            return GetClassDefContext(factoryId).Lookup(classId, version);
        }

        public IClassDefinition LookupClassDefinition(IData data)
        {
            if (!data.IsPortable) throw new ArgumentException("Data is not Portable.", nameof(data));

            using var input = _serializationService.CreateObjectDataInput(data);

            var factoryId = input.ReadInt();
            var classId = input.ReadInt();
            var version = input.ReadInt();

            return LookupClassDefinition(factoryId, classId, version) ??
                   ReadClassDefinition(input, factoryId, classId, version);
        }

        public IClassDefinition RegisterClassDefinition(IClassDefinition cd)
        {
            return GetClassDefContext(cd.FactoryId).Register(cd);
        }

        /// <exception cref="System.IO.IOException" />
        public IClassDefinition LookupOrRegisterClassDefinition(IPortable p)
        {
            var portableVersion = PortableVersionHelper.GetVersion(p, _version);
            var cd = LookupClassDefinition(p.FactoryId, p.ClassId, portableVersion);
            if (cd == null)
            {
                var writer = new ClassDefinitionWriter(this, p.FactoryId, p.ClassId, portableVersion);
                p.WritePortable(writer);
                cd = writer.RegisterAndGet();
            }
            return cd;
        }

        public IFieldDefinition GetFieldDefinition(IClassDefinition classDef, string name)
        {
            var fd = classDef.GetField(name);
            if (fd == null)
            {
                var fieldNames = name.Split('.');
                if (fieldNames.Length > 1)
                {
                    var currentClassDef = classDef;
                    for (var i = 0; i < fieldNames.Length; i++)
                    {
                        name = fieldNames[i];
                        fd = currentClassDef.GetField(name);
                        if (i == fieldNames.Length - 1)
                        {
                            break;
                        }
                        if (fd == null)
                        {
                            throw new ArgumentException("Unknown field: " + name);
                        }
                        currentClassDef = LookupClassDefinition(fd.FactoryId, fd.ClassId,
                            fd.Version);
                        if (currentClassDef == null)
                        {
                            throw new ArgumentException("Not a registered Portable field: " + fd);
                        }
                    }
                }
            }
            return fd;
        }

        public int GetVersion()
        {
            return _version;
        }

        public Endianness Endianness => _serializationService.Endianness;

        /// <exception cref="System.IO.IOException" />
        internal IClassDefinition ReadClassDefinition(ObjectDataInput @in, int factoryId, int classId,
            int version)
        {
            var register = true;
            var builder = new ClassDefinitionBuilder(factoryId, classId, version);
            // final position after portable is read
            @in.ReadInt();
            // field count
            var fieldCount = @in.ReadInt();
            var offset = @in.Position;
            for (var i = 0; i < fieldCount; i++)
            {
                var pos = @in.ReadInt(offset + i* BytesExtensions.SizeOfInt);
                @in.Position = pos;
                var len = @in.ReadShort();
                var chars = new char[len];
                for (var k = 0; k < len; k++)
                {
                    chars[k] = (char) @in.ReadByte();
                }
                var type = (FieldType) (@in.ReadByte());
                var name = new string(chars);
                var fieldFactoryId = 0;
                var fieldClassId = 0;
                int fieldVersion = version;
                if (type == FieldType.Portable)
                {
                    // is null
                    if (@in.ReadBoolean())
                    {
                        register = false;
                    }
                    fieldFactoryId = @in.ReadInt();
                    fieldClassId = @in.ReadInt();
                    if (register)
                    {
                        fieldVersion = @in.ReadInt();
                        ReadClassDefinition(@in, fieldFactoryId, fieldClassId, fieldVersion);
                    }
                }
                else
                {
                    if (type == FieldType.PortableArray)
                    {
                        var k1 = @in.ReadInt();
                        fieldFactoryId = @in.ReadInt();
                        fieldClassId = @in.ReadInt();
                        if (k1 > 0)
                        {
                            var p = @in.ReadInt();
                            @in.Position = p;
                            fieldVersion = @in.ReadInt();
                            ReadClassDefinition(@in, fieldFactoryId, fieldClassId, fieldVersion);
                        }
                        else
                        {
                            register = false;
                        }
                    }
                }
                builder.AddField(new FieldDefinition(i, name, type, fieldFactoryId, fieldClassId, fieldVersion));
            }
            var classDefinition = builder.Build();
            if (register)
            {
                classDefinition = RegisterClassDefinition(classDefinition);
            }
            return classDefinition;
        }

        private ClassDefinitionContext GetClassDefContext(int factoryId)
        {
            return _classDefContextMap.GetOrAdd(factoryId,
                theFactoryId => new ClassDefinitionContext(this, theFactoryId));
        }
    }
}
