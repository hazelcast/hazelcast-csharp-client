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
using System.Security.Cryptography.X509Certificates;
using Hazelcast.Test;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    [Category("enterprise")]
    [Category("3.11")]
    public class ClientSslMutualAuthenticationTest : ClientSSLBaseTest
    {

        [Test]
        public void TestSSLEnabled_mutualAuthRequired_Server1KnowsClient1()
        {
            Setup(serverXml:Resources.hazelcast_ma_required,
                isSslEnabled:true,
                validateCertificateChain:true, 
                validateCertificateName:null, 
                checkCertificateRevocation:null, 
                certSubjectName:null, 
                clientCertificate:Resources.client1, 
                certPassword:Password);
            Assert.True(Client.GetLifecycleService().IsRunning());
        }

        [Test]
        public void TestSSLEnabled_mutualAuthRequired_Server1KnowsClient1_clientDoesNotProvideCerts()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                Setup(serverXml:Resources.hazelcast_ma_required,
                    isSslEnabled:true,
                    validateCertificateChain:true, 
                    validateCertificateName:null, 
                    checkCertificateRevocation:null, 
                    certSubjectName:null, 
                    clientCertificate:null, 
                    certPassword:null);
                Assert.True(Client.GetLifecycleService().IsRunning());
            });
        }

        [Test]
        public void TestSSLEnabled_mutualAuthRequired_Server1NotKnowsClient2()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                Setup(serverXml:Resources.hazelcast_ma_required,
                    isSslEnabled:true,
                    validateCertificateChain:true, 
                    validateCertificateName:null, 
                    checkCertificateRevocation:null, 
                    certSubjectName:null, 
                    clientCertificate:Resources.client2, 
                    certPassword:Password);
                Assert.True(Client.GetLifecycleService().IsRunning());
            });
        }

        [Test]
        public void TestSSLEnabled_mutualAuthOptional_Server1KnowsClient1()
        {
            Setup(serverXml:Resources.hazelcast_ma_optional,
                isSslEnabled:true,
                validateCertificateChain:true, 
                validateCertificateName:null, 
                checkCertificateRevocation:null, 
                certSubjectName:null, 
                clientCertificate:Resources.client1, 
                certPassword:Password);
            Assert.True(Client.GetLifecycleService().IsRunning());
        }

        [Test]
        public void TestSSLEnabled_mutualAuthOptional_Server1KnowsClient1_clientDoesNotProvideCerts()
        {
            Setup(serverXml:Resources.hazelcast_ma_optional,
                isSslEnabled:true,
                validateCertificateChain:true, 
                validateCertificateName:null, 
                checkCertificateRevocation:null, 
                certSubjectName:null, 
                clientCertificate:null, 
                certPassword:null);
            Assert.True(Client.GetLifecycleService().IsRunning());
        }

        [Test]
        public void TestSSLEnabled_mutualAuthOptional_Server1NotKnowsClient2()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                Setup(serverXml:Resources.hazelcast_ma_optional,
                    isSslEnabled:true,
                    validateCertificateChain:true, 
                    validateCertificateName:null, 
                    checkCertificateRevocation:null, 
                    certSubjectName:null, 
                    clientCertificate:Resources.client2, 
                    certPassword:Password);
                Assert.True(Client.GetLifecycleService().IsRunning());
            });
        }

        [Test]
        public void TestSSLEnabled_mutualAuthDisabled_Client1()
        {
            Setup(serverXml:Resources.hazelcast_ssl_signed,
                isSslEnabled:true,
                validateCertificateChain:true, 
                validateCertificateName:null, 
                checkCertificateRevocation:null, 
                certSubjectName:null, 
                clientCertificate:Resources.client1, 
                certPassword:Password);
            Assert.True(Client.GetLifecycleService().IsRunning());
        }

    }
}