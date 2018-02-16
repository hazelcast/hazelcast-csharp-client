// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
