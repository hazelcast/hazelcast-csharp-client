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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Hazelcast.IO.Serialization;
using Hazelcast.Net.Ext;
using Hazelcast.Util;

namespace Hazelcast.Config
{
    public class SerializationConfig
    {
        public ByteOrder ByteOrder { get; set; } = ByteOrder.BigEndian;
        public bool CheckClassDefErrors { get; set; } = true;
        public ICollection<IClassDefinition> ClassDefinitions { get; set; } = new HashSet<IClassDefinition>();

        public IDictionary<int, IDataSerializableFactory> DataSerializableFactories { get; set; } =
            new ConcurrentDictionary<int, IDataSerializableFactory>();

        public IDictionary<int, string> DataSerializableFactoryClasses { get; set; } = new ConcurrentDictionary<int, string>();

        public GlobalSerializerConfig GlobalSerializerConfig { get; set; } //= new GlobalSerializerConfig();

        public IDictionary<int, IPortableFactory> PortableFactories { get; set; } =
            new ConcurrentDictionary<int, IPortableFactory>();

        public IDictionary<int, string> PortableFactoryClasses { get; set; } = new ConcurrentDictionary<int, string>();
        public int PortableVersion { get; set; } = 0;

        public ICollection<SerializerConfig> SerializerConfigs { get; set; } = new List<SerializerConfig>();

        public bool UseNativeByteOrder { get; set; } = false;

        public SerializationConfig ConfigureGlobalSerializer(Action<GlobalSerializerConfig> configAction)
        {
            if (GlobalSerializerConfig == null)
            {
                GlobalSerializerConfig = new GlobalSerializerConfig();
            }
            configAction(GlobalSerializerConfig);
            return this;
        }

        public SerializationConfig AddClassDefinition(IClassDefinition classDefinition)
        {
            if (ClassDefinitions.Contains(classDefinition))
            {
                throw new ArgumentException("IClassDefinition for class-id[" + classDefinition.GetClassId() +
                                            "] already exists!");
            }
            ClassDefinitions.Add(classDefinition);
            return this;
        }

        public SerializationConfig AddDataSerializableFactory(int factoryId, IDataSerializableFactory dataSerializableFactory)
        {
            DataSerializableFactories.Add(factoryId, dataSerializableFactory);
            return this;
        }

        public SerializationConfig AddDataSerializableFactoryClass(int factoryId, string dataSerializableFactoryClass)
        {
            DataSerializableFactoryClasses.Add(factoryId, dataSerializableFactoryClass);
            return this;
        }

        public SerializationConfig AddDataSerializableFactoryClass(int factoryId, Type dataSerializableFactoryClass)
        {
            var factoryClassName = ValidationUtil.IsNotNull(dataSerializableFactoryClass, "dataSerializableFactoryClass")
                .AssemblyQualifiedName;
            return AddDataSerializableFactoryClass(factoryId, factoryClassName);
        }

        public SerializationConfig AddPortableFactory(int factoryId, IPortableFactory portableFactory)
        {
            PortableFactories.Add(factoryId, portableFactory);
            return this;
        }

        public SerializationConfig AddPortableFactoryClass(int factoryId, Type portableFactoryClass)
        {
            var portableFactoryClassName =
                ValidationUtil.IsNotNull(portableFactoryClass, "portableFactoryClass").AssemblyQualifiedName;
            return AddPortableFactoryClass(factoryId, portableFactoryClassName);
        }

        public SerializationConfig AddPortableFactoryClass(int factoryId, string portableFactoryClass)
        {
            PortableFactoryClasses.Add(factoryId, portableFactoryClass);
            return this;
        }
        
        public override string ToString()
        {
            var sb = new StringBuilder("SerializationConfig{");
            sb.Append("portableVersion=").Append(PortableVersion);
            sb.Append(", dataSerializableFactoryClasses=").Append(DataSerializableFactoryClasses);
            sb.Append(", dataSerializableFactories=").Append(DataSerializableFactories);
            sb.Append(", portableFactoryClasses=").Append(PortableFactoryClasses);
            sb.Append(", portableFactories=").Append(PortableFactories);
            sb.Append(", globalSerializerConfig=").Append(GlobalSerializerConfig);
            sb.Append(", serializerConfigs=").Append(SerializerConfigs);
            sb.Append(", checkClassDefErrors=").Append(CheckClassDefErrors);
            sb.Append(", classDefinitions=").Append(ClassDefinitions);
            sb.Append(", byteOrder=").Append(ByteOrder);
            sb.Append(", useNativeByteOrder=").Append(UseNativeByteOrder);
            sb.Append('}');
            return sb.ToString();
        }
    }
}