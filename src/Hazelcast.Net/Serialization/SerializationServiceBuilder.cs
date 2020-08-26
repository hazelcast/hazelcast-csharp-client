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
using Hazelcast.Core;
using Hazelcast.Partitioning.Strategies;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Serialization
{
    internal sealed class SerializationServiceBuilder : ISerializationServiceBuilder
    {
        private readonly ILoggerFactory _loggerFactory;
        private const int DefaultOutBufferSize = 4*1024;

        private readonly IDictionary<int, IDataSerializableFactory> _dataSerializableFactories =
            new Dictionary<int, IDataSerializableFactory>();

        private readonly IDictionary<int, IPortableFactory> _portableFactories =
            new Dictionary<int, IPortableFactory>();

        private Endianness _endianness = Endianness.BigEndian;
        private bool _checkClassDefErrors = true;

        private ICollection<IClassDefinition> _classDefinitions =
            new HashSet<IClassDefinition>();

        private readonly SerializerHooks _hooks;
        private readonly List<ISerializerDefinitions> _definitions = new List<ISerializerDefinitions>();

        private SerializationOptions _options;

        private int _initialOutputBufferSize = DefaultOutBufferSize;

        private IPartitioningStrategy _partitioningStrategy;

        private int _portableVersion = -1;
        private byte _version = SerializationService.SerializerVersion;

        public SerializationServiceBuilder(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _hooks = new SerializerHooks();
        }

        public ISerializationServiceBuilder SetVersion(byte version)
        {
            if (version > SerializationService.SerializerVersion)
                throw new ArgumentException($"Value cannot be higher than the max supported version ({SerializationService.SerializerVersion}).");

            _version = version;
            return this;
        }

        public ISerializationServiceBuilder AddHook<T>()
        {
            _hooks.Add(typeof(T));
            return this;
        }

        public ISerializationServiceBuilder AddHook(Type type)
        {
            _hooks.Add(type);
            return this;
        }

        public ISerializationServiceBuilder AddDefinitions(ISerializerDefinitions definition)
        {
            _definitions.Add(definition);
            return this;
        }

        public ISerializationServiceBuilder SetPortableVersion(int version)
        {
            if (version < 0)
                throw new ArgumentOutOfRangeException(nameof(version), "Value must be greater than, or equal to, zero.");

            _portableVersion = version;
            return this;
        }

        public ISerializationServiceBuilder SetConfig(SerializationOptions options)
        {
            _options = options;
            if (_portableVersion < 0)
                _portableVersion = options.PortableVersion;

            _checkClassDefErrors = options.ValidateClassDefinitions;
            _endianness = options.Endianness;

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
            _classDefinitions.Add(cd);
            return this;
        }

        public ISerializationServiceBuilder SetCheckClassDefErrors(bool checkClassDefErrors)
        {
            _checkClassDefErrors = checkClassDefErrors;
            return this;
        }

        public ISerializationServiceBuilder SetEndianness(Endianness endianness)
        {
            _endianness = endianness;
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
                throw new ArgumentOutOfRangeException(nameof(initialOutputBufferSize), "Value must be greater than zero.");
            }
            _initialOutputBufferSize = initialOutputBufferSize;
            return this;
        }

        public ISerializationService Build()
        {
            if (_portableVersion < 0)
                _portableVersion = 0;

            if (_options != null)
            {
                AddConfigDataSerializableFactories(_dataSerializableFactories, _options);
                AddConfigPortableFactories(_portableFactories, _options);
                _classDefinitions = _classDefinitions.Union(_options.ClassDefinitions).ToList();
            }

            var service = new SerializationService(
                CreateInputOutputFactory(),
                _portableVersion,
                _dataSerializableFactories,
                _portableFactories,
                _classDefinitions,
                _hooks,
                _definitions,
                _checkClassDefErrors,
                _partitioningStrategy,
                _initialOutputBufferSize,
                _loggerFactory);

            if (_options != null)
            {
                var globalSerializer = _options.DefaultSerializer;
                if (globalSerializer != null)
                    service.SetGlobalSerializer(globalSerializer.Service, globalSerializer.OverrideClr);

                foreach (var serializer in _options.Serializers)
                    service.AddConfiguredSerializer(serializer.SerializedType, serializer.Service);
            }

            return service;
        }

        private static void AddConfigDataSerializableFactories(IDictionary<int, IDataSerializableFactory> dataSerializableFactories, SerializationOptions options)
        {
            foreach (var factoryOptions in options.DataSerializableFactories)
            {
                if (factoryOptions.Id <= 0)
                    throw new ArgumentException("IDataSerializableFactory factoryId must be positive.");

                if (dataSerializableFactories.ContainsKey(factoryOptions.Id))
                    throw new InvalidOperationException($"IDataSerializableFactory with factoryId {factoryOptions.Id} is already registered.");

                dataSerializableFactories.Add(factoryOptions.Id, factoryOptions.Service);
            }
        }

        private static void AddConfigPortableFactories(IDictionary<int, IPortableFactory> portableFactories, SerializationOptions options)
        {
            foreach (var factoryOptions in options.PortableFactories)
            {
                if (factoryOptions.Id <= 0)
                    throw new ArgumentException("IPortableFactory factoryId must be positive.");

                if (portableFactories.ContainsKey(factoryOptions.Id))
                    throw new InvalidOperationException($"IPortableFactory with factoryId {factoryOptions.Id} is already registered.");

                portableFactories.Add(factoryOptions.Id, factoryOptions.Service);
            }
        }

        private IInputOutputFactory CreateInputOutputFactory()
        {
            if (_endianness == Endianness.Unspecified)
                _endianness = Endianness.BigEndian;

            return new ByteArrayInputOutputFactory(_endianness);
        }
    }
}
