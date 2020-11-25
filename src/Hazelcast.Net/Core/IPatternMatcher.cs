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
using Hazelcast.Configuration;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides a strategy to match an item name to a configuration pattern.
    /// </summary>
    /// <remarks>
    /// The IConfigPatternMatcher provides a strategy to match an item name to a configuration pattern.
    /// <p/>
    /// It is used on each Config.getXXXConfig() and ClientConfig.getXXXConfig() call for map, list, queue, set, executor, topic,
    /// semaphore etc., so for example <code>itemName</code> is the name of a map and <code>configPatterns</code> are all defined
    /// map configurations.
    /// <p/>
    /// If no configuration is found by the matcher it should return <tt>null</tt>. In this case the default config will be used
    /// for this item then. If multiple configurations are found by the matcher a
    /// <see cref="InvalidConfigurationException"/>
    /// should be thrown.
    /// <p/>
    /// Since Hazelcast 3.5 the default matcher is
    /// <see cref="MatchingPointPatternMatcher"/>
    /// .
    /// </remarks>
    public interface IPatternMatcher
    {
        /// <summary>Gets the best match for an item name out of a list of configuration patterns.</summary>
        /// <param name="patterns">A list of configuration patterns.</param>
        /// <param name="name">The item name to match.</param>
        /// <returns>The element of the <see cref="patterns"/> list that best matches the item <see cref="name"/>, if any; otherwise <c>null</c>.</returns>
        /// <exception cref="ConfigurationException">Occurs when ambiguous configurations are found.</exception>
        string Matches(IEnumerable<string> patterns, string name);
    }
}
