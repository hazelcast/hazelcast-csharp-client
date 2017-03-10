// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.IO;

namespace Hazelcast.Config
{
    public class SocketInterceptorConfig
    {
        private string _className;
        private bool _enabled;

        private object _implementation;

        private Dictionary<string, string> _properties = new Dictionary<string, string>();

        /// <summary>
        ///     Returns the name of the
        ///     <see cref="ISocketInterceptor"/>
        ///     implementation class
        /// </summary>
        /// <returns>name of the class</returns>
        public virtual string GetClassName()
        {
            return _className;
        }

        /// <summary>
        ///     Returns the
        ///     <see cref="ISocketInterceptor" />
        ///     implementation object
        /// </summary>
        /// <returns>SocketInterceptor implementation object</returns>
        public virtual object GetImplementation()
        {
            return _implementation;
        }

        /// <summary>Gets all properties.</summary>
        /// <remarks>Gets all properties.</remarks>
        /// <returns>the properties.</returns>
        public virtual Dictionary<string, string> GetProperties()
        {
            return _properties;
        }

        /// <summary>Gets a property.</summary>
        /// <remarks>Gets a property.</remarks>
        /// <param name="name">the name of the property to get.</param>
        /// <returns>the value of the property, null if not found</returns>
        /// <exception cref="System.ArgumentNullException">if name is null.</exception>
        public virtual string GetProperty(string name)
        {
            string value;
            _properties.TryGetValue(name, out value);
            return value;
        }

        /// <summary>Returns if this configuration is enabled</summary>
        /// <returns>true if enabled, false otherwise</returns>
        public virtual bool IsEnabled()
        {
            return _enabled;
        }

        /// <summary>
        ///     Sets the name for the
        ///     <see cref="ISocketInterceptor" />
        ///     implementation class
        /// </summary>
        /// <param name="className">
        ///     the name of the
        ///     <see cref="ISocketInterceptor" />
        ///     implementation class to set
        /// </param>
        /// <returns>this SocketInterceptorConfig instance</returns>
        public virtual SocketInterceptorConfig SetClassName(string className)
        {
            _className = className;
            return this;
        }

        /// <summary>Enables and disables this configuration</summary>
        /// <param name="enabled"></param>
        public virtual SocketInterceptorConfig SetEnabled(bool enabled)
        {
            _enabled = enabled;
            return this;
        }

        /// <summary>
        ///     Sets the
        ///     <see cref="ISocketInterceptor" />
        ///     implementation object
        /// </summary>
        /// <param name="implementation">implementation object</param>
        /// <returns>this SocketInterceptorConfig instance</returns>
        public virtual SocketInterceptorConfig SetImplementation(object implementation)
        {
            _implementation = implementation;
            return this;
        }

        /// <summary>Sets the properties.</summary>
        /// <remarks>Sets the properties.</remarks>
        /// <param name="properties">the properties to set.</param>
        /// <returns>the updated SSLConfig.</returns>
        /// <exception cref="System.ArgumentException">if properties is null.</exception>
        public virtual SocketInterceptorConfig SetProperties(Dictionary<string, string> properties)
        {
            if (properties == null)
            {
                throw new ArgumentException("properties can't be null");
            }
            _properties = properties;
            return this;
        }

        /// <summary>Sets a property.</summary>
        /// <remarks>Sets a property.</remarks>
        /// <param name="name">the name of the property to set.</param>
        /// <param name="value">the value of the property to set</param>
        /// <returns>the updated SocketInterceptorConfig</returns>
        /// <exception cref="System.ArgumentNullException">if name or value is null.</exception>
        public virtual SocketInterceptorConfig SetProperty(string name, string value)
        {
            _properties.Add(name, value);
            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("SocketInterceptorConfig");
            sb.Append("{className='").Append(_className).Append('\'');
            sb.Append(", enabled=").Append(_enabled);
            sb.Append(", implementation=").Append(_implementation);
            sb.Append(", properties=").Append(_properties);
            sb.Append('}');
            return sb.ToString();
        }
    }
}