﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Networking;
using Hazelcast.Testing;
using Hazelcast.Testing.Logging;
using Hazelcast.Testing.Remote;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Cluster = Hazelcast.Testing.Remote.Cluster;

// ReSharper disable InconsistentNaming
// ReSharper disable StringLiteralTypo

namespace Hazelcast.Tests.Networking
{
    public abstract class ClientSslTestBase : RemoteTestBase
    {
        private IDisposable _console;

        protected static TimeSpan TestTimeout = TimeSpan.FromSeconds(30);

        // constants for the original certificate files, as copied from the enterprise repository
        /*
        public const string ServerCertificateValidName = "foobar.hazelcast.com";
        public const string ServerCertificateInvalidName = "meh";
        public const string ClientCertificatePassword = "password";
        public const string ClientCertificatePrefix = "java.";
        public const string Ssl_KeyStorePath = "java.server1.keystore";
        public const string Ssl_KeyStorePassword = "password";
        public const string SslSigned_KeyStorePath = "java.letsencrypt.jks";
        public const string SslSigned_KeyStorePassword = "123456";
        public const string SslMA_KeyStorePath = "java.letsencrypt.jks";
        public const string SslMA_KeyStorePassword = "123456";
        public const string SslMA_TrustStorePath = "java.server1.truststore";
        public const string SslMA_TrustStorePassword = "password";
        */

        // constants for the set of files generated by 'hz generate-certs'
        // but still using the original letsencrypt.jks for the server, which
        // is signed with a trusted root that is installed already on machines
        //
        public const string ServerCertificateValidName = "foobar.hazelcast.com";
        public const string ServerCertificateInvalidName = "meh";
        public const string ClientCertificatePassword = "123456";
        public const string ClientCertificatePrefix = "";
        public const string Ssl_KeyStorePath = "server1.keystore";
        public const string Ssl_KeyStorePassword = "123456";
        public const string SslSigned_KeyStorePath = "java.letsencrypt.jks";
        public const string SslSigned_KeyStorePassword = "123456";
        public const string SslMA_KeyStorePath = "java.letsencrypt.jks";
        public const string SslMA_KeyStorePassword = "123456";
        public const string SslMA_TrustStorePath = "server1.truststore";
        public const string SslMA_TrustStorePassword = "123456";

        // NOTE: constants below are here to show it *can* be done, but not too
        // sure yet it is practical as it requires admin access to machines in
        // order to install our root-ca.crt - ideally we would do this and
        // generate certs before each test runs, but...

        // constants for the set of files generated by 'hz generate-certs'
        // and using our keystore.jks for the server, which is signed with
        // our root-ca.crt which must be installed on machines, as admin
        // (see 'hz install-root-ca' / 'hz remove-root-ca')
        //
        /*
        public const string ServerCertificateValidName = "cluster1.hazelcast.meh";
        public const string ServerCertificateInvalidName = "meh";
        public const string ClientCertificatePassword = "123456";
        public const string ClientCertificatePrefix = "";
        public const string Ssl_KeyStorePath = "server1.keystore";
        public const string Ssl_KeyStorePassword = "123456";
        public const string SslSigned_KeyStorePath = "keystore.jks";
        public const string SslSigned_KeyStorePassword = "123456";
        public const string SslMA_KeyStorePath = "keystore.jks";
        public const string SslMA_KeyStorePassword = "123456";
        public const string SslMA_TrustStorePath = "server1.truststore";
        public const string SslMA_TrustStorePassword = "123456";
        */

        private static void AppendProperty(StringBuilder ssl, string propertyName, string propertyValue)
        {
            ssl.AppendLine($"<property name=\"{propertyName}\">{propertyValue}</property>");
        }

        private string GetServerXml(
            string keyStorePath,
            string keyStorePassword,
            string trustStorePath = null,
            string trustStorePassword = null,
            string mutualAuth = null
        )
        {
            var xml = TestFiles.ReadAllText(this, "Cluster/ssl.xml");

            var ssl = new StringBuilder();
            AppendProperty(ssl, "keyStore", TestFiles.GetFullPath(this, "Certificates/" + keyStorePath));
            AppendProperty(ssl, "keyStorePassword", keyStorePassword);
            if (trustStorePath != null) AppendProperty(ssl, "trustStore", TestFiles.GetFullPath(this, "Certificates/" + trustStorePath));
            if (trustStorePassword != null) AppendProperty(ssl, "trustStorePassword", trustStorePassword);
            if (mutualAuth != null) AppendProperty(ssl, "javax.net.ssl.mutualAuthentication", mutualAuth);
            AppendProperty(ssl, "keyManagerAlgorithm", "SunX509");
            AppendProperty(ssl, "protocol", "TLSv1.2");

            xml = xml
                .Replace("%%SSLCONTEXTFACTORY%%", "com.hazelcast.nio.ssl.BasicSSLContextFactory")
                .Replace("%%SSLPROPERTIES%%", ssl.ToString());

            return xml;
        }

        protected string GetServerXml_Ssl(bool signed)
            => GetServerXml(
                signed ? SslSigned_KeyStorePath : Ssl_KeyStorePath,
                signed ? SslSigned_KeyStorePassword : Ssl_KeyStorePassword);

        protected string GetServerXml_Ma(bool required)
            => GetServerXml(
                SslMA_KeyStorePath,
                SslMA_KeyStorePassword,
                SslMA_TrustStorePath,
                SslMA_TrustStorePassword,
                required ? "REQUIRED" : "OPTIONAL");

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
                    await RcClient.ShutdownClusterAsync(RcCluster).CfAwait();
                    RcCluster = null;
                }
                await RcClient.ExitAsync().CfAwait();
                RcClient = null;
            }
        }

        [SetUp]
        public void SetUp()
        {
            _console = HConsole.Capture(consoleOptions => consoleOptions
                .ClearAll()
                .Configure().SetMaxLevel()
                .Configure(this).SetPrefix("TEST")
                .Configure<AsyncContext>().SetMinLevel()
                .Configure<SocketConnectionBase>().SetIndent(1).SetLevel(0).SetPrefix("SOCKET")
                .Configure<ClientMessageConnection>().SetLevel(1)
                .Configure<MemberConnection>().SetLevel(1)
            );
        }

        [TearDown]
        public async Task TearDown()
        {
            _console?.Dispose();

            // terminate & remove member
            if (RcMember != null)
            {
                await RcClient.StopMemberAsync(RcCluster, RcMember);
                RcMember = null;
            }

            // terminate & remove cluster
            if (RcCluster != null)
            {
                await RcClient.ShutdownClusterAsync(RcCluster).CfAwait();
                RcCluster = null;
            }
        }

        protected async Task<IHazelcastClient> StartClientAsync(
            string serverXml,
            bool enableSsl,
            bool? validateCertificateChain,
            bool? validateCertificateName,
            bool? checkCertificateRevocation,
            string serverCertificateName,
            string clientCertificatePath,
            string clientCertificatePassword,
            bool failFast = false)
        {
            RcCluster = await RcClient.CreateClusterAsync(serverXml);
            RcMember = await RcClient.StartMemberAsync(RcCluster);

            var options = new HazelcastOptionsBuilder()
                .WithHConsoleLogger()
                .Build();

            options.Networking.Addresses.Clear();
            options.Networking.Addresses.Add("127.0.0.1:5701");
            options.ClusterName = RcCluster.Id;

            var sslOptions = options.Networking.Ssl;
            sslOptions.Enabled = enableSsl;
            sslOptions.ValidateCertificateChain = validateCertificateChain ?? sslOptions.ValidateCertificateChain;
            sslOptions.ValidateCertificateName = validateCertificateName ?? sslOptions.ValidateCertificateName;
            sslOptions.CertificateName = serverCertificateName ?? sslOptions.CertificateName;
            sslOptions.CheckCertificateRevocation = checkCertificateRevocation ?? sslOptions.CheckCertificateRevocation;

            if (failFast)
            {
                // default value is 20s but if we know we are going to fail, no point trying again and again
                options.Networking.ConnectionRetry.ClusterConnectionTimeoutMilliseconds = 2_000;
            }

            if (enableSsl && !string.IsNullOrWhiteSpace(clientCertificatePath))
            {
                sslOptions.CertificatePath = clientCertificatePath;
                if (clientCertificatePassword != null) sslOptions.CertificatePassword = clientCertificatePassword;

                // validate that the certificate is OK
                var sslLayer = new SslLayer(sslOptions, new NullLoggerFactory());
                sslLayer.GetClientCertificatesOrDefault(); // this should not throw
            }

            // note: if we let the test timeout (via the [Timeout] attribute) then HConsole
            // does not log + in some cases (?) NUnit will not even handle the timeout
            // properly, probably because "Specifically, once a test has timed out, no further
            // attempt is made to execute its teardown methods." - and so, we don't have
            // a test-level timeout but implement our own timeout here.

            return await HazelcastClientFactory.StartNewClientAsync(options).AsTask().CfAwait(TestTimeout);
        }
    }
}
