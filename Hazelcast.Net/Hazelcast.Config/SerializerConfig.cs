/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

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