// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
    /// <summary>
    ///     This class is the default serializer for all types that are serialized using Hazelcast
    ///     internal methods.
    /// </summary>
    /// <remarks>
    ///     This class is the default serializer for all types that are serialized using Hazelcast
    ///     internal methods.
    ///     If the way the DataSerializer serializes values is changed the extract method needs to be changed too!
    /// </remarks>
    internal sealed class DataSerializer : IStreamSerializer<IIdentifiedDataSerializable>
    {
        private readonly ILogger _logger;

        private readonly IDictionary<int, IDataSerializableFactory> _factories =
            new Dictionary<int, IDataSerializableFactory>();

        internal DataSerializer(SerializerHooks hooks, IEnumerable<KeyValuePair<int, IDataSerializableFactory>> dataSerializableFactories, ILoggerFactory loggerFactory )
        {
            _logger = loggerFactory.CreateLogger<DataSerializer>();

            RegisterHooks(hooks);
            if (dataSerializableFactories != null)
            {
                foreach (var entry in dataSerializableFactories)
                {
                    Register(entry.Key, entry.Value);
                }
            }
        }

        private void RegisterHooks(SerializerHooks hooks)
        {
            foreach (var hook in hooks.Hooks)
                RegisterHook(hook);
        }

        public int TypeId => SerializationConstants.ConstantTypeDataSerializable;

        /// <exception cref="System.IO.IOException"></exception>
        public IIdentifiedDataSerializable Read(IObjectDataInput input)
        {
            var factoryId = 0;
            var id = 0;
            try
            {
                var identified = input.ReadBoolean();
                if (!identified)
                    throw new SerializationException("Non-identified DataSerializable is not supported by .NET client. " +
                                                              "Please use IIdentifiedDataSerializable instead.");

                factoryId = input.ReadInt();
                if (!_factories.TryGetValue(factoryId, out var factory))
                    throw new SerializationException($"No DataSerializerFactory registered with id: {factoryId}.");

                id = input.ReadInt();
                var serializable = factory.Create(id);
                if (serializable == null)
                    throw new SerializationException($"{factory} (factoryId: {factoryId}) failed to create an instance for id: {id}.");

                serializable.ReadData(input);
                return serializable;
            }
            catch (IOException) { throw; }
            catch (SerializationException) { throw; }
            catch (Exception e)
            {
                throw new SerializationException($"Failed to read DataSerializable with factoryId: {factoryId}, id: {id} ({e.GetType()}: {e.Message}).", e);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
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

        private void RegisterHook(IDataSerializerHook hook)
        {
            Register(hook.FactoryId, hook.CreateFactory());
        }

        private void Register(int factoryId, IDataSerializableFactory factory)
        {
            _factories.TryGetValue(factoryId, out var current);
            if (current != null)
            {
                if (current.Equals(factory))
                {
                    _logger.LogWarning("DataSerializableFactory[" + factoryId + "] is already registered! Skipping " +
                                   factory);
                }
                else
                {
                    throw new ArgumentException("DataSerializableFactory[" + factoryId + "] is already registered! " +
                                                current + " -> " + factory);
                }
            }
            else
            {
                _factories.Add(factoryId, factory);
            }
        }
    }
}
