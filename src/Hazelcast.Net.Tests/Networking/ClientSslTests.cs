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
using System.Threading.Tasks;
using Hazelcast.Testing;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Hazelcast.Tests.Networking
{
    [TestFixture]
    [Category("enterprise")]
    public class ClientSslTests : ClientSslTestBase
    {
        [Test]
        public async Task Test_NoSSL()
        {
            await using var client = await StartClientAsync(Hazelcast.Testing.Remote.Resources.hazelcast,
                false,
                true,
                true,
                null,
                ValidCertNameSigned,
                null,
                null);
        }

        [Test]
        public async Task TestSSLEnabled_validateName_validName()
        {
            await using var client = await StartClientAsync(Resources.Cluster_Ssl_Signed,
                true,
                true,
                true,
                null,
                ValidCertNameSigned,
                null,
                null);
        }

        [Test]
        public async Task TestSSLEnabled_validateName_invalidName()
        {
            await AssertEx.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await using var client = await StartClientAsync(Resources.Cluster_Ssl_Signed,
                    true,
                    true,
                    true,
                    null,
                    "Invalid Cert Name",
                    null,
                    null,
                    true);
            });
        }

        [Test]
        public async Task TestSSLEnabled_validateChain_DoNotValidateName_invalidName()
        {
            await using var client = await StartClientAsync(Resources.Cluster_Ssl_Signed,
                true,
                true,
                false,
                null,
                "Invalid Cert Name",
                null,
                null);
        }

        [Test]
        public async Task TestSSLEnabled_DoNotValidateChain_DoNotValidateName_invalidName()
        {
            await using var client = await StartClientAsync(Resources.Cluster_Ssl_Signed,
                true,
                false,
                false,
                null,
                "Invalid Cert Name",
                null,
                null);
        }

        [Test]
        public async Task TestSSLDisabled()
        {
            await using var client = await StartClientAsync(Resources.Cluster_Default,
                false,
                null,
                null,
                null,
                null,
                null,
                null);
        }

        [Test]
        public async Task TestSSLEnabled_self_signed_remote_cert()
        {
            await AssertEx.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await using var client = await StartClientAsync(Resources.Cluster_Ssl,
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
        public async Task TestSSLEnabled_signed_remote_cert()
        {
            await using var client = await StartClientAsync(Resources.Cluster_Ssl_Signed,
                true,
                null,
                null,
                null,
                null,
                null,
                null);
        }

        [Test]
        public async Task TestSSLEnabled_validateChain_validateName_validName()
        {
            await using var client = await StartClientAsync(Resources.Cluster_Ssl_Signed,
                true,
                null,
                true,
                null,
                ValidCertNameSigned,
                null,
                null);
        }
    }
}
