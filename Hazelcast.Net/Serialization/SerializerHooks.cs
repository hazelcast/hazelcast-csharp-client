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
using System.Collections.Generic;
using System.Linq;

namespace Hazelcast.Serialization
{
    /// <summary>
    /// Represents a collection of <see cref="ISerializerHook{T}"/> types.
    /// </summary>
    public sealed class SerializerHooks // FIXME refactor this as a proper enumerable collection!
    {
        private readonly List<Type> _types = new List<Type>();

        /// <summary>
        /// Adds a type.
        /// </summary>
        /// <param name="type">The type.</param>
        public void Add(Type type) => _types.Add(type);

        /// <summary>
        /// Adds a type.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        public void Add<T>() => Add(typeof(T));

        /// <summary>
        /// Gets the types.
        /// </summary>
        public IEnumerable<Type> Types => _types;

        /// <summary>
        /// Gets the hooks.
        /// </summary>
        public IEnumerable<IDataSerializerHook> Hooks => _types.Select(Activator.CreateInstance).Cast<IDataSerializerHook>();
    }
}