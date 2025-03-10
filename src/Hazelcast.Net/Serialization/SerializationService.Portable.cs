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
using System.Collections.Generic;

namespace Hazelcast.Serialization
{
    internal partial class SerializationService
    {
        /// <summary>
        /// (internal for tests only) Gets the portable context.
        /// </summary>
        internal IPortableContext GetPortableContext() => _portableContext;

        /// <summary>
        /// (internal for tests only) Gets the portable serializer.
        /// </summary>
        internal PortableSerializer PortableSerializer => _portableSerializer;

        /// <summary>
        /// (internal for tests only) Creates a portable reader.
        /// </summary>
        internal IPortableReader CreatePortableReader(IData data)
        {
            if (!data.IsPortable)
            {
                throw new ArgumentException("Given data is not Portable! -> " + data.TypeId);
            }

            ObjectDataInput input = null;
            IPortableReader reader;
            try
            {
                input = CreateObjectDataInput(data);
                reader = _portableSerializer.CreateReader(input);
                input = null;
            }
            finally
            {
#pragma warning disable CA1508 // Avoid dead conditional code - false positive
                input?.Dispose();
#pragma warning restore CA1508
            }

            return reader;
        }

        /// <summary>
        /// Registers a collection of portable class definitions in the portable context.
        /// </summary>
        /// <param name="definitions">The definitions.</param>
        /// <param name="validate">Whether to validate the definitions.</param>
        /// <exception cref="SerializationException">Two or more definitions have the same class-id, or <paramref name="validate"/>
        /// was <c>true</c> and some fields are of an undefined portable type.</exception>
        private void RegisterPortableClassDefinitions(ICollection<IClassDefinition> definitions, bool validate)
        {
            IDictionary<int, IClassDefinition> map = new Dictionary<int, IClassDefinition>(definitions.Count);

            // create the class-id -> class-definition map, ensure ids are unique
            foreach (var definition in definitions)
            {
                if (map.ContainsKey(definition.ClassId))
                    throw new SerializationException($"Duplicate IClassDefinition found for class-id[{definition.ClassId}].");
                map.Add(definition.ClassId, definition);
            }

            // validate that portable fields are all defined (and it's naturally recursive)
            if (validate)
            {
                foreach (var definition in definitions)
                foreach (var field in definition.GetPortableFields())
                {
                    if (!map.ContainsKey(field.ClassId))
                        throw new SerializationException($"Could not find registered IClassDefinition for class-id {field.ClassId}.");
                }
            }

            // register into portable context
            foreach (var definition in definitions)
                _portableContext.RegisterClassDefinition(definition);
        }
    }

    internal static class ClassDefinitionExtensions
    {
        public static IEnumerable<IFieldDefinition> GetPortableFields(this IClassDefinition definition)
        {
            for (var i = 0; i < definition.GetFieldCount(); i++)
            {
                var field = definition.GetField(i);
                if (field.FieldType == FieldType.Portable || field.FieldType == FieldType.PortableArray)
                    yield return definition.GetField(i);
            }
        }
    }
}
