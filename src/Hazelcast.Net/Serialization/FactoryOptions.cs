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

using Hazelcast.Core;

namespace Hazelcast.Serialization
{
    public class FactoryOptions<T> : SingletonServiceFactory<T>
        where T : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FactoryOptions{T}"/> class.
        /// </summary>
        public FactoryOptions()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FactoryOptions{T}"/> class.
        /// </summary>
        private FactoryOptions(FactoryOptions<T> other, bool shallow)
            : base(other, shallow)
        {
            Id = other.Id;
        }

        /// <summary>
        /// Gets or sets the identifier of the factory.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal new FactoryOptions<T> Clone(bool shallow = true) => new FactoryOptions<T>(this, shallow);
    }
}
