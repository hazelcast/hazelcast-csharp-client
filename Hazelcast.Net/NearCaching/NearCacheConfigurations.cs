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
