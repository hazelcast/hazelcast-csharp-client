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
using System.Collections.Generic;
using Hazelcast.Configuration;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides a strategy to match an item name to a configuration pattern.
    /// </summary>
    /// <remarks>
    /// <para>A pattern matcher is used to retrieve the configuration of a particular item, based
    /// upon its name. If no configuration matches, the pattern matcher returns <c>null</c>. If
    /// multiple configurations match, the pattern matcher throws a <see cref="ConfigurationException"/>.</para>
    /// </remarks>
    public interface IPatternMatcher
    {
        /// <summary>Gets the best match for an item name out of a list of configuration patterns.</summary>
        /// <param name="patterns">A list of configuration patterns.</param>
        /// <param name="name">The item name to match.</param>
        /// <returns>The element of the <paramref  name="patterns"/> list that best matches the item <paramref  name="name"/>, if any; otherwise <c>null</c>.</returns>
        /// <exception cref="ConfigurationException">Occurs when ambiguous configurations are found.</exception>
        string Matches(IEnumerable<string> patterns, string name);
    }
}
