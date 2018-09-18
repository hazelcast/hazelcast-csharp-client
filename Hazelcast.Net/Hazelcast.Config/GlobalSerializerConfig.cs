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

using Hazelcast.IO.Serialization;

namespace Hazelcast.Config
{
    /// <summary>
    /// Contains the configuration for global serializer.
    /// </summary>
    public class GlobalSerializerConfig
    {
        private string _className;

        private ISerializer _implementation;
        private bool _overrideClrSerialization;

        public virtual string GetClassName()
        {
            return _className;
        }

        public virtual ISerializer GetImplementation()
        {
            return _implementation;
        }

        public virtual GlobalSerializerConfig SetClassName(string className)
        {
            _className = className;
            return this;
        }

        public virtual GlobalSerializerConfig SetImplementation<T>(IStreamSerializer<T> implementation)
        {
            _implementation = implementation;
            return this;
        }

        public GlobalSerializerConfig SetOverrideClrSerialization(bool overrideClrSerialization)
        {
            _overrideClrSerialization = overrideClrSerialization;
            return this;
        }

        public bool GetOverrideClrSerialization()
        {
            return _overrideClrSerialization;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("ClassName: {0}, Implementation: {1}, OverrideClrSerialization: {2}",
                _className, _implementation, _overrideClrSerialization);
        }
    }
}