// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.Remote;
using NUnit.Framework;


namespace Hazelcast.Client.Test
{
    public abstract class ClientSSLBaseTest : HazelcastTestSupport
    {
        protected const string ValidCertNameSigned = "foobar.hazelcast.com";
        protected const string Password = "password";

        protected IHazelcastInstance Client { get; set; }
        protected RemoteController.Client RemoteController { get; set; }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            var di = new DirectoryInfo(Path.GetTempPath());
            foreach (var fileInfo in di.GetFiles())
            {
                try
                {
                    fileInfo.Delete();
                }
                catch (Exception) { }
            }
        }

        [TearDown]
        public void ShutdownRemoteController()
        {
            HazelcastClient.ShutdownAll();
            StopRemoteController(RemoteController);
        }

        protected void Setup(string serverXml, bool isSslEnabled, bool? validateCertificateChain,
            bool? validateCertificateName, bool? checkCertificateRevocation, string certSubjectName, byte[] clientCertificate,
            string certPassword)
        {
            RemoteController = CreateRemoteController();
            var cluster = CreateCluster(RemoteController, serverXml);
            RemoteController.startMember(cluster.Id);
            var clientConfig = new ClientConfig();
            clientConfig.GetNetworkConfig().AddAddress("localhost:5701");
            clientConfig.GetNetworkConfig().SetSSLConfig(CreateSslConfig(isSslEnabled, validateCertificateChain,
                validateCertificateName, checkCertificateRevocation, certSubjectName, clientCertificate, certPassword));
            clientConfig.GetGroupConfig().SetName(cluster.Id).SetPassword(cluster.Id);
            Client = HazelcastClient.NewHazelcastClient(clientConfig);
        }

        private static SSLConfig CreateSslConfig(bool isSslEnabled, bool? validateCertificateChain, bool? validateCertificateName,
            bool? checkCertificateRevocation, string certSubjectName, byte[] clientCertificate, string certPassword)
        {
            var sslConfig = new SSLConfig();
            sslConfig.SetEnabled(isSslEnabled);
            if (clientCertificate != null)
            {
                var certFilePath = CreateTmpFile(clientCertificate);
                sslConfig.SetProperty(SSLConfig.CertificateFilePath, certFilePath);
                if (certPassword != null)
                    sslConfig.SetProperty(SSLConfig.CertificatePassword, certPassword);
            }
            if (validateCertificateChain != null)
                sslConfig.SetProperty(SSLConfig.ValidateCertificateChain, validateCertificateChain.ToString());
            if (validateCertificateName != null)
                sslConfig.SetProperty(SSLConfig.ValidateCertificateName, validateCertificateName.ToString());
            if (certSubjectName != null)
                sslConfig.SetProperty(SSLConfig.CertificateName, certSubjectName);
            if (checkCertificateRevocation != null)
                sslConfig.SetProperty(SSLConfig.CheckCertificateRevocation, checkCertificateRevocation.ToString());
            return sslConfig;
        }

        private static string CreateTmpFile(byte[] cert)
        {
            var tmpFileName = Path.GetTempFileName();

            using (var fs = File.Open(tmpFileName, FileMode.Append))
            {
                fs.Write(cert, 0, cert.Length);
                fs.Flush(true);
            }

            return tmpFileName;
        }
    }
}