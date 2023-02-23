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
        // when mutual auth is required, the client must provide a cert, and the server must know about it
        [TestCase(true, true, true, true)]
        [TestCase(true, true, false, false)]
        [TestCase(true, false, true, false)]
        [TestCase(true, false, false, false)]

        // when mutual auth is optional, the client can provide a cert, and then the server must know about it
        [TestCase(false, true, true, true)]
        [TestCase(false, true, false, true)]
        [TestCase(false, false, true, false)]
        [TestCase(false, false, false, true)]

        public async Task MutualAuth(bool required, bool knowsClient, bool withCert, bool succeeds)
        {
            async ValueTask TryStartClientAsync()
            {
                var clientCertNumber = knowsClient ? 1 : 2;
                var clientCertPath = withCert
                    ? TestFiles.GetFullPath(this, ClientCertificatePath, $"{ClientCertificatePrefix}client{clientCertNumber}.pfx")
                    : null;

                await using var client = await StartClientAsync(
                    serverXml: GetServerXml_Ma(required: required),
                    enableSsl: true,
                    validateCertificateChain: true,
                    validateCertificateName: false,
                    clientCertificatePath: clientCertPath,
                    clientCertificatePassword: ClientCertificatePassword,
                    failFast: !succeeds // when we expect to fail, reduce timeouts, no point retrying
                );
            }

            if (succeeds)
            {
                await TryStartClientAsync();
            }
            else
            {
                await AssertEx.ThrowsAsync<ConnectionException>(TryStartClientAsync);
            }
        }

        // when mutual auth is disabled, the client cert does not matter
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]

        public async Task MutualAuthDisabled(bool knowsClient, bool withCert)
        {
            var clientCertNumber = knowsClient ? 1 : 2;
            var clientCertPath = withCert
                ? TestFiles.GetFullPath(this, ClientCertificatePath, $"{ClientCertificatePrefix}client{clientCertNumber}.pfx")
                : null;

            await using var client = await StartClientAsync(
                serverXml: GetServerXml_Ssl(signed: true),
                enableSsl: true,
                validateCertificateChain: true,
                validateCertificateName: null,
                clientCertificatePath: clientCertPath,
                clientCertificatePassword: ClientCertificatePassword);
        }
    }
}
