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
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    internal sealed class SerializationServiceBuilder : ISerializationServiceBuilder
    {
        private const int DEFAULT_OUT_BUFFER_SIZE = 4*1024;

        private ICollection<IClassDefinition> classDefinitions =
            new HashSet<IClassDefinition>();

        private readonly IDictionary<int, IDataSerializableFactory> _dataSerializableFactories =
            new Dictionary<int, IDataSerializableFactory>();

        private readonly IDictionary<int, IPortableFactory> _portableFactories =
            new Dictionary<int, IPortableFactory>();

        private ByteOrder _byteOrder = ByteOrder.BigEndian;
        private bool _checkClassDefErrors = true;

        private SerializationConfig _config;

        private bool _enableCompression;

        private bool _enableSharedObject;
        private IHazelcastInstance _hazelcastInstance;

        private int _initialOutputBufferSize = DEFAULT_OUT_BUFFER_SIZE;
        private IManagedContext _managedContext;

        private IPartitioningStrategy _partitioningStrategy;
        private bool _useNativeByteOrder;

        private int _portableVersion = -1;
        private byte _version = SerializationService.SerializerVersion;

        public ISerializationServiceBuilder SetVersion(byte version)
        {
            byte maxVersion = SerializationService.SerializerVersion;
            if (version > maxVersion)
            {
                throw new ArgumentException(
                        "Configured serialization version is higher than the max supported version :" + maxVersion);
            }
            _version = version;
            return this;
        }

        public ISerializationServiceBuilder SetPortableVersion(int version)
        {
            if (version < 0)
            {
                throw new ArgumentException("Version cannot be negative!");
            }
            _portableVersion = version;
            return this;
        }

        public ISerializationServiceBuilder SetConfig(SerializationConfig config)
        {
            this._config = config;
            if (_portableVersion < 0)
            {
                _portableVersion = config.GetPortableVersion();
            }
            _checkClassDefErrors = config.IsCheckClassDefErrors();
            _useNativeByteOrder = config.IsUseNativeByteOrder();
            _byteOrder = config.GetByteOrder();
            _enableCompression = config.IsEnableCompression();
            _enableSharedObject = config.IsEnableSharedObject();
            return this;
        }

        public ISerializationServiceBuilder AddDataSerializableFactory(int id, IDataSerializableFactory factory)
        {
            _dataSerializableFactories.Add(id, factory);
            return this;
        }

        public ISerializationServiceBuilder AddPortableFactory(int id, IPortableFactory factory)
        {
            _portableFactories.Add(id, factory);
            return this;
        }

        public ISerializationServiceBuilder AddClassDefinition(IClassDefinition cd)
        {
            classDefinitions.Add(cd);
            return this;
        }

        public ISerializationServiceBuilder SetCheckClassDefErrors(bool checkClassDefErrors)
        {
            this._checkClassDefErrors = checkClassDefErrors;
            return this;
        }

        public ISerializationServiceBuilder SetManagedContext(IManagedContext managedContext)
        {
            this._managedContext = managedContext;
            return this;
        }

        public ISerializationServiceBuilder SetUseNativeByteOrder(bool useNativeByteOrder)
        {
            this._useNativeByteOrder = useNativeByteOrder;
            return this;
        }

        public ISerializationServiceBuilder SetByteOrder(ByteOrder byteOrder)
        {
            this._byteOrder = byteOrder;
            return this;
        }

        public ISerializationServiceBuilder SetHazelcastInstance(IHazelcastInstance hazelcastInstance)
        {
            this._hazelcastInstance = hazelcastInstance;
            return this;
        }

        public ISerializationServiceBuilder SetEnableCompression(bool enableCompression)
        {
            this._enableCompression = enableCompression;
            return this;
        }

        public ISerializationServiceBuilder SetEnableSharedObject(bool enableSharedObject)
        {
            this._enableSharedObject = enableSharedObject;
            return this;
        }

        public ISerializationServiceBuilder SetPartitioningStrategy(IPartitioningStrategy partitionStrategy)
        {
            _partitioningStrategy = partitionStrategy;
            return this;
        }

        public ISerializationServiceBuilder SetInitialOutputBufferSize(int initialOutputBufferSize)
        {
            if (initialOutputBufferSize <= 0)
            {
                throw new ArgumentException("Initial buffer size must be positive!");
            }
            this._initialOutputBufferSize = initialOutputBufferSize;
            return this;
        }

        public ISerializationService Build()
        {
            if (_portableVersion < 0)
            {
                _portableVersion = 0;
            }
            if (_config != null)
            {
                AddConfigDataSerializableFactories(_dataSerializableFactories, _config);
                AddConfigPortableFactories(_portableFactories, _config);
                classDefinitions = classDefinitions.Union(_config.GetClassDefinitions()).ToList();
            }
            //TODO: add support for multiple versions
            var ss = new SerializationService(
                CreateInputOutputFactory(),
                _portableVersion,
                _dataSerializableFactories,
                _portableFactories,
                classDefinitions,
                _checkClassDefErrors,
                _managedContext,
                _partitioningStrategy,
                _initialOutputBufferSize,
                _enableCompression,
                _enableSharedObject);
            if (_config != null)
            {
                if (_config.GetGlobalSerializerConfig() != null)
                {
                    GlobalSerializerConfig globalSerializerConfig = _config.GetGlobalSerializerConfig();
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
                        aware.SetHazelcastInstance(_hazelcastInstance);
                    }
                    ss.RegisterGlobal(serializer);
                }
                ICollection<SerializerConfig> typeSerializers = _config.GetSerializerConfigs();
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
                        ((IHazelcastInstanceAware)serializer).SetHazelcastInstance(_hazelcastInstance);
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
                    ////call by reflaction
                    //MethodInfo method = typeof(ISerializationService).GetMethod("Register");
                    //MethodInfo generic = method.MakeGenericMethod(typeClass);
                    //generic.Invoke(ss, new object[] { serializer });
                    ////mimics: ss.Register<typeClass>(serializer);"
                    ss.Register(typeClass, serializer);
                }
            }
            return ss;
        }

        private IInputOutputFactory CreateInputOutputFactory()
        {
            if (_byteOrder == null)
            {
                _byteOrder = ByteOrder.BigEndian;
            }
            if (_useNativeByteOrder || _byteOrder == ByteOrder.NativeOrder())
            {
                _byteOrder = ByteOrder.NativeOrder();
            }
            return new ByteArrayInputOutputFactory(_byteOrder);
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
                    Type type = Type.GetType(factoryClassName);
                    if (type != null)
                    {
                        factory = Activator.CreateInstance(type) as IDataSerializableFactory;
                    }
                }
                catch (Exception e)
                {
                    throw new HazelcastSerializationException(e);
                }
                dataSerializableFactories.Add(factoryId, factory);
            }
            foreach (IDataSerializableFactory f in dataSerializableFactories.Values)
            {
                var aware = f as IHazelcastInstanceAware;
                if (aware != null)
                {
                    aware.SetHazelcastInstance(_hazelcastInstance);
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

                var type = Type.GetType(factoryClassName);
                if (type == null)
                {
                    throw new HazelcastSerializationException("Unable to find type " + factoryClassName);
                }
                if (!typeof (IPortableFactory).IsAssignableFrom(type))
                {
                    throw new HazelcastSerializationException("Type " + type + " does not implement IPortableFactory");
                }
                var factory = Activator.CreateInstance(type) as IPortableFactory;
                portableFactories.Add(factoryId, factory);
            }

            foreach (IPortableFactory f in portableFactories.Values)
            {
                if (f is IHazelcastInstanceAware)
                {
                    ((IHazelcastInstanceAware)f).SetHazelcastInstance(_hazelcastInstance);
                }
            }
        }
    }
}