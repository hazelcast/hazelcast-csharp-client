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
using System.Resources;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Remote;
using Hazelcast.Test;
using NUnit.Framework;


namespace Hazelcast.Client.Test
{
    public abstract class ClientSSLBaseTest : HazelcastTestSupport
    {
        protected const string ValidCertNameSigned = "foobar.hazelcast.com";
        protected const string Password = "password";

        protected IHazelcastInstance Client { get; set; }
        protected IRemoteController RemoteController { get; set; }

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
                catch (Exception) {}
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
            var clientConfig = new Configuration();
            clientConfig.NetworkConfig.AddAddress("localhost:5701");
            clientConfig.NetworkConfig.ConfigureSSL(sslConfig =>
            {
                ConfigSslConfig(sslConfig, isSslEnabled, validateCertificateChain,
                    validateCertificateName, checkCertificateRevocation, certSubjectName, clientCertificate, certPassword);
            });
            clientConfig.ClusterName = cluster.Id;
            Client = HazelcastClient.NewHazelcastClient(clientConfig);
        }

        private static SSLConfig ConfigSslConfig(SSLConfig sslConfig, bool isSslEnabled, bool? validateCertificateChain,
            bool? validateCertificateName, bool? checkCertificateRevocation, string certSubjectName, byte[] clientCertificate,
            string certPassword)
        {
            sslConfig.Enabled = isSslEnabled;
            if (clientCertificate != null)
            {
                var certFilePath = CreateTmpFile(clientCertificate);
                sslConfig.CertificateFilePath = certFilePath;
                if (certPassword != null)
                    sslConfig.CertificatePassword = certPassword;
            }
            if (validateCertificateChain != null)
                sslConfig.ValidateCertificateChain = validateCertificateChain.Value;
            if (validateCertificateName != null)
                sslConfig.ValidateCertificateName = validateCertificateName.Value;
            if (certSubjectName != null)
                sslConfig.CertificateName = certSubjectName;
            if (checkCertificateRevocation != null)
                sslConfig.CheckCertificateRevocation = checkCertificateRevocation.Value;
            return sslConfig;
        }

        private static string CreateTmpFile(byte[] cert)
        {
            var tmpFileName = Path.GetTempFileName();
            var fs = File.Open(tmpFileName, FileMode.Append);
            var bw = new BinaryWriter(fs);
            bw.Write(cert);
            bw.Close();
            fs.Close();
            return tmpFileName;
        }
    }
}