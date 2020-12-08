// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Collections.Generic;
using System.Linq;

namespace Hazelcast.Serialization
{
    internal sealed class PortableSerializer : IStreamSerializer<IPortable>
    {
        private readonly PortableContext _context;
        private readonly IDictionary<int, IPortableFactory> _factories = new Dictionary<int, IPortableFactory>();

        internal PortableSerializer(PortableContext context,
            IEnumerable<KeyValuePair<int, IPortableFactory>> portableFactories)
        {
            _context = context;
            _factories = _factories.Union(portableFactories).ToDictionary(x => x.Key, x => x.Value);
        }

        public int TypeId => SerializationConstants.ConstantTypePortable;

        /// <exception cref="System.IO.IOException"></exception>
        public void Write(IObjectDataOutput output, IPortable p)
        {
            if (!(output is ObjectDataOutput))
            {
                throw new ArgumentException("ObjectDataOutput must be instance of BufferObjectDataOutput!");
            }
            if (p.ClassId == 0)
            {
                throw new ArgumentException("Portable class id cannot be zero!");
            }
            output.WriteInt(p.FactoryId);
            output.WriteInt(p.ClassId);
            WriteInternal((ObjectDataOutput) output, p);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public IPortable Read(IObjectDataInput input)
        {
            if (!(input is ObjectDataInput))
            {
                throw new ArgumentException("ObjectDataInput must be instance of BufferObjectDataInput!");
            }
            var factoryId = input.ReadInt();
            var classId = input.ReadInt();
            return Read((ObjectDataInput) input, factoryId, classId);
        }

        public void Dispose()
        {
            _factories.Clear();
        }

        internal DefaultPortableReader CreateMorphingReader(ObjectDataInput input)
        {
            var factoryId = input.ReadInt();
            var classId = input.ReadInt();
            var version = input.ReadInt();

            var portable = CreateNewPortableInstance(factoryId, classId);
            var portableVersion = FindPortableVersion(factoryId, classId, portable);

            return CreateReader(input, factoryId, classId, version, portableVersion);
        }

        /// <exception cref="System.IO.IOException"></exception>
        internal DefaultPortableReader CreateReader(ObjectDataInput input)
        {
            var factoryId = input.ReadInt();
            var classId = input.ReadInt();
            var version = input.ReadInt();
            return CreateReader(input, factoryId, classId, version, version);
        }

        /// <exception cref="System.IO.IOException"/>
        internal void WriteInternal(ObjectDataOutput output, IPortable p)
        {
            var cd = _context.LookupOrRegisterClassDefinition(p);
            output.WriteInt(cd.Version);
            var writer = new DefaultPortableWriter(this, output, cd);
            p.WritePortable(writer);
            writer.End();
        }

        private IPortable CreateNewPortableInstance(int factoryId, int classId)
        {
            _factories.TryGetValue(factoryId, out var portableFactory);
            if (portableFactory == null)
            {
                throw new SerializationException("Could not find PortableFactory for factory-id: " + factoryId);
            }
            var portable = portableFactory.Create(classId);
            if (portable == null)
            {
                throw new SerializationException("Could not create Portable for class-id: " + classId);
            }
            return portable;
        }

        private DefaultPortableReader CreateReader(ObjectDataInput input, int factoryId, int classId, int version,
            int portableVersion)
        {
            var effectiveVersion = version;
            if (version < 0)
            {
                effectiveVersion = _context.GetVersion();
            }
            var cd = _context.LookupClassDefinition(factoryId, classId, effectiveVersion);
            if (cd == null)
            {
                var begin = input.Position;
                cd = _context.ReadClassDefinition(input, factoryId, classId, effectiveVersion);
                input.Position = begin;
            }
            DefaultPortableReader reader;
            if (portableVersion == effectiveVersion)
            {
                reader = new DefaultPortableReader(this, input, cd);
            }
            else
            {
                reader = new MorphingPortableReader(this, input, cd);
            }
            return reader;
        }

        private int FindPortableVersion(int factoryId, int classId, IPortable portable)
        {
            var currentVersion = _context.GetClassVersion(factoryId, classId);
            if (currentVersion < 0)
            {
                currentVersion = PortableVersionHelper.GetVersion(portable, _context.GetVersion());
                if (currentVersion > 0)
                {
                    _context.SetClassVersion(factoryId, classId, currentVersion);
                }
            }
            return currentVersion;
        }

        /// <exception cref="System.IO.IOException"/>
        internal IPortable Read(ObjectDataInput @in, int factoryId, int classId)
        {
            var version = @in.ReadInt();
            var portable = CreateNewPortableInstance(factoryId, classId);
            var portableVersion = FindPortableVersion(factoryId, classId, portable);
            var reader = CreateReader(@in, factoryId, classId, version, portableVersion);
            portable.ReadPortable(reader);
            reader.End();
            return portable;
        }
    }
}
