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
using System.Text;
using System.Xml;
using Hazelcast.Core;

namespace Hazelcast.Serialization
{
    /// <summary>
    /// Contains the serialization configuration
    /// </summary>
    /// <remarks>
    /// <see cref="IIdentifiedDataSerializable"/>, <see cref="IPortable"/>, custom serializers, and global serializer can be configured using this config.
    /// </remarks>
    public class SerializationConfiguration
    {
        private Endianness _endianness = Endianness.BigEndian;
        private bool _checkClassDefErrors = true;
        private ICollection<IClassDefinition> _classDefinitions;
        private IDictionary<int, IDataSerializableFactory> _dataSerializableFactories;
        private IDictionary<int, string> _dataSerializableFactoryClasses;

        private GlobalSerializerConfig _globalSerializerConfig;
        private IDictionary<int, IPortableFactory> _portableFactories;
        private IDictionary<int, string> _portableFactoryClasses;
        private int _portableVersion;

        private ICollection<SerializerConfig> _serializerConfigs;

        private bool _useNativeByteOrder;

        /// <summary>
        /// Adds a <see cref="IPortable"/> class definition to be registered
        /// </summary>
        /// <param name="classDefinition"><see cref="IPortable"/> class definition</param>
        /// <returns>configured <see cref="SerializationConfiguration"/> for chaining</returns>
        public virtual SerializationConfiguration AddClassDefinition(IClassDefinition classDefinition)
        {
            if (GetClassDefinitions().Contains(classDefinition))
            {
                throw new ArgumentException("IClassDefinition for class-id[" + classDefinition.GetClassId() +
                                            "] already exists!");
            }
            GetClassDefinitions().Add(classDefinition);
            return this;
        }

        /// <summary>
        /// Adds a <see cref="IDataSerializableFactory"/> mapped with a factory id to be registered
        /// </summary>
        /// <param name="factoryId">factory ID of <see cref="IDataSerializableFactory"/> to be registered</param>
        /// <param name="dataSerializableFactory"><see cref="IDataSerializableFactory"/>object to be registered</param>
        /// <returns>configured <see cref="SerializationConfiguration"/> for chaining</returns>
        public virtual SerializationConfiguration AddDataSerializableFactory(int factoryId,
            IDataSerializableFactory dataSerializableFactory)
        {
            GetDataSerializableFactories().Add(factoryId, dataSerializableFactory);
            return this;
        }

        /// <summary>
        /// Adds a <see cref="IDataSerializableFactory"/> mapped with a factory id to be registered
        /// </summary>
        /// <param name="factoryId">factory ID of <see cref="IDataSerializableFactory"/> to be registered</param>
        /// <param name="dataSerializableFactoryClass">class name of the factory</param>
        /// <returns>configured <see cref="SerializationConfiguration"/> for chaining</returns>
        public virtual SerializationConfiguration AddDataSerializableFactoryClass(int factoryId,
            string dataSerializableFactoryClass)
        {
            //TODO dataSerializableFactoryClass paramtere extends ??? IDataSerializableFactory
            GetDataSerializableFactoryClasses().Add(factoryId, dataSerializableFactoryClass);
            return this;
        }

        /// <summary>
        /// Adds a <see cref="IDataSerializableFactory"/> mapped with a factory id to be registered
        /// </summary>
        /// <param name="factoryId">factory ID of <see cref="IDataSerializableFactory"/> to be registered</param>
        /// <param name="dataSerializableFactoryClass">Type of the factory</param>
        /// <returns>configured <see cref="SerializationConfiguration"/> for chaining</returns>
        public virtual SerializationConfiguration AddDataSerializableFactoryClass(int factoryId,
            Type dataSerializableFactoryClass)
        {
            var factoryClassType = dataSerializableFactoryClass ?? throw new ArgumentNullException(nameof(dataSerializableFactoryClass));
            var factoryClassName = factoryClassType.AssemblyQualifiedName;
            return AddDataSerializableFactoryClass(factoryId, factoryClassName);
        }

        /// <summary>
        /// Adds a <see cref="IPortableFactory"/> mapped with a factory id to be registered
        /// </summary>
        /// <param name="factoryId">factory ID of <see cref="IPortableFactory"/> to be registered</param>
        /// <param name="portableFactory">factory instance to be registered</param>
        /// <returns>configured <see cref="SerializationConfiguration"/> for chaining</returns>
        public virtual SerializationConfiguration AddPortableFactory(int factoryId, IPortableFactory portableFactory)
        {
            GetPortableFactories().Add(factoryId, portableFactory);
            return this;
        }

        /// <summary>
        /// Adds a <see cref="IPortableFactory"/> mapped with a factory id to be registered
        /// </summary>
        /// <param name="factoryId">factory ID of <see cref="IPortableFactory"/> to be registered</param>
        /// <param name="portableFactoryClass">Type of the factory</param>
        /// <returns>configured <see cref="SerializationConfiguration"/> for chaining</returns>
        public virtual SerializationConfiguration AddPortableFactoryClass(int factoryId, Type portableFactoryClass)
        {
            var factoryClassType = portableFactoryClass ?? throw new ArgumentNullException(nameof(portableFactoryClass));
            var factoryClassName = factoryClassType.AssemblyQualifiedName;
            return AddPortableFactoryClass(factoryId, factoryClassName);
        }

        /// <summary>
        /// Adds a <see cref="IPortableFactory"/> mapped with a factory id to be registered
        /// </summary>
        /// <param name="factoryId">factory ID of <see cref="IPortableFactory"/> to be registered</param>
        /// <param name="portableFactoryClass">class name of the factory</param>
        /// <returns>configured <see cref="SerializationConfiguration"/> for chaining</returns>
        public virtual SerializationConfiguration AddPortableFactoryClass(int factoryId, string portableFactoryClass)
        {
            GetPortableFactoryClasses().Add(factoryId, portableFactoryClass);
            return this;
        }

        /// <summary>
        /// Adds a <see cref="SerializationConfiguration"/>
        /// </summary>
        /// <param name="serializerConfig"><see cref="SerializationConfiguration"/></param>
        /// <returns>configured <see cref="SerializationConfiguration"/> for chaining</returns>
        public virtual SerializationConfiguration AddSerializerConfig(SerializerConfig serializerConfig)
        {
            GetSerializerConfigs().Add(serializerConfig);
            return this;
        }

        /// <summary>
        /// Gets the configured <see cref="ByteOrder"/>
        /// </summary>
        /// <returns><see cref="ByteOrder"/></returns>
        public virtual Endianness Endianness => _endianness;

        /// <summary>
        /// Gets all registered <see cref="IClassDefinition"/>s
        /// </summary>
        /// <returns>collection of <see cref="IClassDefinition"/></returns>
        public virtual ICollection<IClassDefinition> GetClassDefinitions()
        {
            if (_classDefinitions == null)
            {
                _classDefinitions = new HashSet<IClassDefinition>();
            }
            return _classDefinitions;
        }

        /// <summary>
        /// Gets the dictionary of factoryId to <see cref="IDataSerializableFactory"/> mapping
        /// </summary>
        /// <returns>dictionary of <see cref="IDataSerializableFactory"/></returns>
        public virtual IDictionary<int, IDataSerializableFactory> GetDataSerializableFactories()
        {
            if (_dataSerializableFactories == null)
            {
                _dataSerializableFactories = new Dictionary<int, IDataSerializableFactory>();
            }
            return _dataSerializableFactories;
        }

        /// <summary>
        /// Gets the dictionary of factoryId to <see cref="IDataSerializableFactory"/> class names mapping
        /// </summary>
        /// <returns>dictionary of factory ID and corresponding IDataSerializableFactory names</returns>
        public virtual IDictionary<int, string> GetDataSerializableFactoryClasses()
        {
            if (_dataSerializableFactoryClasses == null)
            {
                _dataSerializableFactoryClasses = new Dictionary<int, string>();
            }
            return _dataSerializableFactoryClasses;
        }

        /// <summary>
        /// Gets <see cref="GlobalSerializerConfig"/>
        /// </summary>
        /// <returns><see cref="GlobalSerializerConfig"/></returns>
        public virtual GlobalSerializerConfig GetGlobalSerializerConfig()
        {
            return _globalSerializerConfig;
        }

        /// <summary>
        /// Gets the dictionary of factoryId to <see cref="IPortableFactory"/> mapping
        /// </summary>
        /// <returns>dictionary of <see cref="IPortableFactory"/></returns>
        public virtual IDictionary<int, IPortableFactory> GetPortableFactories()
        {
            if (_portableFactories == null)
            {
                _portableFactories = new Dictionary<int, IPortableFactory>();
            }
            return _portableFactories;
        }

        /// <summary>
        /// Gets the dictionary of factoryId to <see cref="IPortableFactory"/> class names mapping
        /// </summary>
        /// <returns>dictionary of factory ID and corresponding portable factory names</returns>
        public virtual IDictionary<int, string> GetPortableFactoryClasses()
        {
            if (_portableFactoryClasses == null)
            {
                _portableFactoryClasses = new Dictionary<int, string>();
            }
            return _portableFactoryClasses;
        }

        /// <summary>
        /// Gets the configured Portable version that will be used to differentiate two versions of the same class
        /// that have changes on the class, like adding/removing a field or changing a type of a field.
        /// </summary>
        /// <returns>version of portable classes</returns>
        public virtual int GetPortableVersion()
        {
            return _portableVersion;
        }

        /// <summary>
        /// Gets all <see cref="SerializationConfiguration"/>s
        /// </summary>
        /// <returns>collection of <see cref="SerializerConfig"/></returns>
        public virtual ICollection<SerializerConfig> GetSerializerConfigs()
        {
            if (_serializerConfigs == null)
            {
                _serializerConfigs = new List<SerializerConfig>();
            }
            return _serializerConfigs;
        }

        /// <summary>
        /// When enabled, serialization system will check for class definitions error at start
        /// and throw an Serialization Exception with error definition or not.
        /// <br/>
        /// Default value is <c>true</c>
        /// </summary>
        /// <returns><c>true</c> if enables <c>false</c> otherwise</returns>
        public virtual bool IsCheckClassDefErrors()
        {
            return _checkClassDefErrors;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns><c>true</c> if serialization is configured to use native byte order of the underlying platform</returns>
        public virtual bool IsUseNativeByteOrder()
        {
            return _useNativeByteOrder;
        }

        /// <summary>
        /// Sets the serialization's <see cref="ByteOrder"/>
        /// </summary>
        /// <param name="byteOrder">the <see cref="ByteOrder"/> that serialization will use</param>
        /// <returns>configured <see cref="SerializationConfiguration"/> for chaining</returns>
        public virtual SerializationConfiguration SetEndianness(Endianness endianness)
        {
            _endianness = endianness;
            return this;
        }

        /// <summary>
        /// When enabled, serialization system will check for class definitions error at start
        /// and throw an Serialization Exception with error definition or not.
        /// </summary>
        /// <param name="checkClassDefErrors">set to <c>false</c> to disable</param>
        /// <returns>configured <see cref="SerializationConfiguration"/> for chaining</returns>
        public virtual SerializationConfiguration SetCheckClassDefErrors(bool checkClassDefErrors)
        {
            _checkClassDefErrors = checkClassDefErrors;
            return this;
        }

        /// <summary>
        /// Sets all <see cref="IClassDefinition"/>s
        /// </summary>
        /// <param name="classDefinitions">collection of <see cref="IClassDefinition"/>s</param>
        /// <returns>configured <see cref="SerializationConfiguration"/> for chaining</returns>
        public virtual SerializationConfiguration SetClassDefinitions(ICollection<IClassDefinition> classDefinitions)
        {
            _classDefinitions = classDefinitions;
            return this;
        }

        /// <summary>
        /// Sets the dictionary of factory ID and corresponding <see cref="IDataSerializableFactory"/>s
        /// </summary>
        /// <param name="dataSerializableFactories">dictionary of factory ID and corresponding <see cref="IDataSerializableFactory"/>s</param>
        /// <returns>configured <see cref="SerializationConfiguration"/> for chaining</returns>
        public virtual SerializationConfiguration SetDataSerializableFactories(
            IDictionary<int, IDataSerializableFactory> dataSerializableFactories)
        {
            _dataSerializableFactories = dataSerializableFactories;
            return this;
        }

        /// <summary>
        /// Sets the dictionary of factory ID and corresponding factory class names
        /// </summary>
        /// <param name="dataSerializableFactoryClasses">dictionary of factory ID and corresponding factory class names</param>
        /// <returns>configured <see cref="SerializationConfiguration"/> for chaining</returns>
        public virtual SerializationConfiguration SetDataSerializableFactoryClasses(
            IDictionary<int, string> dataSerializableFactoryClasses)
        {
            _dataSerializableFactoryClasses = dataSerializableFactoryClasses;
            return this;
        }

        /// <summary>
        /// Sets <see cref="GlobalSerializerConfig"/>
        /// </summary>
        /// <param name="globalSerializerConfig"><see cref="GlobalSerializerConfig"/></param>
        /// <returns>configured <see cref="SerializationConfiguration"/> for chaining</returns>
        public virtual SerializationConfiguration SetGlobalSerializerConfig(GlobalSerializerConfig globalSerializerConfig)
        {
            _globalSerializerConfig = globalSerializerConfig;
            return this;
        }

        /// <summary>
        /// Sets the dictionary of factory ID and corresponding <see cref="IPortableFactory"/>s
        /// </summary>
        /// <param name="portableFactories">dictionary of factory ID and corresponding <see cref="IPortableFactory"/>s</param>
        /// <returns>configured <see cref="SerializationConfiguration"/> for chaining</returns>
        public virtual SerializationConfiguration SetPortableFactories(IDictionary<int, IPortableFactory> portableFactories)
        {
            _portableFactories = portableFactories;
            return this;
        }

        /// <summary>
        /// Sets the dictionary of factory ID and corresponding factory class names
        /// </summary>
        /// <param name="portableFactoryClasses">dictionary of factory ID and corresponding factory class names</param>
        /// <returns>configured <see cref="SerializationConfiguration"/> for chaining</returns>
        public virtual SerializationConfiguration SetPortableFactoryClasses(IDictionary<int, string> portableFactoryClasses)
        {
            _portableFactoryClasses = portableFactoryClasses;
            return this;
        }

        /// <summary>
        /// Sets the version of portable classes
        /// </summary>
        /// <param name="portableVersion">version of portable classes</param>
        /// <returns>configured <see cref="SerializationConfiguration"/> for chaining</returns>
        /// <exception cref="ArgumentException">if portableVersion is less than 0</exception>
        public virtual SerializationConfiguration SetPortableVersion(int portableVersion)
        {
            if (portableVersion < 0)
            {
                throw new ArgumentException("IPortable version cannot be negative!");
            }
            _portableVersion = portableVersion;
            return this;
        }

        /// <summary>
        /// Sets all <see cref="SerializationConfiguration"/>s
        /// </summary>
        /// <param name="serializerConfigs">collection of <see cref="SerializationConfiguration"/>s</param>
        /// <returns>configured <see cref="SerializationConfiguration"/> for chaining</returns>
        public virtual SerializationConfiguration SetSerializerConfigs(ICollection<SerializerConfig> serializerConfigs)
        {
            _serializerConfigs = serializerConfigs;
            return this;
        }

        /// <summary>
        /// Sets to use native byte order of the underlying platform
        /// </summary>
        /// <param name="useNativeByteOrder">set to <c>true</c> to use native byte order of the underlying platform</param>
        /// <returns>configured <see cref="SerializationConfiguration"/> for chaining</returns>
        public virtual SerializationConfiguration SetUseNativeByteOrder(bool useNativeByteOrder)
        {
            _useNativeByteOrder = useNativeByteOrder;
            return this;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder("SerializationConfig{");
            sb.Append("portableVersion=").Append(_portableVersion);
            sb.Append(", dataSerializableFactoryClasses=").Append(_dataSerializableFactoryClasses);
            sb.Append(", dataSerializableFactories=").Append(_dataSerializableFactories);
            sb.Append(", portableFactoryClasses=").Append(_portableFactoryClasses);
            sb.Append(", portableFactories=").Append(_portableFactories);
            sb.Append(", globalSerializerConfig=").Append(_globalSerializerConfig);
            sb.Append(", serializerConfigs=").Append(_serializerConfigs);
            sb.Append(", checkClassDefErrors=").Append(_checkClassDefErrors);
            sb.Append(", classDefinitions=").Append(_classDefinitions);
            sb.Append(", endianness=").Append(_endianness);
            sb.Append(", useNativeByteOrder=").Append(_useNativeByteOrder);
            sb.Append('}');
            return sb.ToString();
        }

        public static SerializationConfiguration Parse(XmlNode node)
        {
            var configuration = new SerializationConfiguration();

            foreach (XmlNode child in node.ChildNodes)
            {
                var name = child.GetCleanName();
                switch (name)
                {
                    case "portable-version":
                        configuration.SetPortableVersion(child.GetInt32Content());
                        break;
                    case "check-class-def-errors":
                        configuration.SetCheckClassDefErrors(child.GetTrueFalseContent());
                        break;
                    case "use-native-byte-order":
                        configuration.SetUseNativeByteOrder(child.GetTrueFalseContent());
                        break;
                    case "byte-order":
                        var bigEndian = child.GetTextContent();
                        var endianness = "BIG_ENDIAN".Equals(bigEndian) ? Endianness.BigEndian : Endianness.LittleEndian;
                        configuration.SetEndianness(endianness);
                        break;
                    case "enable-compression":
                        // ignore
                        break;
                    case "enable-shared-object":
                        // ignore
                        break;
                    case "data-serializable-factories":
                        FillDataSerializableFactories(child, configuration);
                        break;
                    case "portable-factories":
                        FillPortableFactories(child, configuration);
                        break;
                    case "serializers":
                        FillSerializers(child, configuration);
                        break;
                }
            }

            return configuration;
        }

        private static void FillDataSerializableFactories(XmlNode node, SerializationConfiguration serializationConfiguration)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                var name = child.GetCleanName();
                if ("data-serializable-factory".Equals(name))
                {
                    var value = child.GetTextContent();
                    var factoryIdNode = child.Attributes.GetNamedItem("factory-id");
                    if (factoryIdNode == null)
                    {
                        throw new ArgumentException(
                            "'factory-id' attribute of 'data-serializable-factory' is required!");
                    }

                    var factoryId = factoryIdNode.GetInt32Content();
                    serializationConfiguration.AddDataSerializableFactoryClass(factoryId, value);
                }
            }
        }

        private static void FillPortableFactories(XmlNode node, SerializationConfiguration serializationConfiguration)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                var name = child.GetCleanName();
                if ("portable-factory".Equals(name))
                {
                    var value = child.GetTextContent();
                    var factoryIdNode = child.Attributes.GetNamedItem("factory-id");
                    if (factoryIdNode == null)
                    {
                        throw new ArgumentException("'factory-id' attribute of 'portable-factory' is required!");
                    }

                    var factoryId = factoryIdNode.GetInt32Content();
                    serializationConfiguration.AddPortableFactoryClass(factoryId, value);
                }
            }
        }

        private static void FillSerializers(XmlNode node, SerializationConfiguration serializationConfiguration)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                var name = child.GetCleanName();
                var value = child.GetTextContent();
                if ("serializer".Equals(name))
                {
                    var serializerConfig = new SerializerConfig();
                    serializerConfig.SetClassName(value);
                    var typeClassName = child.GetStringAttribute("type-class");
                    serializerConfig.SetTypeClassName(typeClassName);
                    serializationConfiguration.AddSerializerConfig(serializerConfig);
                }
                else
                {
                    if ("global-serializer".Equals(name))
                    {
                        var globalSerializerConfig = new GlobalSerializerConfig();
                        globalSerializerConfig.SetClassName(value);
                        serializationConfiguration.SetGlobalSerializerConfig(globalSerializerConfig);
                    }
                }
            }
        }
    }
}