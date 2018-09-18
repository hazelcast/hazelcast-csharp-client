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

using System.Collections.Generic;
using Hazelcast.Security;

namespace Hazelcast.Config
{
    /// <summary>
    /// Contains the configuration for Credentials Factory.
    /// </summary>
    public class CredentialsFactoryConfig
    {
        private string _className;

        private ICredentialsFactory _implementation;

        private Dictionary<string, string> _properties = new Dictionary<string, string>();

        public CredentialsFactoryConfig(string className =null)
        {
            _className = className;
        }

        /// <summary>
        /// Gets the configured class name of <see cref="ICredentialsFactory"/> implementation
        /// </summary>
        /// <returns>class name of <see cref="ICredentialsFactory"/> implementation</returns>
        public string GetClassName() {
            return _className;
        }

        /// <summary>
        /// Sets the class name of <see cref="ICredentialsFactory"/> implementation that will be used to
        /// instantiate the factory instance
        /// </summary>
        /// <param name="classname">the factory class name</param>
        /// <returns>configured <see cref="CredentialsFactoryConfig"/> for chaining</returns>
        public CredentialsFactoryConfig SetClassName(string classname) {
            _className = classname;
            return this;
        }
        
        /// <summary>
        /// Get the configured factory instance
        /// </summary>
        /// <returns>factory instance</returns>
        public ICredentialsFactory GetImplementation()
        {
            return _implementation;
        }

        /// <summary>
        /// Set the factory instance
        /// </summary>
        /// <param name="factoryImpl">factory instance to be configured</param>
        /// <returns>configured <see cref="CredentialsFactoryConfig"/> for chaining</returns>
        public CredentialsFactoryConfig SetImplementation(ICredentialsFactory factoryImpl)
        {
            _implementation = factoryImpl;
            return this;
        }
        
        /// <summary>
        /// Gets the dictionary of the properties that will be used in <see cref="ICredentialsFactory.Configure"/>
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetProperties()
        {
            return _properties;
        }

        /// <summary>
        /// Sets the dictionary of the properties that will be used in <see cref="ICredentialsFactory.Configure"/>
        /// </summary>
        /// <param name="properites"></param>
        /// <returns>configured <see cref="CredentialsFactoryConfig"/> for chaining</returns>
        public CredentialsFactoryConfig SetProperties(Dictionary<string, string> properites)
        {
            _properties = properites;
            return this;
        }

        /// <summary>
        /// Gets the value of the configured key-value pair
        /// </summary>
        /// <param name="name">name of the pair</param>
        /// <returns>value of the pair</returns>
        public string GetProperty(string name)
        {
            string value;
            _properties.TryGetValue(name, out value);
            return value;
        }

        /// <summary>
        /// Adds a key, value pair into the properties dictionary of this configuration if not exist
        /// </summary>
        /// <param name="name">name of the pair</param>
        /// <param name="value">value of the pair</param>
        /// <returns>configured <see cref="CredentialsFactoryConfig"/> for chaining</returns>
        public CredentialsFactoryConfig SetProperty(string name, string value)
        {
            _properties.Add(name, value);
            return this;
        }
    }
}