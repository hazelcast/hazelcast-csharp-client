// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Configuration;
using Hazelcast.Configuration.Binding;
using Hazelcast.Core;

namespace Hazelcast.Serialization
{
    /// <summary>
    /// Configures a serializer for a type.
    /// </summary>
    public class SerializerOptions : SingletonServiceFactory<ISerializer>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationOptions"/> class.
        /// </summary>
        public SerializerOptions()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationOptions"/> class.
        /// </summary>
        protected SerializerOptions(SerializerOptions other, bool shallow)
            : base(other, shallow)
        {
            SerializedType = other.SerializedType;
        }

        /// <summary>
        /// Gets or sets the type being serialized.
        /// </summary>
        [BinderIgnore]
        public Type SerializedType { get; set; }

        /// <summary>
        /// Gets or sets the name of the type being serialized.
        /// </summary>
        [BinderIgnore(false)]
        private string SerializedTypeName
        {
            get => default;
            set => SerializedType = Type.GetType(value) ?? throw new ConfigurationException($"Unknown serialized type \"{value}\".");
        }

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal new SerializerOptions Clone(bool shallow = true) => new SerializerOptions(this, shallow);
    }
}
