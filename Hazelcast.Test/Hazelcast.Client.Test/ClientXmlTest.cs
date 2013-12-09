using System;
using System.IO;
using Hazelcast.Config;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    class ClientXmlTest
    {


        [Test]
        public virtual void Test()
        {
            var streamReader = new StreamReader(@"..\..\..\Hazelcast.Net\Resources\hazelcast-config-full.xml");

            ClientConfig clientConfig = new XmlClientConfigBuilder(streamReader).Build();

            Assert.NotNull(clientConfig);
        }
    }
}
