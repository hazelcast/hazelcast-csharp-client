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
using System.Xml;
using Hazelcast.Core;

namespace Hazelcast.NearCaching
{
    /// <summary>
    /// Represents the Near Cache configurations.
    /// </summary>
    public class NearCacheConfigurations
    {
        /// <summary>
        /// Gets or sets the NearCache configuration pattern matcher.
        /// </summary>
        public IPatternMatcher PatternMatcher { get; set; } = new MatchingPointPatternMatcher();

        /// <summary>
        /// Gets or sets the configurations.
        /// </summary>
        public Dictionary<string, NearCacheConfiguration> Configurations { get; set; } = new Dictionary<string, NearCacheConfiguration>();

        /// <summary>
        /// Gets a configuration.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A configuration matching the name.</returns>
        public NearCacheConfiguration GetConfig(string name)
        {
            if (Configurations.TryGetValue(name, out var configuration))
                return configuration;

            if (PatternMatcher == null)
                throw new InvalidOperationException("No pattern matcher has been defined.");

            var key = PatternMatcher.Matches(Configurations.Keys, name);
            return key == null ? null : Configurations[key];
        }

        /// <summary>
        /// Parses configuration from an Xml document.
        /// </summary>
        /// <param name="node">The Xml node.</param>
        /// <returns>The configuration.</returns>
        public static NearCacheConfigurations Parse(XmlNode node)
        {
            var configurations = new NearCacheConfigurations();

            var name = node.GetStringAttribute("name");
            var nearCacheConfig = new NearCacheConfiguration(name);
            foreach (XmlNode child in node.ChildNodes)
            {
                var nodeName = child.GetCleanName();
                switch (nodeName)
                {
                    case "max-size":
                        nearCacheConfig.MaxSize = child.GetInt32Content();
                        break;
                    case "time-to-live-seconds":
                        nearCacheConfig.TimeToLiveSeconds = child.GetInt32Content();
                        break;
                    case "max-idle-seconds":
                        nearCacheConfig.MaxIdleSeconds = child.GetInt32Content();
                        break;
                    case "eviction-policy":
                        nearCacheConfig.EvictionPolicy = child.GetEnumContent<EvictionPolicy>();
                        break;
                    case "in-memory-format":
                        nearCacheConfig.InMemoryFormat = child.GetEnumContent<InMemoryFormat>();
                        break;
                    case "invalidate-on-change":
                        nearCacheConfig.InvalidateOnChange = child.GetBoolContent();
                        break;
                }
            }
            configurations.Configurations.Add(name, nearCacheConfig);
            return configurations;
        }
    }
}
