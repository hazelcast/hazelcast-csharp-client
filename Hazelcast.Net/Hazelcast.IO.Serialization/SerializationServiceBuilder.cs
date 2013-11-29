using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hazelcast.Config;
using Hazelcast.Core;

namespace Hazelcast.IO.Serialization
{
    public sealed class SerializationServiceBuilder
    {
        private readonly IDictionary<int, IDataSerializableFactory> dataSerializableFactories =
            new Dictionary<int, IDataSerializableFactory>();

        private readonly IDictionary<int, IPortableFactory> portableFactories = new Dictionary<int, IPortableFactory>();

        private bool checkClassDefErrors = true;

        private ICollection<IClassDefinition> classDefinitions =
            new HashSet<IClassDefinition>();

        private SerializationConfig config;

        private bool enableCompression;

        private bool enableSharedObject;
        private IHazelcastInstance hazelcastInstance;

        private int initialOutputBufferSize = 4*1024;
        private bool isBigEndian = true;
        private IManagedContext managedContext;

        private IPartitioningStrategy partitioningStrategy;
        private bool useNativeByteOrder;
        private int version = -1;

        public SerializationServiceBuilder SetVersion(int version)
        {
            if (version < 0)
            {
                throw new ArgumentException("Version cannot be negative!");
            }
            this.version = version;
            return this;
        }

        public SerializationServiceBuilder SetConfig(SerializationConfig config)
        {
            this.config = config;
            if (version < 0)
            {
                version = config.GetPortableVersion();
            }
            checkClassDefErrors = config.IsCheckClassDefErrors();
            useNativeByteOrder = config.IsUseNativebool();
            isBigEndian = config.IsBigEndian();
            enableCompression = config.IsEnableCompression();
            enableSharedObject = config.IsEnableSharedObject();
            return this;
        }

        public SerializationServiceBuilder AddDataSerializableFactory(int id, IDataSerializableFactory factory)
        {
            dataSerializableFactories.Add(id, factory);
            return this;
        }

        public SerializationServiceBuilder AddPortableFactory(int id, IPortableFactory factory)
        {
            portableFactories.Add(id, factory);
            return this;
        }

        public SerializationServiceBuilder AddClassDefinition(IClassDefinition cd)
        {
            classDefinitions.Add(cd);
            return this;
        }

        public SerializationServiceBuilder SetCheckClassDefErrors(bool checkClassDefErrors)
        {
            this.checkClassDefErrors = checkClassDefErrors;
            return this;
        }

        public SerializationServiceBuilder SetManagedContext(IManagedContext managedContext)
        {
            this.managedContext = managedContext;
            return this;
        }

        public SerializationServiceBuilder SetUseNativeByteOrder(bool useNativeByteOrder)
        {
            this.useNativeByteOrder = useNativeByteOrder;
            return this;
        }

        public SerializationServiceBuilder SetBigEndian(bool isBigEndian)
        {
            this.isBigEndian = isBigEndian;
            return this;
        }

        public SerializationServiceBuilder SetHazelcastInstance(IHazelcastInstance hazelcastInstance)
        {
            this.hazelcastInstance = hazelcastInstance;
            return this;
        }

        public SerializationServiceBuilder SetEnableCompression(bool enableCompression)
        {
            this.enableCompression = enableCompression;
            return this;
        }

        public SerializationServiceBuilder SetEnableSharedObject(bool enableSharedObject)
        {
            this.enableSharedObject = enableSharedObject;
            return this;
        }

        public SerializationServiceBuilder SetPartitioningStrategy(IPartitioningStrategy partitionStrategy)
        {
            partitioningStrategy = partitionStrategy;
            return this;
        }

        public SerializationServiceBuilder SetInitialOutputBufferSize(int initialOutputBufferSize)
        {
            if (initialOutputBufferSize <= 0)
            {
                throw new ArgumentException("Initial buffer size must be positive!");
            }
            this.initialOutputBufferSize = initialOutputBufferSize;
            return this;
        }

        public ISerializationService Build()
        {
            if (version < 0)
            {
                version = 0;
            }
            if (config != null)
            {
                AddConfigDataSerializableFactories(dataSerializableFactories, config);
                AddConfigPortableFactories(portableFactories, config);
                classDefinitions = classDefinitions.Union(config.GetClassDefinitions()).ToList();
            }
            ISerializationService ss = new SerializationService(
                CreateInputOutputFactory(),
                version,
                dataSerializableFactories,
                portableFactories,
                classDefinitions,
                checkClassDefErrors,
                managedContext,
                partitioningStrategy,
                initialOutputBufferSize,
                enableCompression,
                enableSharedObject);
            if (config != null)
            {
                if (config.GetGlobalSerializerConfig() != null)
                {
                    GlobalSerializerConfig globalSerializerConfig = config.GetGlobalSerializerConfig();
                    ISerializer serializer = globalSerializerConfig.GetImplementation();
                    if (serializer == null)
                    {
                        try
                        {
                            string className = globalSerializerConfig.GetClassName();
                            Type type = Type.GetType(className);
                            if (type != null)
                            {
                                serializer = Activator.CreateInstance(type) as ISerializer;
                            }
                        }
                        catch (Exception e)
                        {
                            throw new HazelcastSerializationException(e);
                        }
                    }
                    var aware = serializer as IHazelcastInstanceAware;
                    if (aware != null)
                    {
                        aware.SetHazelcastInstance(hazelcastInstance);
                    }
                    ss.RegisterGlobal(serializer);
                }
                ICollection<SerializerConfig> typeSerializers = config.GetSerializerConfigs();
                foreach (SerializerConfig serializerConfig in typeSerializers)
                {
                    ISerializer serializer = serializerConfig.GetImplementation();
                    if (serializer == null)
                    {
                        try
                        {
                            string className = serializerConfig.GetClassName();
                            Type type = Type.GetType(className);
                            if (type != null)
                            {
                                serializer = Activator.CreateInstance(type) as ISerializer;
                            }
                        }
                        catch (Exception e)
                        {
                            throw new HazelcastSerializationException(e);
                        }
                    }
                    if (serializer is IHazelcastInstanceAware)
                    {
                        ((IHazelcastInstanceAware) serializer).SetHazelcastInstance(hazelcastInstance);
                    }
                    Type typeClass = serializerConfig.GetTypeClass();
                    if (typeClass == null)
                    {
                        try
                        {
                            string className = serializerConfig.GetTypeClassName();
                            typeClass = Type.GetType(className);
                        }
                        catch (TypeLoadException e)
                        {
                            throw new HazelcastSerializationException(e);
                        }
                    }
                    //call by reflaction
                    MethodInfo method = typeof (ISerializationService).GetMethod("Register");
                    MethodInfo generic = method.MakeGenericMethod(typeClass);
                    generic.Invoke(null, new object[] {serializer});
                    //mimics: ss.Register<typeClass>(serializer);"
                    //
                }
            }
            return ss;
        }

        private IInputOutputFactory CreateInputOutputFactory()
        {
            if (useNativeByteOrder || isBigEndian == BitConverter.IsLittleEndian)
            {
                isBigEndian = BitConverter.IsLittleEndian;
            }
            if (isBigEndian)
            {
                return new ByteArrayInputOutputFactory();
            }
            throw new NotImplementedException("LITTLE ENDIAN SUPPORT NOT IMPLEMENTED");
            //return new ByteBufferInputOutputFactory(byteOrder);
        }

        private void AddConfigDataSerializableFactories(
            IDictionary<int, IDataSerializableFactory> dataSerializableFactories, SerializationConfig config)
        {
            foreach (var entry in config.GetDataSerializableFactories())
            {
                int factoryId = entry.Key;
                IDataSerializableFactory factory = entry.Value;
                if (factoryId <= 0)
                {
                    throw new ArgumentException("IDataSerializableFactory factoryId must be positive! -> " + factory);
                }
                if (dataSerializableFactories.ContainsKey(factoryId))
                {
                    throw new ArgumentException("IDataSerializableFactory with factoryId '" + factoryId +
                                                "' is already registered!");
                }
                dataSerializableFactories.Add(factoryId, factory);
            }
            foreach (var entry in config.GetDataSerializableFactoryClasses())
            {
                int factoryId = entry.Key;
                string factoryClassName = entry.Value;
                if (factoryId <= 0)
                {
                    throw new ArgumentException("IDataSerializableFactory factoryId must be positive! -> " +
                                                factoryClassName);
                }
                if (dataSerializableFactories.ContainsKey(factoryId))
                {
                    throw new ArgumentException("IDataSerializableFactory with factoryId '" + factoryId +
                                                "' is already registered!");
                }
                IDataSerializableFactory factory = null;
                try
                {
                    //TODO CLASSLOAD
                    Type type = Type.GetType(factoryClassName);
                    if (type != null)
                    {
                        factory = Activator.CreateInstance(type) as IDataSerializableFactory;
                    }
                }
                catch (Exception e)
                {
                    //ClassLoaderUtil.newInstance(cl, factoryClassName);
                    throw new HazelcastSerializationException(e);
                }
                dataSerializableFactories.Add(factoryId, factory);
            }
            foreach (IDataSerializableFactory f in dataSerializableFactories.Values)
            {
                var aware = f as IHazelcastInstanceAware;
                if (aware != null)
                {
                    aware.SetHazelcastInstance(hazelcastInstance);
                }
            }
        }

        private void AddConfigPortableFactories(IDictionary<int, IPortableFactory> portableFactories,
            SerializationConfig config)
        {
            foreach (var entry in config.GetPortableFactories())
            {
                int factoryId = entry.Key;
                IPortableFactory factory = entry.Value;
                if (factoryId <= 0)
                {
                    throw new ArgumentException("IPortableFactory factoryId must be positive! -> " + factory);
                }
                if (portableFactories.ContainsKey(factoryId))
                {
                    throw new ArgumentException("IPortableFactory with factoryId '" + factoryId +
                                                "' is already registered!");
                }
                portableFactories.Add(factoryId, factory);
            }
            foreach (var entry in config.GetPortableFactoryClasses())
            {
                int factoryId = entry.Key;
                string factoryClassName = entry.Value;
                if (factoryId <= 0)
                {
                    throw new ArgumentException("IPortableFactory factoryId must be positive! -> " + factoryClassName);
                }
                if (portableFactories.ContainsKey(factoryId))
                {
                    throw new ArgumentException("IPortableFactory with factoryId '" + factoryId +
                                                "' is already registered!");
                }
                IPortableFactory factory;
                try
                {
                    //TODO CLASSLOAD
                    factory = null;
                }
                catch (Exception e)
                {
                    //ClassLoaderUtil.newInstance(cl, factoryClassName);
                    throw new HazelcastSerializationException(e);
                }
                portableFactories.Add(factoryId, factory);
            }
            foreach (IPortableFactory f in portableFactories.Values)
            {
                if (f is IHazelcastInstanceAware)
                {
                    ((IHazelcastInstanceAware) f).SetHazelcastInstance(hazelcastInstance);
                }
            }
        }
    }
}