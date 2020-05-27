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

using System.Xml;
using Hazelcast.Core;

namespace Hazelcast.Networking
{
    /// <summary>
    /// Represents the Hazelcast Cloud configuration.
    /// </summary>
    public class CloudConfiguration
    {
        /// <summary>
        /// Whether Hazelcast Cloud is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the discovery token of the cluster.
        /// </summary>
        public string DiscoveryToken { get; set; }

        /// <summary>
        /// Gets or sets the cloud url base.
        /// </summary>
        public string UrlBase { get; set; } = "https://coordinator.hazelcast.cloud";

        /// <summary>
        /// Parses configuration from an Xml document.
        /// </summary>
        /// <param name="node">The Xml node.</param>
        /// <returns>The configuration.</returns>
        public static CloudConfiguration Parse(XmlNode node)
        {
            var configuration = new CloudConfiguration();

            var enabledNode = node.Attributes.GetNamedItem("enabled");
            var enabled = enabledNode != null && enabledNode.GetTrueFalseContent();
            configuration.IsEnabled = enabled;
            foreach (XmlNode child in node.ChildNodes)
            {
                var nodeName = child.GetCleanName();
                if ("discovery-token".Equals(nodeName))
                {
                    configuration.DiscoveryToken = child.GetTextContent();
                }
            }

            return configuration;
        }
    }
}