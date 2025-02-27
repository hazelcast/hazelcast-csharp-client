// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;

namespace Hazelcast.Configuration
{
    /// <summary>
    /// An in-memory based Hazelcast <see cref="ConfigurationProvider"/>.
    /// </summary>
    internal class HazelcastMemoryConfigurationProvider : MemoryConfigurationProvider
    {
        private const string HazelcastKeyAndDot = HazelcastOptions.SectionNameConstant + ".";
        private const string FailoverKeyAndDot = HazelcastFailoverOptions.SectionNameConstant + ".";

        public HazelcastMemoryConfigurationProvider(HazelcastMemoryConfigurationSource source)
            : base(FilterSource(source))
        { }

        /// <summary>
        /// (internal for tests only)
        /// Filters a configuration source.
        /// </summary>
        internal static MemoryConfigurationSource FilterSource(HazelcastMemoryConfigurationSource source)
        {
            KeyValuePair<string, string> Filter(KeyValuePair<string, string> kvp)
                => kvp.Key.StartsWith(HazelcastKeyAndDot, StringComparison.Ordinal) || kvp.Key.StartsWith(FailoverKeyAndDot, StringComparison.Ordinal)
                    ? new KeyValuePair<string, string>(kvp.Key.Replace(".", ConfigurationPath.KeyDelimiter, StringComparison.Ordinal), kvp.Value)
                    : kvp;

            return new MemoryConfigurationSource
            {
                InitialData = source.InitialData.Select(Filter)
            };
        }
    }
}
