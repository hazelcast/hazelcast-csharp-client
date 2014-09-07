using System;
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Core;

namespace Hazelcast.IO.Serialization
{
    internal sealed class PortableSerializer : IStreamSerializer<IPortable>
    {
        private readonly IPortableContext context;

        private readonly IDictionary<int, IPortableFactory> factories = new Dictionary<int, IPortableFactory>();

        internal PortableSerializer(IPortableContext context, IDictionary<int, IPortableFactory> portableFactories)
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
            if (p.GetClassId() == 0)
            {
                throw new ArgumentException("IPortable class id cannot be zero!");
            }
            if (!(output is IBufferObjectDataOutput))
            {
                throw new ArgumentException("ObjectDataOutput must be instance of IBufferObjectDataOutput!");
            }
            if (p.GetClassId() == 0)
            {
                throw new ArgumentException("IPortable class id cannot be zero!");
            }
            IClassDefinition cd = context.LookupOrRegisterClassDefinition(p);
            var bufferedOut = (IBufferObjectDataOutput) output;
            var writer = new DefaultPortableWriter(this, bufferedOut, cd);
            p.WritePortable(writer);
            writer.End();
        }

        /// <exception cref="System.IO.IOException"></exception>
        public IPortable Read(IObjectDataInput input)
        {
            if (!(input is IBufferObjectDataInput))
            {
                throw new ArgumentException("ObjectDataInput must be instance of IBufferObjectDataInput!");
            }
            if (!(input is PortableContextAwareInputStream))
            {
                throw new ArgumentException("ObjectDataInput must be instance of PortableContextAwareInputStream!");
            }
            var ctxIn = (PortableContextAwareInputStream) input;
            int factoryId = ctxIn.GetFactoryId();
            int dataClassId = ctxIn.GetClassId();
            int dataVersion = ctxIn.GetVersion();
            IPortableFactory portableFactory = null;
            factories.TryGetValue(factoryId, out portableFactory);
            if (portableFactory == null)
            {
                throw new HazelcastSerializationException("Could not find IPortableFactory for factory-id: " + factoryId);
            }
            IPortable portable = portableFactory.Create(dataClassId);
            if (portable == null)
            {
                throw new HazelcastSerializationException("Could not create IPortable for class-id: " + dataClassId);
            }
            DefaultPortableReader reader;
            IClassDefinition cd;
            var bufferedIn = (IBufferObjectDataInput) input;
            if (context.GetVersion() == dataVersion)
            {
                cd = context.Lookup(factoryId, dataClassId);
                // using context.version
                if (cd == null)
                {
                    throw new HazelcastSerializationException("Could not find class-definition for " + "factory-id: " +
                                                              factoryId + ", class-id: " + dataClassId + ", version: " +
                                                              dataVersion);
                }
                reader = new DefaultPortableReader(this, bufferedIn, cd);
            }
            else
            {
                cd = context.Lookup(factoryId, dataClassId, dataVersion);
                // registered during read
                if (cd == null)
                {
                    throw new HazelcastSerializationException("Could not find class-definition for " + "factory-id: " +
                                                              factoryId + ", class-id: " + dataClassId + ", version: " +
                                                              dataVersion);
                }
                reader = new MorphingPortableReader(this, bufferedIn, cd);
            }
            portable.ReadPortable(reader);
            reader.End();
            return portable;
        }

        public void Destroy()
        {
            factories.Clear();
        }

        /// <exception cref="System.IO.IOException"></exception>
        internal IPortable ReadAndInitialize(IBufferObjectDataInput input)
        {
            IPortable p = Read(input);
            IManagedContext managedContext = context.GetManagedContext();
            return managedContext != null ? (IPortable) managedContext.Initialize(p) : p;
        }
    }
}