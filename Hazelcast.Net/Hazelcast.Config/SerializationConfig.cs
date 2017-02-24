// Copyright (c) 2008, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.Config
{
    public class SerializationConfig
    {
        private ByteOrder _byteOrder = ByteOrder.BigEndian;
        private bool _checkClassDefErrors = true;
        private ICollection<IClassDefinition> _classDefinitions;
        private IDictionary<int, IDataSerializableFactory> _dataSerializableFactories;
        private IDictionary<int, string> _dataSerializableFactoryClasses;
        private bool _enableCompression;

        private bool _enableSharedObject;

        private GlobalSerializerConfig _globalSerializerConfig;
        private IDictionary<int, IPortableFactory> _portableFactories;
        private IDictionary<int, string> _portableFactoryClasses;
        private int _portableVersion;

        private ICollection<SerializerConfig> _serializerConfigs;

        private bool _useNativeByteOrder;

        public virtual SerializationConfig AddClassDefinition(IClassDefinition classDefinition)
        {
            if (GetClassDefinitions().Contains(classDefinition))
            {
                throw new ArgumentException("IClassDefinition for class-id[" + classDefinition.GetClassId() +
                                            "] already exists!");
            }
            GetClassDefinitions().Add(classDefinition);
            return this;
        }

        public virtual SerializationConfig AddDataSerializableFactory(int factoryId,
            IDataSerializableFactory dataSerializableFactory)
        {
            GetDataSerializableFactories().Add(factoryId, dataSerializableFactory);
            return this;
        }

        public virtual SerializationConfig AddDataSerializableFactoryClass(int factoryId,
            string dataSerializableFactoryClass)
        {
            //TODO dataSerializableFactoryClass paramtere extends ??? IDataSerializableFactory
            GetDataSerializableFactoryClasses().Add(factoryId, dataSerializableFactoryClass);
            return this;
        }

        public virtual SerializationConfig AddDataSerializableFactoryClass(int factoryId,
            Type dataSerializableFactoryClass)
        {
            var factoryClassName =
                ValidationUtil.IsNotNull(dataSerializableFactoryClass, "dataSerializableFactoryClass")
                    .AssemblyQualifiedName;
            return AddDataSerializableFactoryClass(factoryId, factoryClassName);
        }

        public virtual SerializationConfig AddPortableFactory(int factoryId, IPortableFactory portableFactory)
        {
            GetPortableFactories().Add(factoryId, portableFactory);
            return this;
        }

        public virtual SerializationConfig AddPortableFactoryClass(int factoryId, Type portableFactoryClass)
        {
            var portableFactoryClassName =
                ValidationUtil.IsNotNull(portableFactoryClass, "portableFactoryClass").AssemblyQualifiedName;
            return AddPortableFactoryClass(factoryId, portableFactoryClassName);
        }

        public virtual SerializationConfig AddPortableFactoryClass(int factoryId, string portableFactoryClass)
        {
            GetPortableFactoryClasses().Add(factoryId, portableFactoryClass);
            return this;
        }

        public virtual SerializationConfig AddSerializerConfig(SerializerConfig serializerConfig)
        {
            GetSerializerConfigs().Add(serializerConfig);
            return this;
        }

        public virtual ByteOrder GetByteOrder()
        {
            return _byteOrder;
        }

        public virtual ICollection<IClassDefinition> GetClassDefinitions()
        {
            if (_classDefinitions == null)
            {
                _classDefinitions = new HashSet<IClassDefinition>();
            }
            return _classDefinitions;
        }

        public virtual IDictionary<int, IDataSerializableFactory> GetDataSerializableFactories()
        {
            if (_dataSerializableFactories == null)
            {
                _dataSerializableFactories = new Dictionary<int, IDataSerializableFactory>();
            }
            return _dataSerializableFactories;
        }

        public virtual IDictionary<int, string> GetDataSerializableFactoryClasses()
        {
            if (_dataSerializableFactoryClasses == null)
            {
                _dataSerializableFactoryClasses = new Dictionary<int, string>();
            }
            return _dataSerializableFactoryClasses;
        }

        public virtual GlobalSerializerConfig GetGlobalSerializerConfig()
        {
            return _globalSerializerConfig;
        }

        public virtual IDictionary<int, IPortableFactory> GetPortableFactories()
        {
            if (_portableFactories == null)
            {
                _portableFactories = new Dictionary<int, IPortableFactory>();
            }
            return _portableFactories;
        }

        public virtual IDictionary<int, string> GetPortableFactoryClasses()
        {
            if (_portableFactoryClasses == null)
            {
                _portableFactoryClasses = new Dictionary<int, string>();
            }
            return _portableFactoryClasses;
        }

        public virtual int GetPortableVersion()
        {
            return _portableVersion;
        }

        public virtual ICollection<SerializerConfig> GetSerializerConfigs()
        {
            if (_serializerConfigs == null)
            {
                _serializerConfigs = new List<SerializerConfig>();
            }
            return _serializerConfigs;
        }

        public virtual bool IsCheckClassDefErrors()
        {
            return _checkClassDefErrors;
        }

        public virtual bool IsEnableCompression()
        {
            return _enableCompression;
        }

        public virtual bool IsEnableSharedObject()
        {
            return _enableSharedObject;
        }

        public virtual bool IsUseNativeByteOrder()
        {
            return _useNativeByteOrder;
        }

        public virtual SerializationConfig SetByteOrder(ByteOrder byteOrder)
        {
            _byteOrder = byteOrder;
            return this;
        }

        public virtual SerializationConfig SetCheckClassDefErrors(bool checkClassDefErrors)
        {
            _checkClassDefErrors = checkClassDefErrors;
            return this;
        }

        public virtual SerializationConfig SetClassDefinitions(ICollection<IClassDefinition> classDefinitions)
        {
            _classDefinitions = classDefinitions;
            return this;
        }

        public virtual SerializationConfig SetDataSerializableFactories(
            IDictionary<int, IDataSerializableFactory> dataSerializableFactories)
        {
            _dataSerializableFactories = dataSerializableFactories;
            return this;
        }

        public virtual SerializationConfig SetDataSerializableFactoryClasses(
            IDictionary<int, string> dataSerializableFactoryClasses)
        {
            _dataSerializableFactoryClasses = dataSerializableFactoryClasses;
            return this;
        }

        public virtual SerializationConfig SetEnableCompression(bool enableCompression)
        {
            _enableCompression = enableCompression;
            return this;
        }

        public virtual SerializationConfig SetEnableSharedObject(bool enableSharedObject)
        {
            _enableSharedObject = enableSharedObject;
            return this;
        }

        public virtual SerializationConfig SetGlobalSerializerConfig(GlobalSerializerConfig globalSerializerConfig)
        {
            _globalSerializerConfig = globalSerializerConfig;
            return this;
        }

        public virtual SerializationConfig SetPortableFactories(IDictionary<int, IPortableFactory> portableFactories)
        {
            _portableFactories = portableFactories;
            return this;
        }

        public virtual SerializationConfig SetPortableFactoryClasses(IDictionary<int, string> portableFactoryClasses)
        {
            _portableFactoryClasses = portableFactoryClasses;
            return this;
        }

        public virtual SerializationConfig SetPortableVersion(int portableVersion)
        {
            if (portableVersion < 0)
            {
                throw new ArgumentException("IPortable version cannot be negative!");
            }
            _portableVersion = portableVersion;
            return this;
        }

        public virtual SerializationConfig SetSerializerConfigs(ICollection<SerializerConfig> serializerConfigs)
        {
            _serializerConfigs = serializerConfigs;
            return this;
        }

        public virtual SerializationConfig SetUseNativeByteOrder(bool useNativeByteOrder)
        {
            _useNativeByteOrder = useNativeByteOrder;
            return this;
        }

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
            sb.Append(", byteOrder=").Append(_byteOrder);
            sb.Append(", useNativeByteOrder=").Append(_useNativeByteOrder);
            sb.Append('}');
            return sb.ToString();
        }
    }
}