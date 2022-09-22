// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Text;
using Hazelcast.Configuration;
using Hazelcast.Configuration.Binding;
using Hazelcast.Core;
using Hazelcast.Serialization.Compact;

// suppress warning about IPortableFactory not being disposed
// TODO: should HazelcastOptions be IDisposable so in the end we dispose these factories?
#pragma warning disable CA2000 // Dispose objects before losing scope

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

            Compact = new CompactOptions();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationOptions"/> class.
        /// </summary>
        private SerializationOptions(SerializationOptions other)
        {
            Endianness = other.Endianness;
            ValidateClassDefinitions = other.ValidateClassDefinitions;
            PortableVersion = other.PortableVersion;
            EnableClrSerialization = other.EnableClrSerialization;

            Compact = other.Compact.Clone();

            ClassDefinitions = new HashSet<IClassDefinition>(other.ClassDefinitions);
            PortableFactories = new List<FactoryOptions<IPortableFactory>>(other.PortableFactories);
            DataSerializableFactories = new List<FactoryOptions<IDataSerializableFactory>>(other.DataSerializableFactories);

            GlobalSerializer = other.GlobalSerializer?.Clone();
            Serializers = new List<SerializerOptions>(other.Serializers);
        }

        /// <summary>
        /// Whether to enable CLR serialization via <see cref="BinaryFormatter"/>.
        /// </summary>
        /// <remarks>
        /// <para><see cref="BinaryFormatter"/> is now considered insecure, and CLR serialization
        /// is disabled by default. In order to enable CLR serialization, set this value to <c>true</c>.
        /// Note that if a global serializer is configured via <see cref="GlobalSerializer"/>, then this
        /// option must be true, and <see cref="GlobalSerializerOptions.OverrideClrSerialization"/> must
        /// be false, for CLR serialization to be enabled.</para>
        /// </remarks>
        public bool EnableClrSerialization { get; set; } = true; // otherwise, breaking change

        /// <summary>
        /// Gets or sets the <see cref="Endianness"/>. This value should match the server configuration.
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

        /// <summary>
        /// Gets the compact serialization options.
        /// </summary>
        public CompactOptions Compact { get; }

        #region Class Definitions

        /// <summary>
        /// Gets the collection of <see cref="IClassDefinition"/>.
        /// </summary>
        /// <remarks>
        /// <para>This can only be done programmatically.</para>
        /// </remarks>
        public ICollection<IClassDefinition> ClassDefinitions { get; } = new HashSet<IClassDefinition>();

        #endregion

        #region Portable Factories

        /// <summary>
        /// Gets the collection of <see cref="FactoryOptions{T}"/> of <see cref="IPortableFactory"/>.
        /// </summary>
        [BinderIgnore]
        public ICollection<FactoryOptions<IPortableFactory>> PortableFactories { get; }

#pragma warning disable IDE0052 // Remove unread private members - used by binding
        [BinderIgnore(false)]
        [BinderName("portableFactories")]
        private CollectionBinder<IdentifiedInjectionOptions> PortableFactoriesBinder { get; }
#pragma warning restore IDE0052 // Remove unread private members

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

        /// <summary>
        /// Gets the collection of <see cref="FactoryOptions{T}"/> of <see cref="IDataSerializableFactory"/>.
        /// </summary>
        [BinderIgnore]
        public ICollection<FactoryOptions<IDataSerializableFactory>> DataSerializableFactories { get; }

#pragma warning disable IDE0052 // Remove unread private members - used by binding
        [BinderIgnore(false)]
        [BinderName("dataSerializableFactories")]
        private CollectionBinder<IdentifiedInjectionOptions> DataSerializableFactoriesBinder { get; }
#pragma warning restore IDE0052 // Remove unread private members

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

        /// <summary>
        /// Gets the <see cref="GlobalSerializerOptions"/>.
        /// </summary>
        /// <remarks>
        /// <para>When defined in a configuration file, it is defined as an injected type, for instance:
        /// <code>
        /// "globalSerializer":
        /// {
        ///   "typeName": "My.Serializer",
        ///   "args":
        ///   {
        ///     "foo": 42
        ///   },
        ///   "overrideClrSerialization": true
        /// }
        /// </code>
        /// with the additional <c>overrideClrSerialization</c> property.</para>
        /// </remarks>
        [BinderIgnore]
        public GlobalSerializerOptions GlobalSerializer { get; set; } = new GlobalSerializerOptions();

#pragma warning disable IDE0051 // Remove unused private members - used by binding
        [BinderIgnore(false)]
        [BinderName("globalSerializer")]
        private GlobalSerializerInjectionOptions GlobalSerializerBinder
        {
            get => default;
            set
            {
                GlobalSerializer = new GlobalSerializerOptions
                {
                    OverrideClrSerialization = value.OverrideClrSerialization,
                    Creator = () => ServiceFactory.CreateInstance<ISerializer>(value.TypeName, value.Args)
                };
            }
        }
#pragma warning restore IDE0051 // Remove unused private members

        /// <summary>
        /// Gets the collection of <see cref="SerializerOptions"/>.
        /// </summary>
        /// <remarks>
        /// <para></para>
        /// </remarks>
        [BinderIgnore]
        public ICollection<SerializerOptions> Serializers { get; }

#pragma warning disable IDE0052 // Remove unread private members - used by binding
        [BinderIgnore(false)]
        [BinderName("serializers")]
        private CollectionBinder<SerializerInjectionOptions> SerializersBinder { get; }
#pragma warning restore IDE0052 // Remove unread private members

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
                text.Append('\'');
            }
        }

        internal class GlobalSerializerInjectionOptions : InjectionOptions
        {
            public bool OverrideClrSerialization { get; set; }

            protected override void ToString(StringBuilder text)
            {
                base.ToString(text);
                text.Append(", overrideClr: ");
                text.Append(OverrideClrSerialization);
            }
        }

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal SerializationOptions Clone() => new SerializationOptions(this);
    }
}
