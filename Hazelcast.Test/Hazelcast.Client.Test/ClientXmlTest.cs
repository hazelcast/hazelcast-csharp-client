using System;
using System.IO;
using Hazelcast.Config;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientXmlTest
    {


        [Test]
        public virtual void TestXmlParserWithConfigFile()
        {
            ClientConfig clientConfig = XmlClientConfigBuilder.Build(@"..\..\..\Hazelcast.Net\Resources\hazelcast-client-full.xml");

            Assert.NotNull(clientConfig);
        }

        [Test]
        public virtual void TestXmlParserDefault()
        {
            ClientConfig clientConfig = XmlClientConfigBuilder.Build();

            Assert.NotNull(clientConfig);
        }
    }
}
