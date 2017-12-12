using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Remote;
using Hazelcast.Test;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    [Category("enterprise")]
    public class ClientSSLTest : HazelcastTestSupport
    {
        private const string ValidCertName = "foo.bar.com";

        protected IHazelcastInstance Client { get; private set; }
        protected RemoteController.Client RemoteController { get; private set; }


        private ClientConfig ConfigureClient(bool ssl, bool validateCertificateChain, bool validateCertificateName, string certificateName)
        {
            var clientConfig = new ClientConfig();
            clientConfig.GetNetworkConfig().AddAddress("localhost:5701");
            clientConfig.GetNetworkConfig().GetSSLConfig().SetEnabled(ssl)
                .SetProperty(SSLConfig.CertificateName, certificateName)
                .SetProperty(SSLConfig.ValidateCertificateChain, validateCertificateChain.ToString())
                .SetProperty(SSLConfig.ValidateCertificateName, validateCertificateName.ToString());
            return clientConfig;
        }


        public void Setup(bool ssl, bool validateCertificateChain, bool validateCertificateName, string certificateName)
        {
            RemoteController = CreateRemoteController();
            var cluster = CreateCluster(RemoteController, ssl?Resources.hazelcast_ssl:Resources.hazelcast);
            RemoteController.startMember(cluster.Id);
            Client = HazelcastClient.NewHazelcastClient(ConfigureClient(ssl, validateCertificateChain, validateCertificateName, certificateName));
        }

        [TearDown]
        public void ShutdownRemoteController()
        {
            HazelcastClient.ShutdownAll();
            StopRemoteController(RemoteController);
        }

        [Test]
        public void TestMapSSLEnabled_validateName_validName()
        {
            Setup(true, false, true, ValidCertName);
            Assert.True(Client.GetLifecycleService().IsRunning());
        }


        [Test]
		public void TestMapSSLEnabled_validateName_invalideName()
		{
			Assert.Throws<InvalidOperationException>(() =>
        {
            Setup(true, false, true, "Invalid Cert Name");
        });
		}
 
        [Test]
		public void TestMapSSLEnabled_validateChain_DoNotValidateName_invalideName()
		{
			Assert.Throws<InvalidOperationException>(() =>
        {
            Setup(true, true, false, "Invalid Cert Name");
        });
		}
 
        [Test]
        public void TestMapSSLEnabled_DoNotValidateName()
        {
            Setup(true, false, false, "Invalid Cert Name");
            Assert.True(Client.GetLifecycleService().IsRunning());
        }
 
        [Test]
        public void TestMapSSLDisabled()
        {
            Setup(false, false, false, "IGNORE");
            Assert.True(Client.GetLifecycleService().IsRunning());
        }
        
    }
}
