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
using System.Xml;
using Hazelcast.Core;
using Hazelcast.Security;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents the security configuration.
    /// </summary>
    public class SecurityConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityConfiguration"/> class.
        /// </summary>
        public SecurityConfiguration()
        {
            Authenticator = new ServiceFactory<IAuthenticator>(() => new Authenticator(this));
        }

        /// <summary>
        /// Gets or sets the credentials factory factory.
        /// </summary>
        public ServiceFactory<ICredentialsFactory> CredentialsFactory { get; } = new ServiceFactory<ICredentialsFactory>(() => new DefaultCredentialsFactory());

        /// <summary>
        /// Gets or sets the authenticator factory.
        /// </summary>
        public ServiceFactory<IAuthenticator> Authenticator { get; }

        /// <summary>
        /// Parses configuration from an Xml document.
        /// </summary>
        /// <param name="node">The Xml node.</param>
        /// <returns>The configuration.</returns>
        public static SecurityConfiguration Parse(XmlNode node)
        {
            var configuration = new SecurityConfiguration();

            foreach (XmlNode child in node.ChildNodes)
            {
                var nodeName = child.GetCleanName();
                if ("credentials-factory".Equals(nodeName))
                {
                    var classname = child.GetStringAttribute("class-name");
                    var properties = new Dictionary<string, string>();

                    foreach (XmlNode child2 in child.ChildNodes)
                    {
                        var nodeName2 = child2.GetCleanName();
                        if ("properties".Equals(nodeName2))
                        {
                            child2.FillProperties(properties);
                            break;
                        }
                    }

                    configuration.CredentialsFactory.Creator = () =>
                    {
                        var factory = Services.CreateInstance<ICredentialsFactory>(classname);
                        factory.Initialize(properties);
                        return factory;
                    };
                }
            }

            return configuration;
        }
    }
}