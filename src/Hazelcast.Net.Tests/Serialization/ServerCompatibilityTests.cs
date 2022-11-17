// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hazelcast.Testing;
using Hazelcast.Testing.Remote;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization
{
    /// <summary>
    /// Checks that object is deserialized server-side (Java) to the same value as was serialized client-side (.NET).
    /// </summary>
    [TestFixture]
    public class ServerCompatibilityTests : SingleMemberClientRemoteTestBase
    {
        [Test]
        public async Task GuidValueTest()
        {
            // we set a Guid in the map, and then use a script to retrieve it as a string
            // which means we are getting Java's representation of the Guid - and then we
            // parse it and compare it to the original Guid, thus ensuring that Java sees
            // the same Guid as .NET

            var mapName = CreateUniqueName();
            await using var map = await Client.GetMapAsync<string, Guid>(mapName);
            var guid = Guid.NewGuid();
            await map.SetAsync("key", guid);

            var script = $@"
                result = """" + instance_0.getMap(""{mapName}"").get(""key"")
            ";

            var response = await RcClient.ExecuteOnControllerAsync(RcCluster.Id, script, Lang.JAVASCRIPT);
            Assert.That(response.Result, Is.Not.Null);
            var resultString = Encoding.UTF8.GetString(response.Result, 0, response.Result.Length);
            Console.WriteLine(guid.ToString("D"));
            Console.WriteLine(resultString);
            Assert.That(Guid.TryParse(resultString, out var resultGuid));
            Assert.That(resultGuid, Is.EqualTo(guid));
        }

        [Test]
        public async Task GuidKeyTest()
        {
            // we use a Guid as a key, and then use a script to retrieve the value for
            // that key, passing the key as a string - and we ensure that we indeed get
            // a value, thus ensuring that Java sees the same Guid as .NET

            var mapName = CreateUniqueName();
            await using var map = await Client.GetMapAsync<Guid, string>(mapName);
            var guid = Guid.NewGuid();
            await map.SetAsync(guid, "value");

            var script = $@"
                var UUID = Java.type(""java.util.UUID"")
                var key = UUID.fromString(""{guid:D}"")
                result = """" + instance_0.getMap(""{mapName}"").get(key)
            ";

            var response = await RcClient.ExecuteOnControllerAsync(RcCluster.Id, script, Lang.JAVASCRIPT);
            Assert.That(response.Result, Is.Not.Null);
            var resultString = Encoding.UTF8.GetString(response.Result, 0, response.Result.Length);
            Console.WriteLine(resultString);
            Assert.That(resultString, Is.EqualTo("value"));
        }
    }
}
