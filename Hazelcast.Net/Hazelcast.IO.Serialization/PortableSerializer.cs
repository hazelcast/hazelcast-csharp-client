/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Core;

namespace Hazelcast.IO.Serialization
{
    internal sealed class PortableSerializer : IStreamSerializer<IPortable>
    {
        private readonly PortableContext context;
        private readonly IDictionary<int, IPortableFactory> factories = new Dictionary<int, IPortableFactory>();

        internal PortableSerializer(PortableContext context, IEnumerable<KeyValuePair<int, IPortableFactory>> portableFactories)
        {
            this.context = context;
            factories = factories.Union(portableFactories).ToDictionary(x => x.Key, x => x.Value);
        }

        public int GetTypeId()
        {
            return SerializationConstants.ConstantTypePortable;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public void Write(IObjectDataOutput output, IPortable p)
        {
            if (!(output is IBufferObjectDataOutput))
            {
                throw new ArgumentException("ObjectDataOutput must be instance of BufferObjectDataOutput!");
            }
            if (p.GetClassId() == 0)
            {
                throw new ArgumentException("Portable class id cannot be zero!");
            }
            output.WriteInt(p.GetFactoryId());
            output.WriteInt(p.GetClassId());
            WriteInternal((IBufferObjectDataOutput)output, p);
        }

        /// <exception cref="System.IO.IOException"/>
        internal void WriteInternal(IBufferObjectDataOutput output, IPortable p)
        {
            IClassDefinition cd = context.LookupOrRegisterClassDefinition(p);
            output.WriteInt(cd.GetVersion());
            DefaultPortableWriter writer = new DefaultPortableWriter(this, output, cd);
            p.WritePortable(writer);
            writer.End();
        }

        /// <exception cref="System.IO.IOException"></exception>
        public IPortable Read(IObjectDataInput input)
        {
            if (!(input is IBufferObjectDataInput))
            {
                throw new ArgumentException("ObjectDataInput must be instance of BufferObjectDataInput!");
            }
            int factoryId = input.ReadInt();
            int classId = input.ReadInt();
            return Read((IBufferObjectDataInput)input, factoryId, classId);
        }

        /// <exception cref="System.IO.IOException"/>
        private IPortable Read(IBufferObjectDataInput @in, int factoryId, int classId)
        {
            int version = @in.ReadInt();
            IPortable portable = CreateNewPortableInstance(factoryId, classId);
            int portableVersion = FindPortableVersion(factoryId, classId, portable);
            DefaultPortableReader reader = CreateReader(@in, factoryId, classId, version, portableVersion);
            portable.ReadPortable(reader);
            reader.End();
            return portable;
        }

        private int FindPortableVersion(int factoryId, int classId, IPortable portable)
        {
            int currentVersion = context.GetClassVersion(factoryId, classId);
            if (currentVersion < 0)
            {
                currentVersion = PortableVersionHelper.GetVersion(portable, context.GetVersion());
                if (currentVersion > 0)
                {
                    context.SetClassVersion(factoryId, classId, currentVersion);
                }
            }
            return currentVersion;
        }

        private IPortable CreateNewPortableInstance(int factoryId, int classId)
        {
            IPortableFactory portableFactory;
            factories.TryGetValue(factoryId, out portableFactory);
            if (portableFactory == null)
            {
                throw new HazelcastSerializationException("Could not find PortableFactory for factory-id: " + factoryId);
            }
            IPortable portable = portableFactory.Create(classId);
            if (portable == null)
            {
                throw new HazelcastSerializationException("Could not create Portable for class-id: " + classId);
            }
            return portable;
        }

        /// <exception cref="System.IO.IOException"/>
        internal IPortable ReadAndInitialize(IBufferObjectDataInput @in, int factoryId, int classId)
        {
            IPortable p = Read(@in, factoryId, classId);
            IManagedContext managedContext = context.GetManagedContext();
            return managedContext != null ? (IPortable)managedContext.Initialize(p) : p;
        }

        /// <exception cref="System.IO.IOException"></exception>
        internal DefaultPortableReader CreateReader(IBufferObjectDataInput input)
        {
            int factoryId = input.ReadInt();
            int classId = input.ReadInt();
            int version = input.ReadInt();
            return CreateReader(input, factoryId, classId, version, version);
        }

        private DefaultPortableReader CreateReader(IBufferObjectDataInput input, int factoryId, int classId, int version, int portableVersion)
        {
            int effectiveVersion = version;
            if (version < 0)
            {
                effectiveVersion = context.GetVersion();
            }
            IClassDefinition cd = context.LookupClassDefinition(factoryId, classId, effectiveVersion);
            if (cd == null)
            {
                int begin = input.Position();
                cd = context.ReadClassDefinition(input, factoryId, classId, effectiveVersion);
                input.Position(begin);
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

        public void Destroy()
        {
            factories.Clear();
        }
    }
}