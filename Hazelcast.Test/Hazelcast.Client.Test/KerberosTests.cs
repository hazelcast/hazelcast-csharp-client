// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Generic;
using Hazelcast.Config;
using Hazelcast.Security;
using Hazelcast.Test;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    // NOTES
    // this test requires that ...
    //
    // on .NET Core it also requires that the SRV record for the AD can be obtained
    // via a DNS query, meaning that the DNS *has* to be the AD DC somehow
    //
    // at the moment, it
    // - works on .NET Framework
    // - works on .NET Core

    [TestFixture]
    public class KerberosTests : SingleMemberBaseTest
    {
        protected override void ConfigureClient(ClientConfig config)
        {
            base.ConfigureClient(config);

            // add serializer for User
            config.GetSerializationConfig().AddSerializerConfig(new SerializerConfig()
                .SetImplementation(new ZpqrtBnkTests.UserSerializer())
                .SetTypeClass(typeof(ZpqrtBnkTests.User)));

            // configure Kerberos credentials with the current Windows principal
            // requires that the tests run as part of the domain
            //config.ConfigureKerberosCredentials("hzimdg/server19.hz.local");

            // configure Kerberos credentials with a specified username and password
            // which must correspond to a user in the domain
            var credentialsFactoryConfig = config.GetSecurityConfig().CredentialsFactoryConfig = new CredentialsFactoryConfig();
            credentialsFactoryConfig.Implementation = new KerberosCredentialsFactory();
            // FIXME this is confusing, if we set the implementation then "properties" are not used?
            credentialsFactoryConfig.Implementation.Init(new Dictionary<string, string>
            {
                ["spn"] = "hzimdg/server19.hz.local",
                ["timeout"] = "0",
                ["username"] = "Administrateur",
                ["password"] = "kErb!55",
                ["domain"] = "hz.local",
            });
            //credentialsFactoryConfig.Properties["spn"] = "hzimdg/server19.hz.local";
            ////credentialsFactoryConfig.Properties["timeout"] = "0";
            //credentialsFactoryConfig.Properties["username"] = "Administrateur";
            //credentialsFactoryConfig.Properties["password"] = "kErb!55";
            //credentialsFactoryConfig.Properties["domain"] = "hz.local";
        }

        protected override string GetServerConfig()
        {
            // use the Kerberos configuration
            return Resources.hazelcast_kerberos;
        }

        [Test]
        public void TestCreds()
        {
            // do a normal IMap operation
            // expect it to work with Kerberos authentication / authorization

            var user = new ZpqrtBnkTests.User { Name = "qsdf" };
            var map = Client.GetMap<string, ZpqrtBnkTests.User>(TestSupport.RandomString());
            map.Put("x", user);
            var mapUser = map.Get("x");
            Assert.AreEqual(user.Name, mapUser.Name);
        }
    }
}