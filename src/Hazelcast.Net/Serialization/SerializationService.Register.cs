// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Serialization
{
    internal partial class SerializationService
    {
        #region RegisterConstantSerializer

        private static void EnsureValidConstantSerializer(ISerializerAdapter adapter)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));
            if (adapter.TypeId > 0) throw new ArgumentException("Constant serializer type-id must be <=0.", nameof(adapter));
            if (-adapter.TypeId >= ConstantSerializersCount) throw new ArgumentException($"Constant serializer type-id must be >-{ConstantSerializersCount}.", nameof(adapter));
        }

        /// <summary>
        /// Registers a constant serializer (by type and negative type-id).
        /// </summary>
        /// <param name="adapter">The serializer.</param>
        /// <param name="type">The type.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="adapter"/> or the <paramref name="type"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="adapter"/> has a positive type-id.</exception>
        private void RegisterConstantSerializer(ISerializerAdapter adapter, Type type)
        {
            EnsureValidConstantSerializer(adapter);
            if (type == null) throw new ArgumentNullException(nameof(type));

            _constantByType.AddOrUpdate(type, adapter, (_, _) => adapter);
            _constantById[-adapter.TypeId] = adapter; // type-id key is >0 because _constantById is an array
        }

        /// <summary>
        /// Registers a constant serializer (by negative type-id).
        /// </summary>
        /// <param name="adapter">The serializer.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="adapter"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="adapter"/> has a positive type-id.</exception>
        private void RegisterConstantSerializer(ISerializerAdapter adapter)
        {
            EnsureValidConstantSerializer(adapter);

            _constantById[-adapter.TypeId] = adapter; // type-id key is >0 because _constantById is an array
        }

        /// <summary>
        /// Registers a constant serializer (by type and negative type-id).
        /// </summary>
        /// <typeparam name="TSerialized">The type.</typeparam>
        /// <param name="serializer">The serializer.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="serializer"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="serializer"/> has a positive type-id.</exception>
        public void RegisterConstantSerializer<TSerialized>(ISerializer serializer)
            => RegisterConstantSerializer(CreateSerializerAdapter<TSerialized>(serializer), typeof(TSerialized));

        /// <summary>
        /// Tries to register a constant serializer (by type and negative type-id).
        /// </summary>
        /// <param name="adapter">The serializer.</param>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the serializer was registered; <c>false</c> if it was already registered.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="adapter"/> or <paramref name="type"/> is null.</exception>
        /// <exception cref="ArgumentException">The <paramref name="adapter"/> has a positive type-id.</exception>
        /// <exception cref="InvalidOperationException">Another serializer is already registered for the type or type-id.</exception>
        private bool TryRegisterConstantSerializer(ISerializerAdapter adapter, Type type)
        {
            EnsureValidConstantSerializer(adapter);
            if (type == null) throw new ArgumentNullException(nameof(type));

            // it is OK to register a constant serializer for a new type,
            // but the id and serializer instance need to match exactly
            if (!ReferenceEquals(_constantById[-adapter.TypeId], adapter))
                throw new ArgumentException($"Cannot register serializer {adapter.Serializer} for type-id {adapter.TypeId} because serializer {_constantById[-adapter.TypeId].Serializer} has already been registered for that type-id.");

            if (_constantByType.TryAdd(type, adapter)) return true;

            var existing = _constantByType[type];
            if (existing.Serializer.GetType() == adapter.Serializer.GetType()) return false;

            throw new InvalidOperationException($"Cannot register serializer {adapter.Serializer} for type-id {adapter.TypeId} because serializer {existing.Serializer} has already been registered for that type-id.");
        }

        #endregion

        #region RegisterCustomSerializer

        /// <summary>
        /// Tries to register a custom serializer (by type and positive type-id).
        /// </summary>
        /// <param name="adapter">The serializer.</param>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the serializer was registered; <c>false</c> if it was already registered.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="adapter"/> or <paramref name="type"/> is null.</exception>
        /// <exception cref="ArgumentException">The <paramref name="adapter"/> has a negative type-id.</exception>
        /// <exception cref="InvalidOperationException">Another serializer is already registered for the type or type-id.</exception>
        private bool TryRegisterCustomSerializer(ISerializerAdapter adapter, Type type)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (adapter.TypeId <= 0) throw new ArgumentException($"Custom serializer {adapter.Serializer} cannot have a negative type-id ({adapter.TypeId}).");

            // make sure we are not overriding a constant serializer by type
            if (_constantByType.ContainsKey(type))
                throw new ArgumentException($"Custom serializer cannot be registered for constant type {type}.");

            var byType = _customByType.TryAdd(type, adapter);
            var byId = _customById.TryAdd(adapter.TypeId, adapter);

            if (byType && byId) return true;

            if (!byType)
            {
                var existing = _customByType[type];
                if (existing.Serializer.GetType() != adapter.Serializer.GetType())
                {
                    if (byId) _customById.TryRemove(adapter.TypeId, out _);
                    throw new InvalidOperationException($"Cannot register serializer {adapter.Serializer} for type-id {adapter.TypeId} because serializer {existing.Serializer} has already been registered for that type-id.");
                }
            }

            if (!byId)
            {
                var existing = _customByType[type];
                if (existing.Serializer.GetType() != adapter.Serializer.GetType())
                {
                    if (byType) _customByType.TryRemove(type, out _);
                    throw new InvalidOperationException($"Cannot register serializer {adapter.Serializer} for type {type} because serializer {existing.Serializer} has already been registered for that type.");
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to register a custom serializer (by positive type-id).
        /// </summary>
        /// <param name="adapter">The serializer.</param>
        /// <returns><c>true</c> if the serializer was registered; <c>false</c> if it was already registered.</returns>
        /// <exception cref="ArgumentNullException">The serializer is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="adapter"/> has a negative type-id.</exception>
        /// <exception cref="InvalidOperationException">Another serializer is already registered for the type-id.</exception>
        private bool TryRegisterCustomSerializer(ISerializerAdapter adapter)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));
            if (adapter.TypeId <= 0) throw new ArgumentException($"Custom serializer {adapter.Serializer} cannot have a negative type-id ({adapter.TypeId}).");

            if (_customById.TryAdd(adapter.TypeId, adapter)) return true;

            var existing = _customById[adapter.TypeId];
            if (existing.Serializer.GetType() == adapter.Serializer.GetType()) return false;

            throw new InvalidOperationException($"Cannot register serializer {adapter.Serializer} for type-id {adapter.TypeId} because serializer {existing.Serializer} has already been registered for that type-id.");
        }

        /// <summary>
        /// Registers a custom serializer (by type and positive type-id).
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <param name="type">The type.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="serializer"/> or <paramref name="type"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="serializer"/> has a negative type-id.</exception>
        /// <exception cref="InvalidOperationException">Another serializer is already registered for the type or type-id.</exception>
        private void RegisterCustomSerializer(ISerializer serializer, Type type)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            if (type == null) throw new ArgumentNullException(nameof(type));

            TryRegisterCustomSerializer(CreateSerializerAdapter(type, serializer), type);
        }

        #endregion
    }
}
