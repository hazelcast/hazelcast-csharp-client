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
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Serialization
{
    /// <summary>Implements identified data serialization.</summary>
    internal sealed class DataSerializer : IStreamSerializer<IIdentifiedDataSerializable>
    {
        private readonly ILogger _logger;

        private readonly IDictionary<int, IDataSerializableFactory> _factories =
            new Dictionary<int, IDataSerializableFactory>();

        internal DataSerializer(IEnumerable<IDataSerializerHook> hooks, IEnumerable<KeyValuePair<int, IDataSerializableFactory>> dataSerializableFactories, ILoggerFactory loggerFactory )
        {
            _logger = loggerFactory.CreateLogger<DataSerializer>();

            if (hooks != null)
            {
                foreach (var hook in hooks)
                    Register(hook.FactoryId, hook.CreateFactory());
            }

            if (dataSerializableFactories != null)
            {
                foreach (var (factoryId, factory) in dataSerializableFactories)
                    Register(factoryId, factory);
            }
        }

        public int TypeId => SerializationConstants.ConstantTypeDataSerializable;

        public IIdentifiedDataSerializable Read(IObjectDataInput input)
        {
            var factoryId = 0;
            var id = 0;
            try
            {
                var identified = input.ReadBoolean();
                if (!identified)
                    throw new SerializationException("Non-identified IDataSerializable is not supported by the .NET client, " +
                                                              "use IIdentifiedDataSerializable instead.");

                factoryId = input.ReadInt();
                if (!_factories.TryGetValue(factoryId, out var factory))
                    throw new SerializationException($"No IDataSerializerFactory registered with factory-id {factoryId}.");

                id = input.ReadInt();
                var serializable = factory.Create(id);
                if (serializable == null)
                    throw new SerializationException($"IDataSerializerFactory factory {factory} with factory-id {factoryId} failed " +
                                                     $" to create an instance of type-id {id}.");

                serializable.ReadData(input);
                return serializable;
            }
            catch (Exception e) when (!(e is IOException || e is SerializationException))
            {
                throw new SerializationException($"Failed to read IIdentifiedDataSerializable with factory-id {factoryId} and type-id {id} ({e.GetType()}: {e.Message}).", e);
            }
        }

        public void Write(IObjectDataOutput output, IIdentifiedDataSerializable obj)
        {
            output.WriteBoolean(true); // identified flag
            output.WriteInt(obj.FactoryId);
            output.WriteInt(obj.ClassId);
            obj.WriteData(output);
        }

        public void Dispose()
        {
            _factories.Clear();
        }

        private void Register(int factoryId, IDataSerializableFactory factory)
        {
            if (!_factories.TryGetValue(factoryId, out var current))
            {
                _factories.Add(factoryId, factory);
                return;
            }

            if (current.Equals(factory))
            {
                _logger.LogWarning("Trying to register IDataSerializableFactory {Factory} with factory-id {FactoryId} " +
                                   "multiple times, skipping.", factory, factoryId);
            }
            else
            {
                throw new ArgumentException($"Cannot register IDataSerializableFactory {factory} with factory-id {factoryId} " +
                                            $"because factory {current} is already registered for that factory-id.");
            }
        }
    }
}
