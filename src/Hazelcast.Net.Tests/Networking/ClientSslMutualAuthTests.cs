// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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

using System.Threading.Tasks;
using Hazelcast.Exceptions;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Networking
{
    [TestFixture]
    [Category("enterprise")]
    public class ClientSslMutualAuthTests : ClientSslTestBase
    {
        [Test]
        public async Task MutualAuthRequired_ServerKnowsClient()
        {
            await using var client = await StartClientAsync(
                GetServerXml_Ma(required: true),
                true,
                true,
                null,
                null,
                null,
                TestFiles.GetFullPath(this, $"Certificates/{ClientCertificatePrefix}client1.pfx"),
                ClientCertificatePassword);
        }

        [Test]
        public async Task MutualAuthRequired_ServerKnowsClient_ClientDoesNotProvideCert()
        {
            var e = await AssertEx.ThrowsAsync<ConnectionException>(async () =>
            {
                await using var client = await StartClientAsync(
                    GetServerXml_Ma(required: true),
                    true,
                    true,
                    null,
                    null,
                    null,
                    null,
                    null,
                    true);
            });

            // TODO: assert e
        }

        [Test]
        public async Task MutualAuthRequired_ServerDoesNotKnowClient()
        {
            var e = await AssertEx.ThrowsAsync<ConnectionException>(async () =>
            {
                await using var client = await StartClientAsync(
                    GetServerXml_Ma(required: true),
                    true,
                    true,
                    null,
                    null,
                    null,
                    TestFiles.GetFullPath(this, $"Certificates/{ClientCertificatePrefix}client2.pfx"),
                    ClientCertificatePassword,
                    true);
            });
            // TODO: assert e
        }

        [Test]
        public async Task MutualAuthOptional_ServerKnowsClient()
        {
            await using var client = await StartClientAsync(
                GetServerXml_Ma(required: false),
                true,
                true,
                null,
                null,
                null,
                TestFiles.GetFullPath(this, $"Certificates/{ClientCertificatePrefix}client1.pfx"),
                ClientCertificatePassword);
        }

        [Test]
        public async Task MutualAuthOptional_ServerKnowsClient_ClientDoesNotProvideCert()
        {
            await using var client = await StartClientAsync(
                GetServerXml_Ma(required: false),
                true,
                true,
                null,
                null,
                null,
                null,
                null);
        }

        [Test]
        public async Task MutualAuthOptional_ServerDoesNotKnowClient()
        {
            var e = await AssertEx.ThrowsAsync<ConnectionException>(async () =>
            {
                await using var client = await StartClientAsync(
                    GetServerXml_Ma(required: false),
                    true,
                    true,
                    null,
                    null,
                    null,
                    TestFiles.GetFullPath(this, $"Certificates/{ClientCertificatePrefix}client2.pfx"),
                    ClientCertificatePassword,
                    true);
            });
            // TODO: assert e
        }

        [Test]
        public async Task MutualAuthDisabled()
        {
            await using var client = await StartClientAsync(
                GetServerXml_Ssl(signed: true),
                true,
                true,
                null,
                null,
                null,
                TestFiles.GetFullPath(this, $"Certificates/{ClientCertificatePrefix}client1.pfx"),
                ClientCertificatePassword);
        }
    }
}
