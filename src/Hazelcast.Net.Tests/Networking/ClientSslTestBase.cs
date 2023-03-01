// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

// SSL tests require the following certificate files:
//
// - a client1.pfx that contains client1's standalone certificate
// - a client2.pfx that contains client2's standalone certificate
// - a server truststore that contains the trusted client certificates
//   for our tests, server truststore contains client1.pfx, not client2.pfx,
//   so the server can mutual-auth with client1 but not with client2
// - a server keystore that contains the server's signed certificate.
//   for our tests it can ben the original letsencrypt.jks which is signed with a trusted
//   root, or our own keystore which is signed with our own root CA -- but then that root
//   CA must be installed on the machine as a trusted root (hz.ps1 install-root-ca).
//
// These files can be generate with the hz.ps1 generate-certs command, which does:
//
// - create the server signed keystore
//     gen-req -> root-ca.csr (signing request) + root-ca.key (private key)
//     self-sign -> root-ca.crt (self-signed cert)
//     gen-req -> sub-ca.csr (signing request) + sub-ca.key (private key)
//     sign w/root-ca.crt -> sub-ca.crt (signed-by-root cert)
//     gen-req -> cluster1.csr (signing request) + cluster1.key (private key)
//     sign w/sub-ca.crt -> cluster1.crt (signed-by-sub cert)
//     import cluster1.crt + cluster1.key + sub-ca.crt + root-ca.crt -> keystore.jks
// - create the client certificates
//     gen -> clientN.keystore + clientN.cer (standalone)
//     import clientN.keystore -> clientN.pfx
// - create the server truststore
//     import client1.pfx -> server1.truststore
// - create the server non-signed keystore
//     gen -> server1.keystore + server1.cer
//
// And then there are 3 sets of constants that we can use for SSL tests, corresponding to
// which set of certificate files we want to use:
//
// JAVA corresponds to the original certificate files which were embedded in the enterprise
//   test jar, as copied from the enterprise repository, and now in our test resources, for
//   comparison purposes.
//
// MIXED corresponds to our generated files - except we still use the original letsencrypt.jks
//   for the server because it won't expire anytime soon, and it is signed with a trusted
//   root that is already installed on machines.
//
// CUSTOM corresponds to our generated files, including the new keystore.jks for the server,
//   but beware that the root-ca.crt must be installed on machines as this keystore is
//   signed by our own root self-signed CA. We do no use these by default, as the CA install
//   process requires privileges.
//
// Ideally, we would use CUSTOM constants, but the root CA installation process requires
// special privileges that we are not sure to have on every platform, and so for the time
// being we stick with MIXED by default. The mode can be overriden by a compile-time constant
// definition.

#if !SSLCERTS_JAVA && !SSLCERTS_MIXED && !SSLCERTS_CUSTOM
#define SSLCERTS_CUSTOM
#endif

using System;
using System.IO;
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

#if SSLCERTS_JAVA
        public const string ServerCertificateValidName = "foobar.hazelcast.com";
        public const string ServerCertificateInvalidName = "meh";
        public const string ClientCertificatePassword = "password";
        public const string ClientCertificatePath = "Certificates";
        public const string ClientCertificatePrefix = "java.";
        public const string Ssl_KeyStorePath = "Certificates/java.server1.keystore";
        public const string Ssl_KeyStorePassword = "password";
        public const string SslSigned_KeyStorePath = "Certificates/java.letsencrypt.jks";
        public const string SslSigned_KeyStorePassword = "123456";
        public const string SslMA_KeyStorePath = "Certificates/java.letsencrypt.jks";
        public const string SslMA_KeyStorePassword = "123456";
        public const string SslMA_TrustStorePath = "Certificates/java.server1.truststore";
        public const string SslMA_TrustStorePassword = "password";
#endif

#if SSLCERTS_MIXED
        public const string ServerCertificateValidName = "foobar.hazelcast.com";
        public const string ServerCertificateInvalidName = "meh";
        public const string ClientCertificatePassword = "123456";
        public const string ClientCertificatePath = "temp:certs/clients";
        public const string ClientCertificatePrefix = "";
        public const string Ssl_KeyStorePath = "temp:certs/server1/server1.keystore";
        public const string Ssl_KeyStorePassword = "123456";
        public const string SslSigned_KeyStorePath = "res:Certificates/java.letsencrypt.jks";
        public const string SslSigned_KeyStorePassword = "123456";
        public const string SslMA_KeyStorePath = "res:Certificates/java.letsencrypt.jks";
        public const string SslMA_KeyStorePassword = "123456";
        public const string SslMA_TrustStorePath = "temp:certs/server1/server1.truststore";
        public const string SslMA_TrustStorePassword = "123456";
#endif

#if SSLCERTS_CUSTOM
        public const string ServerCertificateValidName = "hz-2ef34a78.net";
        public const string ServerCertificateInvalidName = "meh";
        public const string ClientCertificatePassword = "123456";
        public const string ClientCertificatePath = "temp:certs";
        public const string ClientCertificatePrefix = "";
        public const string Ssl_KeyStorePath = "temp:certs/cluster1.standalone.keystore";
        public const string Ssl_KeyStorePassword = "123456";
        public const string SslSigned_KeyStorePath = "temp:certs/cluster1.signed2.keystore";
        public const string SslSigned_KeyStorePassword = "123456";
        public const string SslMA_KeyStorePath = "temp:certs/cluster1.signed2.keystore";
        public const string SslMA_KeyStorePassword = "123456";
        public const string SslMA_TrustStorePath = "temp:certs/cluster1.truststore";
        public const string SslMA_TrustStorePassword = "123456";
#endif

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
            var keyStoreFullPath = TestFiles.GetFullPath(this, keyStorePath);
            if (!File.Exists(keyStoreFullPath))
            {
                var message = $"KeyStore file not found at \"{keyStoreFullPath}\".";
                if (keyStorePath.StartsWith("temp:")) message += " Did you forget to generate the temp cert files?";
                throw new ArgumentException(message, nameof(keyStorePath));
            }
            AppendProperty(ssl, "keyStore", keyStoreFullPath);
            AppendProperty(ssl, "keyStorePassword", keyStorePassword);
            AppendProperty(ssl, "keyStoreType", "PKCS12");
            if (trustStorePath != null)
            {
                var trustStoreFullPath = TestFiles.GetFullPath(this, trustStorePath);
                if (!File.Exists(trustStoreFullPath))
                {
                    var message = $"TrustStore file not found at \"{trustStoreFullPath}\".";
                    if (trustStoreFullPath.StartsWith("temp:")) message += " Did you forget to generate the temp cert files?";
                    throw new ArgumentException(message, nameof(trustStorePath));
                }
                AppendProperty(ssl, "trustStore", trustStoreFullPath);
            }
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
            bool? validateCertificateChain = null,
            bool? validateCertificateName = null,
            bool? checkCertificateRevocation = null,
            string serverCertificateName = null,
            string clientCertificatePath = null,
            string clientCertificatePassword = null,
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
