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

using System.Threading.Tasks;
using Hazelcast.Serialization;
using Hazelcast.Testing;
using Hazelcast.Testing.Conditions;
using NUnit.Framework;

namespace Hazelcast.Tests.Networking
{
    // This test requires a complete environment with a KDC.
    // It also requires proper configuration (see CreateHazelcastOptions below)
    //
    // On .NET Core it also requires that the SRV record for the AD can be obtained
    // via a DNS query, meaning that the DNS *has* to be the AD DC somehow
    //
    // requires 4.1 server, as Kerberos is not supported on 4.0, so at least:
    // > build/build.sh -enterprise -server 4.1-SNAPSHOT rc

    [TestFixture]
    [Category("enterprise")] // Kerberos is an Enterprise feature
    [Explicit("Requires KDC and domain + configuration.")]
    [ServerCondition("[4.1,)")]
    public class KerberosTests : SingleMemberRemoteTestBase
    {
        // kerberos config can live in secrets
        protected override string ConfigurationSecretsKey => "hazelcast-tests-kerberos";

        protected override HazelcastOptions CreateHazelcastOptions()
        {
            var options = base.CreateHazelcastOptions();

            options.Serialization.Serializers.Add(new SerializerOptions
            {
                SerializedType = typeof(User),
                Creator = () => new UserSerializer()
            });

            // configure Kerberos - 3 ways

            // 1.
            // configure Kerberos credentials via the config file
            //
            // "hazelcast": {
            //   "authentication": {
            //     "credentialsFactory": {
            //       "typeName": "Hazelcast.Security.KerberosCredentialsFactory, Hazelcast.Net.Win32",
            //       "args": {
            //         "spn": "hz/cluster1234"
            //       }
            //     }
            //   }
            // }

            // 2.
            // configure Kerberos credentials via code, with the current Windows principal
            // (requires that the tests run as part of the domain)
            //
            //options.Authentication.ConfigureKerberosCredentials("hz/cluster1234");

            // 3.
            // configure Kerberos credentials via code, manually with the specified user
            // (when tests do not run as part of the domain)
            //
            //options.Authentication.CredentialsFactory.Creator = ()
            //    => new KerberosCredentialsFactory("hz/cluster1234", "hzclient", "pAssw0rd", "hz.local");

            // 4.
            // configure Kerberos via secrets - drop the following in secrets.json for the project,
            // which you can edit with "Manage User Secrets" in the Visual Studio contextual menu
            // of the project, thus making sure that the password does not end up in the Git repo
            /*
             *   "hazelcast-tests-kerberos": {
             *     "authentication": {
             *       "credentialsFactory": {
             *         "typeName": "Hazelcast.Security.KerberosCredentialsFactory, Hazelcast.Net.Win32",
             *         "args": {
             *           "spn": "hz/cluster1234",
             *           "username": "hzclient",
             *           "password": "***",
             *           "domain": "hz.local"
             *         }
             *       }
             *     }
             *   }
             */

            // => using option 4 here!

            return options;
        }

        // use the Kerberos configuration
        protected override string RcClusterConfiguration => Resources.Cluster_Kerberos;

        [Test]
        public async Task KerberosCredentials()
        {
            // do a normal IHDictionary operation
            // expect it to work with Kerberos authentication / authorization

            var client = await CreateAndStartClientAsync();
            var dictionary = await client.GetDictionaryAsync<string, User>(CreateUniqueName());
            var user = new User { Name = "Mr Random" };
            await dictionary.SetAsync("x", user);
            var user2 = await dictionary.GetAsync("x");
            Assert.AreEqual(user.Name, user2.Name);
        }

        public class User
        {
            public string Name { get; set; }
        }

        public class UserSerializer : IStreamSerializer<User>
        {
            public int TypeId => 123456;

            public User Read(IObjectDataInput input)
            {
                return new User
                {
                    Name = input.ReadString()
                };
            }

            public void Write(IObjectDataOutput output, User obj)
            {
                output.Write(obj.Name);
            }

            public void Destroy()
            { }
        }
    }
}