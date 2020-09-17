﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Testing;
using Hazelcast.Testing.Remote;
using NUnit.Framework;

namespace Hazelcast.Tests.Networking
{
    public abstract class ClientSslTestBase : RemoteTestBase
    {
        protected const string ValidCertNameSigned = "foobar.hazelcast.com";
        protected const string Password = "password";

        protected IRemoteControllerClient RcClient { get; set; }

        protected Cluster RcCluster { get; set; }

        protected Member RcMember { get; set; }

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            RcClient = await ConnectToRemoteControllerAsync();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            // remove temp files
            foreach (var fileInfo in new DirectoryInfo(Path.GetTempPath()).GetFiles())
            {
                try { fileInfo.Delete(); }
                catch (Exception) { /* ignore */ }
            }

            // terminate & remove member (just in case)
            if (RcMember != null)
            {
                await RcClient.StopMemberAsync(RcCluster, RcMember);
                RcMember = null;
            }

            // terminate & remove client (needed) and cluster (just in case)
            if (RcClient != null)
            {
                if (RcCluster != null)
                {
                    await RcClient.ShutdownClusterAsync(RcCluster).CAF();
                    RcCluster = null;
                }
                await RcClient.ExitAsync().CAF();
                RcClient = null;
            }
        }

        [TearDown]
        public async Task TearDown()
        {
            // terminate & remove member
            if (RcMember != null)
            {
                await RcClient.StopMemberAsync(RcCluster, RcMember);
                RcMember = null;
            }

            // terminate & remove cluster
            if (RcCluster != null)
            {
                await RcClient.ShutdownClusterAsync(RcCluster).CAF();
                RcCluster = null;
            }
        }

        protected async Task<IHazelcastClient> Setup(string serverXml, bool enableSsl, bool? validateCertificateChain,
            bool? validateCertificateName, bool? checkCertificateRevocation, string certSubjectName, byte[] clientCertificate,
            string certPassword, bool failFast = false)
        {
            RcCluster = await RcClient.CreateClusterAsync(serverXml);
            RcMember = await RcClient.StartMemberAsync(RcCluster);

            var options = HazelcastOptions.Build();
            options.Networking.Addresses.Clear();
            //options.Networking.Addresses.Add("localhost:5701");
            options.Networking.Addresses.Add("127.0.0.1:5701");
            options.ClusterName = RcCluster.Id;
            options.Logging.LoggerFactory.Creator = () => LoggerFactory;

            var sslOptions = options.Networking.Ssl;
            sslOptions.Enabled = enableSsl;
            sslOptions.ValidateCertificateChain = validateCertificateChain ?? sslOptions.ValidateCertificateChain;
            sslOptions.ValidateCertificateName = validateCertificateName ?? sslOptions.ValidateCertificateName;
            sslOptions.CertificateName = certSubjectName ?? sslOptions.CertificateName;
            sslOptions.CheckCertificateRevocation = checkCertificateRevocation ?? sslOptions.CheckCertificateRevocation;

            if (failFast)
            {
                // default value is 20s but if we know we are going to fail, no point trying again and again
                options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 2_000;
            }

            if (enableSsl && clientCertificate != null)
            {
                var certFilePath = CreateTmpFile(clientCertificate);
                sslOptions.CertificatePath = certFilePath;
                if (certPassword != null) sslOptions.CertificatePassword = certPassword;
            }

            return HazelcastClientFactory.CreateClient(options);
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