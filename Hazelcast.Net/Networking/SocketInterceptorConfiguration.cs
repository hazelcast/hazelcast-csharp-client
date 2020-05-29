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
using System.Text;
using System.Xml;
using Hazelcast.Core;

namespace Hazelcast.Networking
{
    /// <summary>
    /// Contains the configuration for interceptor socket.
    /// </summary>
    public class SocketInterceptorConfiguration
    {
        /// <summary>
        /// Whether socket interception is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        public ServiceFactory<ISocketInterceptor> SocketInterceptor { get; } = new ServiceFactory<ISocketInterceptor>();

        /// <inheritdoc />
        public override string ToString()
        {
            var text = new StringBuilder();
            text.Append("SocketInterceptorConfig");
            return text.ToString();
        }

        /// <summary>
        /// Parses configuration from an Xml document.
        /// </summary>
        /// <param name="node">The Xml node.</param>
        /// <returns>The configuration.</returns>
        public static SocketInterceptorConfiguration Parse(XmlNode node)
        {
            var configuration = new SocketInterceptorConfiguration();

            var atts = node.Attributes;
            var enabledNode = atts.GetNamedItem("enabled");
            var enabled = enabledNode != null && enabledNode.GetTrueFalseContent();
            configuration.IsEnabled = enabled;

            string classname = null;
            Dictionary<string, string> properties = null;

            foreach (XmlNode n in node.ChildNodes)
            {
                var nodeName = n.GetCleanName();
                if ("class-name".Equals(nodeName))
                {
                    classname = n.GetTextContent();
                }
                else
                {
                    if ("properties".Equals(nodeName))
                    {
                        properties = new Dictionary<string, string>();
                        n.FillProperties(properties);
                    }
                }
            }

            if (classname == null) return configuration;

            configuration.SocketInterceptor.Creator = () =>
            {
                var interceptor = Services.CreateInstance<ISocketInterceptor>(classname);
                if (properties != null) interceptor.Initialize(properties);
                return interceptor;
            };

            return configuration;
        }
    }
}
