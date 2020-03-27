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

using Hazelcast.Core;
using Hazelcast.Util;

namespace Hazelcast.Config
{
    /// <summary>
    ///     Contains the configuration for an
    ///     <see cref="IEventListener">IEventListener</see>
    ///     . The configuration contains either the typeName
    ///     of the IEventListener implementation, or the actual IEventListener instance.
    /// </summary>
    public class ListenerConfig
    {
        private string _typeName;

        /// <summary>Creates a ListenerConfig without className/implementation.</summary>
        private ListenerConfig()
        {
        }

        /// <summary>Creates a ListenerConfig with the given className.</summary>
        /// <param name="className">the name of the IEventListener class.</param>
        /// <exception cref="System.ArgumentException">if className is null or an empty String.</exception>
        public ListenerConfig(string typeNameName)
        {
            TypeName = typeNameName;
        }

        /// <summary>Creates a ListenerConfig with the given implementation.</summary>
        /// <param name="implementation">the implementation to use as IEventListener.</param>
        /// <exception cref="System.ArgumentException">if the implementation is null.</exception>
        public ListenerConfig(IEventListener implementation)
        {
            Implementation = ValidationUtil.IsNotNull(implementation, "implementation");
        }

        /// <summary>
        /// the implementation to use as IEventListener.
        /// </summary>
        public IEventListener Implementation { get; set; }

        /// <summary>
        /// The assembly-qualified name of the listener type
        /// </summary>
        public string TypeName
        {
            get => _typeName;
            set
            {
                _typeName = ValidationUtil.HasText(value, "TypeName");
                Implementation = null;
            }
        }

        public override string ToString()
        {
            return $"ListenerConfig [className={_typeName}, implementation={Implementation}]";
        }
    }
}