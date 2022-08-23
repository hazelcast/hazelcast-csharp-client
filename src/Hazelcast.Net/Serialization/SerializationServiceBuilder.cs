﻿// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Configuration;
using Hazelcast.Core;
using Hazelcast.Partitioning.Strategies;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Serialization
{
    internal sealed class SerializationServiceBuilder
    {
        private const int DefaultOutBufferSize = 4 * 1024;

        private readonly SerializationOptions _options;
        private readonly SerializerHooks _hooks = new SerializerHooks();
        private readonly List<ISerializerDefinitions> _definitions = new List<ISerializerDefinitions>();
        private readonly ILoggerFactory _loggerFactory;

        private int _initialOutputBufferSize = DefaultOutBufferSize;
        private Endianness _endianness;
        private bool _validatePortableClassDefinitions;
        private int _portableVersion;

        private readonly IDictionary<int, IDataSerializableFactory> _dataSerializableFactories = new Dictionary<int, IDataSerializableFactory>();
        private readonly IDictionary<int, IPortableFactory> _portableFactories = new Dictionary<int, IPortableFactory>();
        private ICollection<IClassDefinition> _portableClassDefinitions = new HashSet<IClassDefinition>();

        private IPartitioningStrategy _partitioningStrategy;

        public SerializationServiceBuilder(ILoggerFactory loggerFactory)
            : this(new SerializationOptions(), loggerFactory)
        { }

        public SerializationServiceBuilder(SerializationOptions options, ILoggerFactory loggerFactory)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

            if (options.PortableVersion < 0) throw new ConfigurationException("PortableVersion must be >= 0.");
            _portableVersion = options.PortableVersion;

            _validatePortableClassDefinitions = options.ValidateClassDefinitions;
            _endianness = options.Endianness;
        }

        public SerializationServiceBuilder AddHook<T>() => AddHook(typeof (T));

        public SerializationServiceBuilder AddHook(Type type)
        {
            _hooks.Add(type);
            return this;
        }

        public SerializationServiceBuilder AddDefinitions(ISerializerDefinitions definition)
        {
            _definitions.Add(definition);
            return this;
        }

        public SerializationServiceBuilder SetPortableVersion(int version)
        {
            if (version < 0)
                throw new ArgumentOutOfRangeException(nameof(version), "Value must be greater than, or equal to, zero.");

            _portableVersion = version;
            return this;
        }

        public SerializationServiceBuilder AddDataSerializableFactory(int id, IDataSerializableFactory factory)
        {
            _dataSerializableFactories.Add(id, factory);
            return this;
        }

        public SerializationServiceBuilder AddPortableFactory(int id, IPortableFactory factory)
        {
            _portableFactories.Add(id, factory);
            return this;
        }

        public SerializationServiceBuilder AddClassDefinition(IClassDefinition cd)
        {
            _portableClassDefinitions.Add(cd);
            return this;
        }

        public SerializationServiceBuilder SetValidatePortableClassDefinitions(bool validatePortableClassDefinitions)
        {
            _validatePortableClassDefinitions = validatePortableClassDefinitions;
            return this;
        }

        public SerializationServiceBuilder SetEndianness(Endianness endianness)
        {
            _endianness = endianness;
            return this;
        }

        public SerializationServiceBuilder SetPartitioningStrategy(IPartitioningStrategy partitionStrategy)
        {
            _partitioningStrategy = partitionStrategy;
            return this;
        }

        public SerializationServiceBuilder SetInitialOutputBufferSize(int initialOutputBufferSize)
        {
            if (initialOutputBufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialOutputBufferSize), "Value must be greater than zero.");
            }
            _initialOutputBufferSize = initialOutputBufferSize;
            return this;
        }

        public SerializationService Build()
        {
            // merge lists
            AddDataSerializableFactoriesFromOptions(_dataSerializableFactories, _options);
            AddPortableFactoriesFromOptions(_portableFactories, _options);
            _portableClassDefinitions = _portableClassDefinitions.Union(_options.ClassDefinitions).ToList();

            var service = new SerializationService(
                _options,
                _endianness,
                _portableVersion,
                _dataSerializableFactories,
                _portableFactories,
                _portableClassDefinitions,
                _hooks,
                _definitions,
                _validatePortableClassDefinitions,
                _partitioningStrategy,
                _initialOutputBufferSize,
                _loggerFactory);

            return service;
        }

        private static void AddDataSerializableFactoriesFromOptions(IDictionary<int, IDataSerializableFactory> dataSerializableFactories, SerializationOptions options)
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

        private static void AddPortableFactoriesFromOptions(IDictionary<int, IPortableFactory> portableFactories, SerializationOptions options)
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

    }
}
