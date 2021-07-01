using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Hazelcast.Tests.Sql
{
    [TestFixture]
    public class SqlServiceTests: SqlTestBase
    {
        [Test]
        public async Task Execute()
        {
            var sql = await Client.GetSqlServiceAsync();
            var result = await sql.ExecuteAsync($"SELECT * FROM {MapName}");

            var expectedValues = MapValues;
            var resultValues = result.EnumerateOnce().ToDictionary(r => r.GetKey<string>(), r => r.GetValue<int>());

            CollectionAssert.AreEquivalent(expectedValues, resultValues);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        [TestCase(100)]
        public async Task ExecuteWithIntParameter(int minValue)
        {
            var sql = await Client.GetSqlServiceAsync();
            var result = await sql.ExecuteAsync($"SELECT * FROM {MapName} WHERE this >= ?", new object[] { minValue });

            var expectedValues = MapValues.Where(p => p.Value >= minValue);
            var resultValues = result.EnumerateOnce().ToDictionary(r => r.GetKey<string>(), r => r.GetValue<int>());

            CollectionAssert.AreEquivalent(expectedValues, resultValues);
        }

        [Test]
        [TestCase("0")]
        [TestCase("1")]
        [TestCase("5")]
        [TestCase("100")]
        public async Task ExecuteWithStringParameter(string key)
        {
            var sql = await Client.GetSqlServiceAsync();
            var result = await sql.ExecuteAsync($"SELECT * FROM {MapName} WHERE __key == ?", new object[] { key });

            var expectedValues = MapValues.Where(p => p.Key == key);
            var resultValues = result.EnumerateOnce().ToDictionary(r => r.GetKey<string>(), r => r.GetValue<int>());

            CollectionAssert.AreEquivalent(expectedValues, resultValues);
        }

        [OneTimeSetUp]
        public async Task InitAll()
        {
            var map = await Client.GetMapAsync<string, int>(MapName);
            await map.SetAllAsync(MapValues);
        }

        [OneTimeTearDown]
        public async Task DisposeAll()
        {
            var map = await Client.GetMapAsync<string, int>(MapName);
            await map.ClearAsync();
        }
    }
}
