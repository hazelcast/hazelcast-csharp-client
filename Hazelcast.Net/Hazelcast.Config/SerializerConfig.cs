// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
using System.Text;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Config
{
    /// <summary>
    /// Contains the serialization configuration for a particular class.
    /// </summary>
    public class SerializerConfig
    {
        private string _className;
        private ISerializer _implementation;
        private Type _typeClass;
        private string _typeClassName;

        public virtual string GetClassName()
        {
            return _className;
        }

        public virtual ISerializer GetImplementation()
        {
            return _implementation;
        }

        public virtual Type GetTypeClass()
        {
            return _typeClass;
        }

        public virtual string GetTypeClassName()
        {
            return _typeClassName;
        }

        public virtual SerializerConfig SetClass(Type clazz)
        {
            //TODO where _T0:ISerializer
            var className = clazz == null ? null : clazz.FullName;
            return SetClassName(className);
        }

        public virtual SerializerConfig SetClassName(string className)
        {
            _className = className;
            return this;
        }

        public virtual SerializerConfig SetImplementation<T>(IByteArraySerializer<T> implementation)
        {
            _implementation = implementation;
            return this;
        }

        public virtual SerializerConfig SetImplementation<T>(IStreamSerializer<T> implementation)
        {
            _implementation = implementation;
            return this;
        }

        public virtual SerializerConfig SetTypeClass(Type typeClass)
        {
            _typeClass = typeClass;
            return this;
        }

        public virtual SerializerConfig SetTypeClassName(string typeClassName)
        {
            _typeClassName = typeClassName;
            return this;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder("SerializerConfig{");
            sb.Append("className='").Append(_className).Append('\'');
            sb.Append(", implementation=").Append(_implementation);
            sb.Append(", typeClass=").Append(_typeClass);
            sb.Append(", typeClassName='").Append(_typeClassName).Append('\'');
            sb.Append('}');
            return sb.ToString();
        }
    }
}