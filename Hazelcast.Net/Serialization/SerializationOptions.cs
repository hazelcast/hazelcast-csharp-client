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
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using Hazelcast.Configuration.Binding;
using Hazelcast.Core;
using Hazelcast.Exceptions;

namespace Hazelcast.Serialization
{
    /// <summary>
    /// Contains the serialization options
    /// </summary>
    /// <remarks>
    /// <see cref="IIdentifiedDataSerializable"/>, <see cref="IPortable"/>, custom serializers, and global serializer can be configured using this config.
    /// </remarks>
    public sealed class SerializationOptions
    {
        /// <summary>
        /// Gets or sets the <see cref="Endianness"/>.
        /// </summary>
        public Endianness Endianness { get; set; } = Endianness.BigEndian;

        /// <summary>
        /// Whether to check for class definition errors at start,
        /// and throw an Serialization Exception with error definition.
        /// </summary>
        public bool CheckClassDefinitionErrors { get; set; } = true;

        /// <summary>
        /// Gets or sets the portable version.
        /// </summary>
        public int PortableVersion { get; set; }

        #region Class Definitions

        public ICollection<IClassDefinition> ClassDefinitions { get; private set; } = new HashSet<IClassDefinition>();

        #endregion

        #region Portable Factories

        /// <summary>
        /// Gets the portable factories.
        /// </summary>
        public ICollection<FactoryOptions<IPortableFactory>> PortableFactories { get; private set; } = new List<FactoryOptions<IPortableFactory>>();

        /// <summary>
        /// Adds an <see cref="IPortableFactory"/>.
        /// </summary>
        /// <param name="factoryId">The identifier of the factory.</param>
        /// <param name="factory">The factory.</param>
        /// <returns>The <see cref="SerializationOptions"/>.</returns>
        public SerializationOptions AddPortableFactory(int factoryId, IPortableFactory factory)
        {
            PortableFactories.Add(new FactoryOptions<IPortableFactory> { Id = factoryId, Creator = () => factory });
            return this;
        }

        /// <summary>
        /// Adds an <see cref="IPortableFactory"/>.
        /// </summary>
        /// <param name="factoryId">The identifier of the factory.</param>
        /// <param name="factoryType">The type of the factory</param>
        /// <returns>The <see cref="SerializationOptions"/>.</returns>
        public SerializationOptions AddPortableFactory(int factoryId, Type factoryType)
        {
            PortableFactories.Add(new FactoryOptions<IPortableFactory> { Id = factoryId, Creator = () => Services.CreateInstance<IPortableFactory>(factoryType) });
            return this;
        }

        /// <summary>
        /// Adds an <see cref="IPortableFactory"/>.
        /// </summary>
        /// <param name="factoryId">The identifier of the factory.</param>
        /// <param name="factoryTypeName">The type name of the factory</param>
        /// <returns>The <see cref="SerializationOptions"/>.</returns>
        public SerializationOptions AddPortableFactory(int factoryId, string factoryTypeName)
        {
            PortableFactories.Add(new FactoryOptions<IPortableFactory> { Id = factoryId, Creator = () => Services.CreateInstance<IPortableFactory>(factoryTypeName) });
            return this;
        }

        #endregion

        #region Data Serializable Factories

        public ICollection<FactoryOptions<IDataSerializableFactory>> DataSerializableFactories { get; private set; } = new List<FactoryOptions<IDataSerializableFactory>>();

        /// <summary>
        /// Adds an <see cref="IDataSerializableFactory"/>.
        /// </summary>
        /// <param name="factoryId">The identifier of the factory.</param>
        /// <param name="factory">The factory.</param>
        /// <returns>The <see cref="SerializationOptions"/>.</returns>
        public SerializationOptions AddDataSerializableFactory(int factoryId, IDataSerializableFactory factory)
        {
            DataSerializableFactories.Add(new FactoryOptions<IDataSerializableFactory> { Id = factoryId, Creator = () => factory });
            return this;
        }

        /// <summary>
        /// Adds an <see cref="IDataSerializableFactory"/>.
        /// </summary>
        /// <param name="factoryId">The identifier of the factory.</param>
        /// <param name="factoryTypeName">The type name of the factory</param>
        /// <returns>The <see cref="SerializationOptions"/>.</returns>
        public SerializationOptions AddDataSerializableFactoryClass(int factoryId, string factoryTypeName)
        {
            DataSerializableFactories.Add(new FactoryOptions<IDataSerializableFactory> { Id = factoryId, Creator = () => Services.CreateInstance<IDataSerializableFactory>(factoryTypeName) });
            return this;
        }

        /// <summary>
        /// Adds an <see cref="IDataSerializableFactory"/>.
        /// </summary>
        /// <param name="factoryId">The identifier of the factory.</param>
        /// <param name="factoryType">The type of the factory</param>
        /// <returns>The <see cref="SerializationOptions"/>.</returns>
        public SerializationOptions AddDataSerializableFactoryClass(int factoryId, Type factoryType)
        {
            DataSerializableFactories.Add(new FactoryOptions<IDataSerializableFactory> { Id = factoryId, Creator = () => Services.CreateInstance<IDataSerializableFactory>(factoryType) });
            return this;
        }

        #endregion

        #region Serializers

        public SerializerOptions DefaultSerializer { get; set; }

        public ICollection<SerializerOptions> Serializers { get; private set; } = new List<SerializerOptions>();

        #endregion

        /// <summary>
        /// Clones the options.
        /// </summary>
        public SerializationOptions Clone()
        {
            return new SerializationOptions
            {
                Endianness = Endianness,
                CheckClassDefinitionErrors = CheckClassDefinitionErrors,
                PortableVersion = PortableVersion,

                ClassDefinitions = new List<IClassDefinition>(ClassDefinitions),
                PortableFactories = new List<FactoryOptions<IPortableFactory>>(PortableFactories),
                DataSerializableFactories = new List<FactoryOptions<IDataSerializableFactory>>(DataSerializableFactories),

                DefaultSerializer = DefaultSerializer.Clone(),
                Serializers = new List<SerializerOptions>(Serializers)
            };
        }
    }
}
