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

using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Hazelcast.Configuration
{
    /// <summary>
    /// Represents Hazelcast in-memory arguments as an <see cref="IConfigurationSource"/>.
    /// </summary>
    internal class HazelcastMemoryConfigurationSource : IConfigurationSource
    {
        /// <summary>
        /// Gets or sets the initial key value configuration pairs.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> InitialData { get; set; }

        /// <inheritdoc />
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new HazelcastMemoryConfigurationProvider(this);
        }
    }
}
