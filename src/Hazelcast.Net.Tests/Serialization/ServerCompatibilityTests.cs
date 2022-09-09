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
        public async Task GuidTest()
        {
            var originalValue = Guid.NewGuid();
            var serverValueStr = await GetAsServerString(originalValue);

            Assert.That(Guid.TryParse(serverValueStr, out var serverValue));
            Assert.That(originalValue, Is.EqualTo(serverValue));
        }

        private async Task<string> GetAsServerString<T>(T value)
        {
            var (mapName, key) = (CreateUniqueName(), 0);
            await using var map = await Client.GetMapAsync<int, T>(mapName);
            await map.SetAsync(key, value);

            var script = $"result = \"\" + instance_0.getMap(\"{mapName}\").get({key})";
            var response = await RcClient.ExecuteOnControllerAsync(RcCluster.Id, script, Lang.JAVASCRIPT);
            return Encoding.UTF8.GetString(response.Result, 0, response.Result.Length);
        }
    }
}
