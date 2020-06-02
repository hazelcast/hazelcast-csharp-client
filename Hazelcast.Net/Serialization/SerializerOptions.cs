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
using Hazelcast.Configuration.Binding;
using Hazelcast.Core;
using Hazelcast.Exceptions;

namespace Hazelcast.Serialization
{
    public class SerializerOptions : ServiceFactory<ISerializer>
    {
        private Type _serializedType;

        [BinderIgnore]
        public Type SerializedType
        {
            get => _serializedType ??
                   Type.GetType(SerializedTypeName) ??
                   throw new ConfigurationException($"Unknown serialized type \"{SerializedTypeName}\".");
            set => _serializedType = value;
        }

        [BinderName("serializedType")]
        public string SerializedTypeName { get; set; }

        public string SerializerType
        {
            get => default;
            set => Creator = () => Services.CreateInstance<ISerializer>(value);
        }

        public bool OverrideClr { get; set; }

        /// <summary>
        /// Clones the options.
        /// </summary>
        public new SerializerOptions Clone()
        {
            return new SerializerOptions
            {
                _serializedType = _serializedType,
                SerializedTypeName = SerializedTypeName,
                OverrideClr = OverrideClr,
                Creator = Creator
            };
        }
    }
}
