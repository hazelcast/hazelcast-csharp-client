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
using Hazelcast.Core;

namespace Hazelcast.NearCaching
{
    /// <summary>
    /// Represents the Near Cache options.
    /// </summary>
    public class NearCacheOptions : Dictionary<string, NearCacheNamedOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NearCacheOptions"/> class.
        /// </summary>
        public NearCacheOptions()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NearCacheOptions"/> class.
        /// </summary>
        private NearCacheOptions(Dictionary<string, NearCacheNamedOptions> namedOptions)
            : base(namedOptions)
        { }

        /// <summary>
        /// Gets or sets the NearCache configuration pattern matcher.
        /// </summary>
        public IPatternMatcher PatternMatcher { get; set; } = new MatchingPointPatternMatcher();

        /// <summary>
        /// Gets a configuration.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A configuration matching the name.</returns>
        public NearCacheNamedOptions GetConfig(string name)
        {
            if (TryGetValue(name, out var configuration))
                return configuration;

            if (PatternMatcher == null)
                throw new InvalidOperationException("No pattern matcher has been defined.");

            var key = PatternMatcher.Matches(Keys, name);
            return key == null ? null : this[key];
        }

        /// <summary>
        /// Clones the options.
        /// </summary>
        public NearCacheOptions Clone()
        {
            var namedOptions = this.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Clone());

            return new NearCacheOptions(namedOptions);
        }
    }
}
