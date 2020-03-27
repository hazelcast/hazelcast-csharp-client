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
using Hazelcast.Config;
using Hazelcast.Test;
using NUnit.Framework;

namespace Hazelcast.Client.Test.Config
{
    [TestFixture]
    public class ClientXmlTest
    {
        [Test]
        public virtual void TestXmlParserDefault()
        {
            var clientConfig = XmlClientConfigBuilder.Build();

            Assert.NotNull(clientConfig);
        }

        [Test]
        public virtual void TestXmlParserWithConfigFile()
        {
            var xmlFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "Resources",
                "hazelcast-client-full.xml");
            var clientConfig = XmlClientConfigBuilder.Build(xmlFile);

            Assert.NotNull(clientConfig);
        }

        [Test]
        public virtual void TestXmlParserWithReader()
        {
            var clientConfig = XmlClientConfigBuilder.Build(new StringReader(Resources.HazelcastConfigFull));
            Assert.NotNull(clientConfig);
        }
    }
}