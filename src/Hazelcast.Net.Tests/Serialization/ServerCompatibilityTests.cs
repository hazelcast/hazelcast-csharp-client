using System;
using System.Runtime.CompilerServices;
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
