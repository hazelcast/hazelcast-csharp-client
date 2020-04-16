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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
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
    [Category("3.11")]
    public class ClientSSLTest : ClientSSLBaseTest
    {

        [Test]
        public void TestSSLEnabled_validateName_validName()
        {
            Setup(serverXml:Resources.hazelcast_ssl_signed,
                isSslEnabled:true,
                validateCertificateChain:true, 
                validateCertificateName:true, 
                checkCertificateRevocation:null, 
                certSubjectName:ValidCertNameSigned, 
                clientCertificate:null, 
                certPassword:null);
            Assert.True(Client.GetLifecycleService().IsRunning());
        }

        [Test]
        public void TestSSLEnabled_validateName_invalidName()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {            
                Setup(serverXml:Resources.hazelcast_ssl_signed, 
                    isSslEnabled:true, 
                    validateCertificateChain:true, 
                    validateCertificateName:true, 
                    checkCertificateRevocation:null, 
                    certSubjectName:"Invalid Cert Name", 
                    clientCertificate:null, 
                    certPassword:null);
            });
        }

        [Test]
        public void TestSSLEnabled_validateChain_DoNotValidateName_invalidName()
        {
            Setup(serverXml:Resources.hazelcast_ssl_signed, 
                isSslEnabled:true, 
                validateCertificateChain:true, 
                validateCertificateName:false, 
                checkCertificateRevocation:null, 
                certSubjectName:"Invalid Cert Name", 
                clientCertificate:null, 
                certPassword:null);
            Assert.True(Client.GetLifecycleService().IsRunning());
        }

        [Test]
        public void TestSSLEnabled_DoNotValidateChain_DoNotValidateName_invalidName()
        {
            Setup(serverXml:Resources.hazelcast_ssl_signed, 
                isSslEnabled:true, 
                validateCertificateChain:false, 
                validateCertificateName:false, 
                checkCertificateRevocation:null, 
                certSubjectName:"Invalid Cert Name", 
                clientCertificate:null, 
                certPassword:null);
            Assert.True(Client.GetLifecycleService().IsRunning());
        }

        [Test]
        public void TestSSLDisabled()
        {
            Setup(serverXml:Resources.hazelcast, 
                isSslEnabled:false, 
                validateCertificateChain:null, 
                validateCertificateName:null, 
                checkCertificateRevocation:null, 
                certSubjectName:null, 
                clientCertificate:null, 
                certPassword:null);
            Assert.True(Client.GetLifecycleService().IsRunning());
        }

        [Test]
        public void TestSSLEnabled_self_signed_remote_cert()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                Setup(serverXml:Resources.hazelcast_ssl, 
                    isSslEnabled:true, 
                    validateCertificateChain:null, 
                    validateCertificateName:null, 
                    checkCertificateRevocation:null, 
                    certSubjectName:null, 
                    clientCertificate:null, 
                    certPassword:null);
            });
        }
        
        [Test]
        public void TestSSLEnabled_signed_remote_cert()
        {
            Setup(serverXml:Resources.hazelcast_ssl_signed, 
                isSslEnabled:true, 
                validateCertificateChain:null, 
                validateCertificateName:null, 
                checkCertificateRevocation:null, 
                certSubjectName:null, 
                clientCertificate:null, 
                certPassword:null);
            Assert.True(Client.GetLifecycleService().IsRunning());
        }

        [Test]
        public void TestSSLEnabled_validateChain_validateName_validName()
        {
            Setup(serverXml:Resources.hazelcast_ssl_signed, 
                isSslEnabled:true, 
                validateCertificateChain:null, 
                validateCertificateName:true, 
                checkCertificateRevocation:null, 
                certSubjectName:ValidCertNameSigned, 
                clientCertificate:null, 
                certPassword:null);
            Assert.True(Client.GetLifecycleService().IsRunning());
        }
    }
}