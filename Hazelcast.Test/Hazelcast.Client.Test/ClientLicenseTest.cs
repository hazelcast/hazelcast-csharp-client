using System;
using System.IO;
using Hazelcast.Config;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class ClientLicenseTest
    {


        [Test]
        public virtual void TestLicenseInXmlConfig()
        {
            string cfg = "<hazelcast-client>" +
                         "<license-key>"+HazelcastBaseTest.UNLIMITED_LICENSE+"</license-key>" +
                         "</hazelcast-client>";
            var reader = new StringReader(cfg);
            var clientConfig = XmlClientConfigBuilder.Build(reader);

            Assert.NotNull(clientConfig);
            Assert.AreEqual(HazelcastBaseTest.UNLIMITED_LICENSE, clientConfig.GetLicenseKey());
        }

    }
}
