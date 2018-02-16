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
using System.Collections.Concurrent;
using Hazelcast.Core;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    internal sealed class PortableContext : IPortableContext
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

        /// <exception cref="System.IO.IOException" />
        public IClassDefinition LookupClassDefinition(IData data)
        {
            if (!data.IsPortable())
            {
                throw new ArgumentException("Data is not Portable!");
            }
            var @in = _serializationService.CreateObjectDataInput(data);
            var factoryId = @in.ReadInt();
            var classId = @in.ReadInt();
            var version = @in.ReadInt();
            var classDefinition = LookupClassDefinition(factoryId, classId, version);
            if (classDefinition == null)
            {
                classDefinition = ReadClassDefinition(@in, factoryId, classId, version);
            }
            return classDefinition;
        }

        public IClassDefinition RegisterClassDefinition(IClassDefinition cd)
        {
            return GetClassDefContext(cd.GetFactoryId()).Register(cd);
        }

        /// <exception cref="System.IO.IOException" />
        public IClassDefinition LookupOrRegisterClassDefinition(IPortable p)
        {
            var portableVersion = PortableVersionHelper.GetVersion(p, _version);
            var cd = LookupClassDefinition(p.GetFactoryId(), p.GetClassId(), portableVersion);
            if (cd == null)
            {
                var writer = new ClassDefinitionWriter(this, p.GetFactoryId(), p.GetClassId(), portableVersion);
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
                        currentClassDef = LookupClassDefinition(fd.GetFactoryId(), fd.GetClassId(),
                            currentClassDef.GetVersion());
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

        public IManagedContext GetManagedContext()
        {
            return _serializationService.GetManagedContext();
        }

        public ByteOrder GetByteOrder()
        {
            return _serializationService.GetByteOrder();
        }

        /// <exception cref="System.IO.IOException" />
        internal IClassDefinition ReadClassDefinition(IBufferObjectDataInput @in, int factoryId, int classId,
            int version)
        {
            var register = true;
            var builder = new ClassDefinitionBuilder(factoryId, classId, version);
            // final position after portable is read
            @in.ReadInt();
            // field count
            var fieldCount = @in.ReadInt();
            var offset = @in.Position();
            for (var i = 0; i < fieldCount; i++)
            {
                var pos = @in.ReadInt(offset + i*Bits.IntSizeInBytes);
                @in.Position(pos);
                var len = @in.ReadShort();
                var chars = new char[len];
                for (var k = 0; k < len; k++)
                {
                    chars[k] = (char) @in.ReadUnsignedByte();
                }
                var type = (FieldType) (@in.ReadByte());
                var name = new string(chars);
                var fieldFactoryId = 0;
                var fieldClassId = 0;
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
                        var fieldVersion = @in.ReadInt();
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
                            @in.Position(p);
                            var fieldVersion = @in.ReadInt();
                            ReadClassDefinition(@in, fieldFactoryId, fieldClassId, fieldVersion);
                        }
                        else
                        {
                            register = false;
                        }
                    }
                }
                builder.AddField(new FieldDefinition(i, name, type, fieldFactoryId, fieldClassId));
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

        private sealed class ClassDefinitionContext
        {
            private readonly ConcurrentDictionary<int, int> _currentClassVersions = new ConcurrentDictionary<int, int>();
            private readonly int _factoryId;
            private readonly PortableContext _portableContext;

            private readonly ConcurrentDictionary<long, IClassDefinition> _versionedDefinitions =
                new ConcurrentDictionary<long, IClassDefinition>();

            internal ClassDefinitionContext(PortableContext portableContext, int factoryId)
            {
                _portableContext = portableContext;
                _factoryId = factoryId;
            }

            internal int GetClassVersion(int classId)
            {
                int version;
                var hasValue = _currentClassVersions.TryGetValue(classId, out version);
                return hasValue ? version : -1;
            }

            internal IClassDefinition Lookup(int classId, int version)
            {
                var versionedClassId = Bits.CombineToLong(classId, version);
                IClassDefinition cd;
                _versionedDefinitions.TryGetValue(versionedClassId, out cd);
                return cd;
            }

            internal IClassDefinition Register(IClassDefinition cd)
            {
                if (cd == null)
                {
                    return null;
                }
                if (cd.GetFactoryId() != _factoryId)
                {
                    throw new HazelcastSerializationException("Invalid factory-id! " + _factoryId + " -> " + cd);
                }
                if (cd is ClassDefinition)
                {
                    var cdImpl = (ClassDefinition) cd;
                    cdImpl.SetVersionIfNotSet(_portableContext.GetVersion());
                }
                var versionedClassId = Bits.CombineToLong(cd.GetClassId(), cd.GetVersion());
                var currentCd = _versionedDefinitions.GetOrAdd(versionedClassId, cd);
                if (Equals(currentCd, cd))
                {
                    return cd;
                }
                if (currentCd is ClassDefinition)
                {
                    if (!currentCd.Equals(cd))
                    {
                        throw new HazelcastSerializationException(
                            "Incompatible class-definitions with same class-id: " + cd + " VS " + currentCd);
                    }
                    return currentCd;
                }
                _versionedDefinitions.AddOrUpdate(versionedClassId, cd, (key, oldValue) => cd);
                return cd;
            }

            internal void SetClassVersion(int classId, int version)
            {
                var hasAdded = _currentClassVersions.TryAdd(classId, version);
                if (!hasAdded && _currentClassVersions[classId] != version)
                {
                    throw new ArgumentException("Class-id: " + classId + " is already registered!");
                }
            }
        }
    }
}