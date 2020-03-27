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
using System.IO;
using System.Xml;
using Hazelcast.Client.Properties;
using Hazelcast.Logging;
using Hazelcast.Util;

namespace Hazelcast.Config
{
    /// <summary>
    /// Loads the <see cref="Configuration"/> using XML.
    /// </summary>
    public class XmlClientConfigBuilder
    {
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof(XmlClientConfigBuilder));

        internal static XmlDocument ParseXmlConfig(TextReader reader)
        {
            try
            {
                using (reader)
                {
                    var document = new XmlDocument();
                    document.Load(reader);
                    return document;
                }
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Could not parse configuration file, giving up.");
            }
        }

        internal static XmlDocument ParseXmlConfig(string configFile = null)
        {
            if (configFile == null)
            {
                configFile = Environment.GetEnvironmentVariable("hazelcast.client.config");
            }

            TextReader input = null;
            if (configFile != null)
            {
                if (File.Exists(configFile))
                {
                    input = File.OpenText(configFile);
                }
                else
                {
                    var msg = "Config file at '" + configFile + "' doesn't exist.";
                    msg +=
                        "\nHazelcast will try to use the hazelcast-client.xml config file in the working directory.";
                    Logger.Warning(msg);
                }
            }

            if (input == null)
            {
                configFile = Path.Combine(Directory.GetCurrentDirectory(), "hazelcast-client.xml");
                if (File.Exists(configFile))
                {
                    input = File.OpenText(configFile);
                    Logger.Info("Using configuration file at working dir.");
                }
                else
                {
                    input = new StringReader(Resources.hazelcast_client_default);
                    Logger.Info("Using Default configuration file");
                }
            }

            try
            {
                using (input)
                {
                    var document = new XmlDocument();
                    document.Load(input);
                    return document;
                }
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Could not parse configuration file, giving up.");
            }
        }
        
        internal static Configuration Build(TextReader reader)
        {
            var xmlDocument = ParseXmlConfig(reader);
            var configuration = new Configuration();
            configuration.Build(xmlDocument);
            return configuration;
        }

        internal static Configuration Build(string configFile = null)
        {
            var xmlDocument = ParseXmlConfig(configFile);
            var configuration = new Configuration();
            configuration.Build(xmlDocument);
            return configuration;
        }
    }
}