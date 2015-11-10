// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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

using System.IO;
using Hazelcast.Config;
using Hazelcast.Test;
using NUnit.Framework;

namespace Hazelcast.Client.Test.Config
{
    [TestFixture]
    public class ClientXmlTest
    {
        //[Test]
        public virtual void TestConfig()
        {
            var config = new ClientConfig();
            var networkConfig = new ClientNetworkConfig();
            networkConfig.SetAddresses(new[] {"127.0.0.1:5701"});
            config.SetNetworkConfig(networkConfig);
            config.SetGroupConfig(new GroupConfig("mike-local", "password"));
            var _client = HazelcastClient.NewHazelcastClient(config);


            Assert.NotNull(_client);

            _client.Shutdown();
        }

        [Test]
        public virtual void TestXmlParserDefault()
        {
            var clientConfig = XmlClientConfigBuilder.Build();

            Assert.NotNull(clientConfig);
        }

        [Test]
        public virtual void TestXmlParserWithConfigFile()
        {
            var clientConfig =
                XmlClientConfigBuilder.Build(@"..\..\..\Hazelcast.Net\Resources\hazelcast-client-full.xml");

            Assert.NotNull(clientConfig);
        }

        [Test]
        public virtual void TestXmlParserWithReader()
        {
            var clientConfig = XmlClientConfigBuilder.Build(new StringReader(Resources.hazelcast_config_full));
            Assert.NotNull(clientConfig);
        }
    }
}