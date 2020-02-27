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

using Hazelcast.Config;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Security;
using Hazelcast.Test;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    // This test requires a complete environment with a KDC.
    //
    // On .NET Core it also requires that the SRV record for the AD can be obtained
    // via a DNS query, meaning that the DNS *has* to be the AD DC somehow
    //
    // Note that at the moment, we do not support Kerberos on .NET Core.

    [TestFixture]
    [Explicit("Requires KDC and domain.")]
    public class KerberosTests : SingleMemberBaseTest
    {
        protected override void ConfigureClient(ClientConfig config)
        {
            base.ConfigureClient(config);

            // add serializer for User
            config.GetSerializationConfig().AddSerializerConfig(new SerializerConfig()
                .SetImplementation(new UserSerializer())
                .SetTypeClass(typeof(User)));

            // configure Kerberos - 3 ways

            // 1.
            // configure Kerberos credentials via the config file
            // <security>
            //   <credentials-factory class-name="Hazelcast.Security.KerberosCredentialsFactory">
            //     <properties>
            //       <property name="spn">hz/cluster1234</property>
            //     </properties>
            //   </credentials-factory>
            // </security>

            // 2.
            // configure Kerberos credentials with the current Windows principal - requires
            // that the tests run as part of the domain
            //config.ConfigureSecurity(security
            //    => security.ConfigureKerberosCredentials("hz/cluster1234"));

            // 3.
            // configure Kerberos credentials manually with the specified user - when tests
            // do not run as part of the domain
            config.ConfigureSecurity(security =>
                security.ConfigureCredentialsFactory(factory =>
                    factory.Implementation = new KerberosCredentialsFactory("hz/cluster1234",
                        "hzclient", "pAssw0rd", "hz.local")));
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

            var user = new User { Name = "qsdf" };
            var map = Client.GetMap<string, User>(TestSupport.RandomString());
            map.Put("x", user);
            var mapUser = map.Get("x");
            Assert.AreEqual(user.Name, mapUser.Name);
        }

        public class User
        {
            public string Name { get; set; }
        }

        public class UserSerializer : IStreamSerializer<User>
        {
            public void Destroy()
            {
            }

            public int GetTypeId()
            {
                return 123456;
            }

            public User Read(IObjectDataInput input)
            {
                return new User
                {
                    Name = input.ReadUTF()
                };
            }

            public void Write(IObjectDataOutput output, User obj)
            {
                output.WriteUTF(obj.Name);
            }
        }
    }
}