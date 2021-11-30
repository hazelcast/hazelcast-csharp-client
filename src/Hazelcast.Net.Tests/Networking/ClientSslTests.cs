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

using System;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Networking;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Networking
{
    [TestFixture]
    [Category("enterprise")]
    public class ClientSslTests : ClientSslTestBase
    {
        [Test]
        public async Task SSL_NotEnabled()
        {
            await using var client = await StartClientAsync(
                Hazelcast.Testing.Remote.Resources.hazelcast,
                false,
                true,
                true,
                null,
                ServerCertificateValidName,
                null,
                null);
        }

        [Test]
        public async Task SSL_ValidateName_ValidName()
        {
            await using var client = await StartClientAsync(
                GetServerXml_Ssl(signed: true),
                true,
                true,
                true,
                null,
                ServerCertificateValidName,
                null,
                null);
        }

        [Test]
        public async Task SSL_ValidateName_InvalidName()
        {
            await AssertEx.ThrowsAsync<ConnectionException>(async () =>
            {
                await using var client = await StartClientAsync(
                    GetServerXml_Ssl(signed: true),
                    true,
                    true,
                    true,
                    null,
                    ServerCertificateInvalidName,
                    null,
                    null,
                    true);
            });
        }

        [Test]
        public async Task SSL_ValidateChainNotName_InvalidName()
        {
            await using var client = await StartClientAsync(
                GetServerXml_Ssl(signed: true),
                true,
                true,
                false,
                null,
                ServerCertificateInvalidName,
                null,
                null);
        }

        [Test]
        public async Task SSL_ValidateNothing_InvalidName()
        {
            await using var client = await StartClientAsync(
                GetServerXml_Ssl(signed: true),
                true,
                false,
                false,
                null,
                ServerCertificateInvalidName,
                null,
                null);
        }

        [Test]
        public async Task TestSSLDisabled() // FIXME isn't this a duplicate?!
        {
            await using var client = await StartClientAsync(
                Resources.Cluster_Default,
                false,
                null,
                null,
                null,
                null,
                null,
                null);
        }

        [Test]
        public async Task SSL_SelfSignedRemoteCert()
        {
            await AssertEx.ThrowsAsync<ConnectionException>(async () =>
            {
                await using var client = await StartClientAsync(
                    GetServerXml_Ssl(signed: false),
                    true,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    true);
            });
        }

        [Test]
        public async Task SSL_SignedRemoteCert()
        {
            await using var client = await StartClientAsync(
                GetServerXml_Ssl(signed: true),
                true,
                null,
                null,
                null,
                null,
                null,
                null);
        }

        [Test]
        public async Task SSL_ValidateChainAndName_ValidName()
        {
            await using var client = await StartClientAsync(
                GetServerXml_Ssl(signed: true),
                true,
                null,
                true,
                null,
                ServerCertificateValidName,
                null,
                null);
        }
    }
}
