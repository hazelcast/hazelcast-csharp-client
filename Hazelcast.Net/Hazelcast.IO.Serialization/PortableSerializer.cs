using System;
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Core;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    internal sealed class PortableSerializer : IStreamSerializer<IPortable>
    {
        private readonly IPortableContext context;
        private readonly IDictionary<int, IPortableFactory> factories = new Dictionary<int, IPortableFactory>();

        internal PortableSerializer(IPortableContext context, IEnumerable<KeyValuePair<int, IPortableFactory>> portableFactories)
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
            if (!(output is IPortableDataOutput))
            {
                throw new ArgumentException("ObjectDataOutput must be instance of PortableDataOutput!");
            }
            if (p.GetClassId() == 0)
            {
                throw new ArgumentException("Portable class id cannot be zero!");
            }
            IClassDefinition cd = context.LookupOrRegisterClassDefinition(p);
            var portableDataOutput = (IPortableDataOutput) output;
            DynamicByteBuffer headerBuffer = portableDataOutput.GetHeaderBuffer();
            int pos = headerBuffer.Position();
            output.WriteInt(pos);
            headerBuffer.PutInt(cd.GetFactoryId());
            headerBuffer.PutInt(cd.GetClassId());
            headerBuffer.PutInt(cd.GetVersion());
            var writer = new DefaultPortableWriter(this, portableDataOutput, cd);
            p.WritePortable(writer);
            writer.End();
        }

        /// <exception cref="System.IO.IOException"></exception>
        public IPortable Read(IObjectDataInput input)
        {
            if (!(input is IPortableDataInput))
            {
                throw new ArgumentException("ObjectDataInput must be instance of PortableDataInput!");
            }
            var portableDataInput = (IPortableDataInput) input;
            ByteBuffer headerBuffer = portableDataInput.GetHeaderBuffer();
            headerBuffer.Position = portableDataInput.ReadInt();
            int factoryId = headerBuffer.GetInt();
            int classId = headerBuffer.GetInt();
            int version = headerBuffer.GetInt();
            IPortable portable = CreateNewPortableInstance(factoryId, classId);
            int portableVersion = FindPortableVersion(factoryId, classId, portable);
            DefaultPortableReader reader = CreateReader(portableDataInput, factoryId, classId, version, portableVersion);
            portable.ReadPortable(reader);
            reader.End();
            return portable;
        }

        public void Destroy()
        {
            factories.Clear();
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

        /// <exception cref="System.IO.IOException"></exception>
        internal IPortable ReadAndInitialize(IBufferObjectDataInput input)
        {
            IPortable p = Read(input);
            IManagedContext managedContext = context.GetManagedContext();
            return managedContext != null ? (IPortable) managedContext.Initialize(p) : p;
        }

        /// <exception cref="System.IO.IOException"></exception>
        internal DefaultPortableReader CreateReader(IBufferObjectDataInput input)
        {
            ByteBuffer headerBuffer = ((IPortableDataInput) input).GetHeaderBuffer();
            headerBuffer.Position = input.ReadInt();
            int factoryId = headerBuffer.GetInt();
            int classId = headerBuffer.GetInt();
            int version = headerBuffer.GetInt();
            return CreateReader(input, factoryId, classId, version, version);
        }

        private DefaultPortableReader CreateReader(IBufferObjectDataInput input, int factoryId
            , int classId, int version, int portableVersion)
        {
            int effectiveVersion = version;
            if (version < 0)
            {
                effectiveVersion = context.GetVersion();
            }
            IClassDefinition cd = context.LookupClassDefinition(factoryId, classId, effectiveVersion);
            if (cd == null)
            {
                throw new HazelcastSerializationException("Could not find class-definition for factory-id: " + factoryId +
                                                          ", class-id: " + classId + ", version: " + effectiveVersion);
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
    }
}