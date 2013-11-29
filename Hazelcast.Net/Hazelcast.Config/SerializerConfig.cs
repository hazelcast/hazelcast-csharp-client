using System;
using System.Text;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Config
{
    public class SerializerConfig
    {
        private string className;

        private ISerializer implementation;

        private Type typeClass;

        private string typeClassName;

        public SerializerConfig() : base()
        {
        }

        public virtual string GetClassName()
        {
            return className;
        }

        public virtual SerializerConfig SetClass(Type clazz)
        {
            //TODO where _T0:ISerializer
            string className = clazz == null ? null : clazz.FullName;
            return SetClassName(className);
        }

        public virtual SerializerConfig SetClassName(string className)
        {
            this.className = className;
            return this;
        }

        public virtual ISerializer GetImplementation()
        {
            return implementation;
        }

        public virtual SerializerConfig SetImplementation<T>(IByteArraySerializer<T> implementation)
        {
            this.implementation = implementation;
            return this;
        }

        public virtual SerializerConfig SetImplementation<T>(IStreamSerializer<T> implementation)
        {
            this.implementation = implementation;
            return this;
        }

        public virtual Type GetTypeClass()
        {
            return typeClass;
        }

        public virtual SerializerConfig SetTypeClass(Type typeClass)
        {
            this.typeClass = typeClass;
            return this;
        }

        public virtual string GetTypeClassName()
        {
            return typeClassName;
        }

        public virtual SerializerConfig SetTypeClassName(string typeClassName)
        {
            this.typeClassName = typeClassName;
            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("SerializerConfig{");
            sb.Append("className='").Append(className).Append('\'');
            sb.Append(", implementation=").Append(implementation);
            sb.Append(", typeClass=").Append(typeClass);
            sb.Append(", typeClassName='").Append(typeClassName).Append('\'');
            sb.Append('}');
            return sb.ToString();
        }
    }
}