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
    public class ClientSslTests : ClientSslTestBase
    {
        [Test]
        public async Task InvalidChainAndName_SslNotEnabled_IsOk()
        {
            await using var client = await StartClientAsync(
                serverXml: Hazelcast.Testing.Remote.Resources.hazelcast,
                enableSsl: false,
                validateCertificateChain: true,
                validateCertificateName: true,
                serverCertificateName: ServerCertificateInvalidName
            );
        }

        // when the name is valid, every combination of validation is OK
        [TestCase(true, true, true, true)]
        [TestCase(true, false, true, true)]
        [TestCase(false, true, true, true)]
        [TestCase(false, false, true, true)]

        // when the name is invalid, fail if the name is to be validated
        [TestCase(true, true, false, false)]
        [TestCase(true, false, false, true)]
        [TestCase(false, true, false, false)]
        [TestCase(false, false, false, true)]

        public async Task ValidateName(bool validateChain, bool validateName, bool validName, bool succeeds)
        {
            async ValueTask TryStartClientAsync()
            {
                var certName = validName ? ServerCertificateValidName : ServerCertificateInvalidName;

                await using var client = await StartClientAsync(
                    serverXml: GetServerXml_Ssl(signed: true), // the chain is *always* valid here
                    enableSsl: true,
                    validateCertificateChain: validateChain,
                    validateCertificateName: validateName,
                    serverCertificateName: certName,
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

        // TODO: add certificate revocation tests

        // a valid chain is a chain that can be validated by the client
        // i.e. that is signed, by an ultimately trusted authority
        [TestCase(true, true, true)]
        [TestCase(true, false, false)]
        [TestCase(false, true, true)]
        [TestCase(false, false, true)]

        public async Task ValidateChain(bool validateChain, bool validChain, bool succeeds)
        {
            async ValueTask TryStartClientAsync()
            {
                await using var client = await StartClientAsync(
                    serverXml: GetServerXml_Ssl(signed: validChain),
                    enableSsl: true,
                    validateCertificateChain: validateChain,
                    validateCertificateName: false,
                    serverCertificateName: ServerCertificateValidName,
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
    }
}
