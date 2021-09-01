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
    public class ServerDeserializationTests : SingleMemberClientRemoteTestBase
    {
        [Test]
        public async Task GuidTest()
        {
            var expected = Guid.NewGuid();
            var actualStr = await GetAsServerString(expected);

            Assert.AreEqual(expected, Guid.Parse(actualStr));
        }

        private async Task<string> GetAsServerString<T>(T value, [CallerMemberName] string callerName = default)
        {
            var (mapName, key) = (callerName, 0);
            await using var map = await Client.GetMapAsync<int, T>(mapName);
            await map.SetAsync(key, value);

            var script = $@"
            function foo() {{
                var map = instance_0.getMap(""{mapName}"");
                var res = map.get({key});
                if (res.getClass().isArray()) {{
                    return Java.from(res);
                }} else {{
                    return res;
                }}
            }}
            result = """"+foo();
            ";

            var response = await RcClient.ExecuteOnControllerAsync(RcCluster.Id, script, Lang.JAVASCRIPT);
            return Encoding.UTF8.GetString(response.Result, 0, response.Result.Length);
        }
    }
}
