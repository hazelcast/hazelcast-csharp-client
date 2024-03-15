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
using System.Linq;
using Hazelcast.Serialization.Compact;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Serialization
{
    internal partial class SerializationService
    {
        /// <summary>
        /// Looks up a serializer for a type-id.
        /// </summary>
        /// <param name="typeId">The type-id.</param>
        /// <returns>The serializer for the <paramref name="typeId"/>.</returns>
        /// <exception cref="SerializationException">Cannot find a serializer matching the <paramref name="typeId"/>.</exception>
        private ISerializerAdapter LookupSerializer(int typeId)
        {
            return typeId > 0 ? LookupCustomSerializer(typeId) : LookupConstantSerializer(typeId);
        }

        /// <summary>
        /// Looks up a constant serializer for a negative type-id.
        /// </summary>
        /// <param name="typeId">The type-id.</param>
        /// <returns>The serializer for the <paramref name="typeId"/>.</returns>
        /// <exception cref="SerializationException">Cannot find a constant serializer matching the <paramref name="typeId"/>.</exception>
        private ISerializerAdapter LookupConstantSerializer(int typeId)
        {
            // NOTE
            // In Java the javaSerializerAdapter, which would correspond to the _serializableSerializerAdapter here,
            // is somehow registered as a custom serializer and therefore is excluded here. See the note in the
            // SerializationService constructor: I fail to understand the reason for this and revert to registering
            // the _serializableSerializerAdapter as constant.
            // Which means we don't need to exclude it here. But I keep the code commented out as a reference.

            ISerializerAdapter adapter = null;
            if (-typeId < ConstantSerializersCount /*&& typeId != SerializationConstants.CsharpClrSerializationType*/)
                adapter = _constantById[-typeId]; // type-id key is >0 because _constantById is an array

            if (adapter != null) return adapter;

            throw new SerializationException($"Could not find a serializer for type {typeId}.");
        }

        /// <summary>
        /// Looks up a custom serializer for a positive type-id.
        /// </summary>
        /// <param name="typeId">The type-id.</param>
        /// <returns>The serializer for the <paramref name="typeId"/>.</returns>
        /// <exception cref="SerializationException">Cannot find a custom serializer matching the <paramref name="typeId"/>.</exception>
        private ISerializerAdapter LookupCustomSerializer(int typeId)
        {
            if (_customById.TryGetValue(typeId, out var result)) return result;

            throw new SerializationException($"Could not find a serializer for type {typeId}.");
        }

        /// <summary>
        /// Looks up a serializer for an object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="withSchemas">Whether to embed all schemas in the serialized data.</param>
        /// <returns>The serializer for the object.</returns>
        /// <exception cref="SerializationException">Cannot find a custom serializer for the <paramref name="obj"/>.</exception>
        private ISerializerAdapter LookupSerializer(object obj, bool withSchemas)
        {
            if (obj == null) return _nullSerializerAdapter;             // 1. NULL serializer

            var typeOfObj = obj.GetType();

            var serializer = LookupKnownSerializer(typeOfObj, withSchemas) ??   // 2a. compact, identified, portable
                             LookupConstantSerializer(typeOfObj) ??             // 2b. primitive, string, etc
                             LookupCustomSerializer(typeOfObj) ??               // 3.  custom, registered by user
                             LookupSerializableSerializer(typeOfObj) ??         // 4.  .NET BinaryFormatter for [Serializable] types
                             LookupGlobalSerializer(typeOfObj) ??               // 5.  global, if registered by user
                             LookupCompactSerializer(typeOfObj, withSchemas);   // 6.  compact

            // NOTE:
            // for 4, _enableClrSerialization must be true
            //        i.e. SerializationOptions.EnableClrSerialization && !globalSerializer.OverrideClrSerialization

            if (serializer != null) return serializer;

            throw new SerializationException($"Could not find a serializer for type {typeOfObj}.");
        }

        private ISerializerAdapter LookupKnownSerializer(Type type, bool withSchemas)
        {
            // fast path for some known serializers

            if (typeof(CompactGenericRecordBase).IsAssignableFrom(type))
                return withSchemas ? _compactSerializerWithSchemasAdapter : _compactSerializerAdapter;

            if (_compactSerializer.HasRegistrationForType(type))
                return withSchemas ? _compactSerializerWithSchemasAdapter : _compactSerializerAdapter;

            if (typeof(IIdentifiedDataSerializable).IsAssignableFrom(type))
                return _dataSerializerAdapter;

            if (typeof(IPortable).IsAssignableFrom(type))
                return _portableSerializerAdapter;

            return null;
        }

        private ISerializerAdapter LookupConstantSerializer(Type type)
        {
            return _constantByType.TryGetValue(type, out var serializer)
                ? serializer
                : null;
        }

        private ISerializerAdapter LookupCustomSerializer(Type type)
        {
            // direct lookup
            if (_customByType.TryGetValue(type, out var serializer)) return serializer;

            bool TryLookupSerializer(Type lookupType, Type forType, out ISerializerAdapter foundSerializer)
            {
                if (!_customByType.TryGetValue(lookupType, out foundSerializer)) return false;

                // register so we find it faster next time
                _ = TryRegisterCustomSerializer(foundSerializer, forType);
                return true;
            }

            // try its interfaces
            if (type.GetInterfaces().Any(lookupType => TryLookupSerializer(lookupType, type, out serializer)))
                return serializer;

            // try its hierarchy
            for (var lookupType = type.BaseType; lookupType != null; lookupType = lookupType.BaseType)
                if (TryLookupSerializer(lookupType, type, out serializer))
                    return serializer;

            return null;
        }

        private ISerializerAdapter LookupSerializableSerializer(Type type)
        {
            if (!_enableClrSerialization) return null;
            if (!type.IsSerializable) return null;

            // register so we find it faster next time
            if (TryRegisterConstantSerializer(_serializableSerializerAdapter, type))
            {
                // with a warning
                _logger.LogWarning("Performance hint: Serialization service will use the CLR serialization " +
                                   $"for type {type}. Please consider using a faster serialization option such as " +
                                   "IIdentifiedDataSerializable.");
            }

            return _serializableSerializerAdapter;
        }

        private ISerializerAdapter LookupGlobalSerializer(Type type)
        {
            var serializer = _globalSerializerAdapter;

            // register so we find it faster next time
            if (serializer != null) _ = TryRegisterCustomSerializer(serializer, type);

            return serializer;
        }

        private ISerializerAdapter LookupCompactSerializer(Type type, bool withSchemas)
        {
            // don't register when using withSchemas
            if (withSchemas) return _compactSerializerWithSchemasAdapter;

            // register so we find it faster next time
            _ = TryRegisterConstantSerializer(_compactSerializerAdapter, type);
            return _compactSerializerAdapter;
        }
    }
}
