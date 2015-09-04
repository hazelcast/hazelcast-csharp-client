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
        private ByteOrder byteOrder = ByteOrder.BigEndian;
        private bool checkClassDefErrors = true;
        private ICollection<IClassDefinition> classDefinitions;
        private IDictionary<int, IDataSerializableFactory> dataSerializableFactories;
        private IDictionary<int, string> dataSerializableFactoryClasses;
        private bool enableCompression = false;

        private bool enableSharedObject = false;

        private GlobalSerializerConfig globalSerializerConfig;
        private IDictionary<int, IPortableFactory> portableFactories;
        private IDictionary<int, string> portableFactoryClasses;
        private int portableVersion = 0;

        private ICollection<SerializerConfig> serializerConfigs;

        private bool useNativeByteOrder = false;

        public SerializationConfig() : base(){}

        public virtual GlobalSerializerConfig GetGlobalSerializerConfig()
        {
            return globalSerializerConfig;
        }

        public virtual SerializationConfig SetGlobalSerializerConfig(GlobalSerializerConfig globalSerializerConfig)
        {
            this.globalSerializerConfig = globalSerializerConfig;
            return this;
        }

        public virtual ICollection<SerializerConfig> GetSerializerConfigs()
        {
            if (serializerConfigs == null)
            {
                serializerConfigs = new List<SerializerConfig>();
            }
            return serializerConfigs;
        }

        public virtual SerializationConfig AddSerializerConfig(SerializerConfig serializerConfig)
        {
            GetSerializerConfigs().Add(serializerConfig);
            return this;
        }

        public virtual SerializationConfig SetSerializerConfigs(ICollection<SerializerConfig> serializerConfigs)
        {
            this.serializerConfigs = serializerConfigs;
            return this;
        }

        public virtual int GetPortableVersion()
        {
            return portableVersion;
        }

        public virtual SerializationConfig SetPortableVersion(int portableVersion)
        {
            if (portableVersion < 0)
            {
                throw new ArgumentException("IPortable version cannot be negative!");
            }
            this.portableVersion = portableVersion;
            return this;
        }

        public virtual IDictionary<int, string> GetDataSerializableFactoryClasses()
        {
            if (dataSerializableFactoryClasses == null)
            {
                dataSerializableFactoryClasses = new Dictionary<int, string>();
            }
            return dataSerializableFactoryClasses;
        }

        public virtual SerializationConfig SetDataSerializableFactoryClasses(
            IDictionary<int, string> dataSerializableFactoryClasses)
        {
            this.dataSerializableFactoryClasses = dataSerializableFactoryClasses;
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
            string factoryClassName =
                ValidationUtil.IsNotNull(dataSerializableFactoryClass, "dataSerializableFactoryClass").FullName;
            return AddDataSerializableFactoryClass(factoryId, factoryClassName);
        }

        public virtual IDictionary<int, IDataSerializableFactory> GetDataSerializableFactories()
        {
            if (dataSerializableFactories == null)
            {
                dataSerializableFactories = new Dictionary<int, IDataSerializableFactory>();
            }
            return dataSerializableFactories;
        }

        public virtual SerializationConfig SetDataSerializableFactories(
            IDictionary<int, IDataSerializableFactory> dataSerializableFactories)
        {
            this.dataSerializableFactories = dataSerializableFactories;
            return this;
        }

        public virtual SerializationConfig AddDataSerializableFactory(int factoryId,
            IDataSerializableFactory dataSerializableFactory)
        {
            GetDataSerializableFactories().Add(factoryId, dataSerializableFactory);
            return this;
        }

        public virtual IDictionary<int, string> GetPortableFactoryClasses()
        {
            if (portableFactoryClasses == null)
            {
                portableFactoryClasses = new Dictionary<int, string>();
            }
            return portableFactoryClasses;
        }

        public virtual SerializationConfig SetPortableFactoryClasses(IDictionary<int, string> portableFactoryClasses)
        {
            this.portableFactoryClasses = portableFactoryClasses;
            return this;
        }

        public virtual SerializationConfig AddPortableFactoryClass(int factoryId, Type portableFactoryClass)
        {
            //TODO portableFactoryClass paramtere extends ??? IPortableFactory
            string portableFactoryClassName =
                ValidationUtil.IsNotNull(portableFactoryClass, "portableFactoryClass").FullName;
            return AddPortableFactoryClass(factoryId, portableFactoryClassName);
        }

        public virtual SerializationConfig AddPortableFactoryClass(int factoryId, string portableFactoryClass)
        {
            GetPortableFactoryClasses().Add(factoryId, portableFactoryClass);
            return this;
        }

        public virtual IDictionary<int, IPortableFactory> GetPortableFactories()
        {
            if (portableFactories == null)
            {
                portableFactories = new Dictionary<int, IPortableFactory>();
            }
            return portableFactories;
        }

        public virtual SerializationConfig SetPortableFactories(IDictionary<int, IPortableFactory> portableFactories)
        {
            this.portableFactories = portableFactories;
            return this;
        }

        public virtual SerializationConfig AddPortableFactory(int factoryId, IPortableFactory portableFactory)
        {
            GetPortableFactories().Add(factoryId, portableFactory);
            return this;
        }

        public virtual ICollection<IClassDefinition> GetClassDefinitions()
        {
            if (classDefinitions == null)
            {
                classDefinitions = new HashSet<IClassDefinition>();
            }
            return classDefinitions;
        }

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

        public virtual SerializationConfig SetClassDefinitions(ICollection<IClassDefinition> classDefinitions)
        {
            this.classDefinitions = classDefinitions;
            return this;
        }

        public virtual bool IsCheckClassDefErrors()
        {
            return checkClassDefErrors;
        }

        public virtual SerializationConfig SetCheckClassDefErrors(bool checkClassDefErrors)
        {
            this.checkClassDefErrors = checkClassDefErrors;
            return this;
        }

        public virtual bool IsUseNativeByteOrder()
        {
            return useNativeByteOrder;
        }

        public virtual SerializationConfig SetUseNativeByteOrder(bool useNativeByteOrder)
        {
            this.useNativeByteOrder = useNativeByteOrder;
            return this;
        }

        public virtual ByteOrder GetByteOrder()
        {
            return byteOrder;
        }

        public virtual SerializationConfig SetByteOrder(ByteOrder byteOrder)
        {
            this.byteOrder = byteOrder;
            return this;
        }

        public virtual bool IsEnableCompression()
        {
            return enableCompression;
        }

        public virtual SerializationConfig SetEnableCompression(bool enableCompression)
        {
            this.enableCompression = enableCompression;
            return this;
        }

        public virtual bool IsEnableSharedObject()
        {
            return enableSharedObject;
        }

        public virtual SerializationConfig SetEnableSharedObject(bool enableSharedObject)
        {
            this.enableSharedObject = enableSharedObject;
            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("SerializationConfig{");
            sb.Append("portableVersion=").Append(portableVersion);
            sb.Append(", dataSerializableFactoryClasses=").Append(dataSerializableFactoryClasses);
            sb.Append(", dataSerializableFactories=").Append(dataSerializableFactories);
            sb.Append(", portableFactoryClasses=").Append(portableFactoryClasses);
            sb.Append(", portableFactories=").Append(portableFactories);
            sb.Append(", globalSerializerConfig=").Append(globalSerializerConfig);
            sb.Append(", serializerConfigs=").Append(serializerConfigs);
            sb.Append(", checkClassDefErrors=").Append(checkClassDefErrors);
            sb.Append(", classDefinitions=").Append(classDefinitions);
            sb.Append(", byteOrder=").Append(byteOrder);
            sb.Append(", useNativeByteOrder=").Append(useNativeByteOrder);
            sb.Append('}');
            return sb.ToString();
        }
    }
}