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
using Hazelcast.Configuration;
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
        /// Initializes a new instance of the <see cref="SerializationOptions"/> class.
        /// </summary>
        public SerializationOptions()
        {
            PortableFactories = new List<FactoryOptions<IPortableFactory>>();
            PortableFactoriesBinder = new CollectionBinder<IdentifiedInjectionOptions>(item =>
            {
                PortableFactories.Add(new FactoryOptions<IPortableFactory>
                {
                    Id = item.Id,
                    Creator = () => ServiceFactory.CreateInstance<IPortableFactory>(item.TypeName, item.Args)
                });
            });
            
            DataSerializableFactories = new List<FactoryOptions<IDataSerializableFactory>>();
            DataSerializableFactoriesBinder = new CollectionBinder<IdentifiedInjectionOptions>(item =>
            {
                DataSerializableFactories.Add(new FactoryOptions<IDataSerializableFactory>
                {
                    Id = item.Id,
                    Creator = () => ServiceFactory.CreateInstance<IDataSerializableFactory>(item.TypeName, item.Args)
                });
            });
            
            Serializers = new List<SerializerOptions>();
            SerializersBinder = new CollectionBinder<SerializerInjectionOptions>(item =>
            {
                Serializers.Add(new SerializerOptions
                {
                    SerializedType = Type.GetType(item.SerializedTypeName) ?? throw new ConfigurationException($"Unknown serialized type \"{item.SerializedTypeName}\"."),
                    Creator = () => ServiceFactory.CreateInstance<ISerializer>(item.TypeName, item.Args)
                });
            });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationOptions"/> class.
        /// </summary>
        private SerializationOptions(SerializationOptions other)
        {
            Endianness = other.Endianness;
            ValidateClassDefinitions = other.ValidateClassDefinitions;
            PortableVersion = other.PortableVersion;

            ClassDefinitions = new List<IClassDefinition>(other.ClassDefinitions);
            PortableFactories = new List<FactoryOptions<IPortableFactory>>(other.PortableFactories);
            DataSerializableFactories = new List<FactoryOptions<IDataSerializableFactory>>(other.DataSerializableFactories);

            DefaultSerializer = other.DefaultSerializer?.Clone();
            Serializers = new List<SerializerOptions>(other.Serializers);
        }

        /// <summary>
        /// Gets or sets the <see cref="Endianness"/>.
        /// </summary>
        public Endianness Endianness { get; set; } = Endianness.BigEndian;

        /// <summary>
        /// Whether to check for class definition errors at start,
        /// and throw an Serialization Exception with error definition.
        /// </summary>
        public bool ValidateClassDefinitions { get; set; } = true;

        /// <summary>
        /// Gets or sets the portable version.
        /// </summary>
        public int PortableVersion { get; set; }

        #region Class Definitions

        public ICollection<IClassDefinition> ClassDefinitions { get; } = new HashSet<IClassDefinition>();

        #endregion

        #region Portable Factories

        /// <summary>
        /// Gets the portable factories.
        /// </summary>
        [BinderIgnore]
        public ICollection<FactoryOptions<IPortableFactory>> PortableFactories { get; }
        
        [BinderIgnore(false)]
        [BinderName("portableFactories")]
        private CollectionBinder<IdentifiedInjectionOptions> PortableFactoriesBinder { get; }

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
            PortableFactories.Add(new FactoryOptions<IPortableFactory> { Id = factoryId, Creator = () => ServiceFactory.CreateInstance<IPortableFactory>(factoryType, null) });
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
            PortableFactories.Add(new FactoryOptions<IPortableFactory> { Id = factoryId, Creator = () => ServiceFactory.CreateInstance<IPortableFactory>(factoryTypeName, null) });
            return this;
        }

        #endregion

        #region Data Serializable Factories

        [BinderIgnore]
        public ICollection<FactoryOptions<IDataSerializableFactory>> DataSerializableFactories { get; }
        
        [BinderIgnore(false)]
        [BinderName("dataSerializableFactories")]
        private CollectionBinder<IdentifiedInjectionOptions> DataSerializableFactoriesBinder { get; }

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
            DataSerializableFactories.Add(new FactoryOptions<IDataSerializableFactory> { Id = factoryId, Creator = () => ServiceFactory.CreateInstance<IDataSerializableFactory>(factoryTypeName, null) });
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
            DataSerializableFactories.Add(new FactoryOptions<IDataSerializableFactory> { Id = factoryId, Creator = () => ServiceFactory.CreateInstance<IDataSerializableFactory>(factoryType, null) });
            return this;
        }

        #endregion

        #region Serializers

        [BinderIgnore]
        public SerializerOptions DefaultSerializer { get; set; }

        [BinderIgnore(false)]
        [BinderName("defaultSerializer")]
        private DefaultSerializerInjectionOptions DefaultSerializerBinder
        {
            get => default;
            set
            {
                DefaultSerializer = new SerializerOptions
                {
                    OverrideClr = value.OverrideClr,
                    Creator = () => ServiceFactory.CreateInstance<ISerializer>(value.TypeName, value.Args)
                };
            }
        }

        [BinderIgnore]
        public ICollection<SerializerOptions> Serializers { get; }
        
        [BinderIgnore(false)]
        [BinderName("serializers")]
        private CollectionBinder<SerializerInjectionOptions> SerializersBinder { get; }

        #endregion

        internal class IdentifiedInjectionOptions : InjectionOptions
        {
            public int Id { get; set; }

            protected override void ToString(StringBuilder text)
            {
                base.ToString(text);
                text.Append(", id: ");
                text.Append(Id);
            }
        }
        
        internal class SerializerInjectionOptions : InjectionOptions
        {
            public string SerializedTypeName { get; set; }
            
            protected override void ToString(StringBuilder text)
            {
                base.ToString(text);
                text.Append(", serializedTypeName: '");
                text.Append(SerializedTypeName ?? "<null>");
                text.Append("'");
            }
        }

        internal class DefaultSerializerInjectionOptions : InjectionOptions
        {
            public bool OverrideClr { get; set; }
            
            protected override void ToString(StringBuilder text)
            {
                base.ToString(text);
                text.Append(", overrideClr: ");
                text.Append(OverrideClr);
            }
        }

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal SerializationOptions Clone() => new SerializationOptions(this);
    }
}
