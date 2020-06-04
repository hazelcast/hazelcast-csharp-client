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
using Hazelcast.Exceptions;

namespace Hazelcast.Core
{
    internal class MatchingPointPatternMatcher : IPatternMatcher
    {
        public virtual string Matches(IEnumerable<string> configPatterns, string itemName)
        {
            string candidate = null;
            string duplicate = null;
            var lastMatchingPoint = -1;
            foreach (var pattern in configPatterns)
            {
                var matchingPoint = GetMatchingPoint(pattern, itemName);
                if (matchingPoint > -1 && matchingPoint >= lastMatchingPoint)
                {
                    duplicate = matchingPoint == lastMatchingPoint ? candidate : null;
                    lastMatchingPoint = matchingPoint;
                    candidate = pattern;
                }
            }
            if (duplicate != null)
            {
                throw new ConfigurationException(
                    $"Found ambiguous configurations for item \"{itemName}\": \"{candidate}\" vs. \"{duplicate}\"\nPlease specify your configuration.");
            }
            return candidate;
        }

        /// <summary>This method returns higher values the better the matching is.</summary>
        /// <param name="pattern">configuration pattern to match with</param>
        /// <param name="itemName">item name to match</param>
        /// <returns>-1 if name does not match at all, zero or positive otherwise</returns>
        private static int GetMatchingPoint(string pattern, string itemName)
        {
            var index = pattern.IndexOf('*');
            if (index == -1)
            {
                return -1;
            }
            var firstPart = pattern.Substring(0, index);
            if (!itemName.StartsWith(firstPart))
            {
                return -1;
            }
            var secondPart = pattern.Substring(index + 1);
            if (!itemName.EndsWith(secondPart))
            {
                return -1;
            }
            return firstPart.Length + secondPart.Length;
        }
    }
}
