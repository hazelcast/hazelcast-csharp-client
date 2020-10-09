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
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Exceptions;
using Hazelcast.Networking;
using Hazelcast.Testing;
using Hazelcast.Testing.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Networking
{
    [TestFixture]
    public class SslLayerTests
    {
        [Test]
        public async Task GetStreamAsync()
        {
            var options = new SslOptions();
            var ssl = new SslLayer(options, new NullLoggerFactory());

            await AssertEx.ThrowsAsync<ArgumentNullException>(async () => await ssl.GetStreamAsync(null));

            var stream = new MemoryStream();
            var sslStream = await ssl.GetStreamAsync(stream);
            Assert.That(sslStream, Is.SameAs(stream));

            options = new SslOptions { Enabled = true };
            ssl = new SslLayer(options, new NullLoggerFactory());

            await AssertEx.ThrowsAsync<ConnectionException>(async () => sslStream = await ssl.GetStreamAsync(stream));
        }

        [Test]
        public void ValidateCertificate()
        {
            var text = new StringBuilder();
            var loggerFactory = LoggerFactory.Create(builder => builder.AddStringBuilder(text));

            var sender = new object();
            var cert = new X509Certificate();
            var chain = new X509Chain();

            var options = new SslOptions { ValidateCertificateChain = true };
            var ssl = new SslLayer(options, loggerFactory);
            var policyErrors = SslPolicyErrors.RemoteCertificateChainErrors;
            var valid = ssl.ValidateCertificate(sender, cert, chain, policyErrors);

            Console.WriteLine(text.ToString());
            Assert.That(text.ToString(), Does.Contain("certificate error: RemoteCertificateChainErrors"));
            Assert.That(text.ToString(), Does.Contain("chain status:"));
            text.Clear();

            options = new SslOptions { ValidateCertificateChain = false };
            ssl = new SslLayer(options, loggerFactory);
            policyErrors = SslPolicyErrors.RemoteCertificateChainErrors;
            valid = ssl.ValidateCertificate(sender, cert, chain, policyErrors);

            Console.WriteLine(text.ToString());
            Assert.That(text.ToString(), Does.Contain("certificate errors (chain validation) ignored by client configuration"));
            text.Clear();

            options = new SslOptions { ValidateCertificateName = false };
            ssl = new SslLayer(options, loggerFactory);
            policyErrors = SslPolicyErrors.RemoteCertificateNameMismatch;
            valid = ssl.ValidateCertificate(sender, cert, chain, policyErrors);

            Console.WriteLine(text.ToString());
            Assert.That(text.ToString(), Does.Contain("certificate errors (name validation) ignored by client configuration"));
            text.Clear();

            options = new SslOptions { ValidateCertificateName = true };
            ssl = new SslLayer(options, loggerFactory);
            policyErrors = SslPolicyErrors.RemoteCertificateNameMismatch;
            valid = ssl.ValidateCertificate(sender, cert, chain, policyErrors);

            Console.WriteLine(text.ToString());
            Assert.That(text.ToString(), Does.Contain("certificate error: RemoteCertificateNameMismatch"));
            text.Clear();

            options = new SslOptions();
            ssl = new SslLayer(options, loggerFactory);
            policyErrors = SslPolicyErrors.RemoteCertificateNotAvailable;
            valid = ssl.ValidateCertificate(sender, cert, chain, policyErrors);

            Console.WriteLine(text.ToString());
            Assert.That(text.ToString(), Does.Contain("certificate error: RemoteCertificateNotAvailable"));
            text.Clear();
        }

        [Test]
        public void GetClientCertificateOrDefault()
        {
            var text = new StringBuilder();
            var loggerFactory = LoggerFactory.Create(builder => builder.AddStringBuilder(text));

            var path = Environment.CurrentDirectory;
            while (Path.GetFileName(path) != "src")
                path = Path.GetDirectoryName(path);

            path = Path.GetFullPath(path + "/Hazelcast.Net.Tests/Resources/Certificates/");
            Console.WriteLine("Path: " + path);

            var options = new SslOptions
            {
                CertificatePath = null
            };
            var ssl = new SslLayer(options, loggerFactory);
            var certs = ssl.GetClientCertificatesOrDefault();
            Assert.That(certs, Is.Null);

            options = new SslOptions
            {
                CertificateName = "cert1",
                CertificatePath = Path.Combine(path, "client1.pfx"),
                CertificatePassword = "password"
            };
            ssl = new SslLayer(options, loggerFactory);
            certs = ssl.GetClientCertificatesOrDefault();
            Assert.That(certs, Is.Not.Null);
            Assert.That(certs.Count, Is.EqualTo(1));

            options = new SslOptions
            {
                CertificateName = "cert-not",
                CertificatePath = Path.Combine(path, "client-not.pfx"),
                CertificatePassword = "password"
            };
            ssl = new SslLayer(options, loggerFactory);
            try
            {
                certs = ssl.GetClientCertificatesOrDefault();
                Assert.Fail("Expected an exception.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine(text.ToString());
            Assert.That(text.ToString(), Does.Contain("Failed to load client certificate"));
            text.Clear();
        }
    }
}
