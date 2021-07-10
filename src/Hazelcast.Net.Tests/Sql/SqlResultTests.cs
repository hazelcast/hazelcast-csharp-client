using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Hazelcast.Tests.Sql
{
    [TestFixture]
    public class SqlResultTests: SqlTestBase
    {
        [Test]
        public async Task EnumerateAfterDispose()
        {
            var sqlService = await Client.GetSqlServiceAsync();

            var result = await sqlService.ExecuteQueryAsync($"SELECT * FROM {MapName} LIMIT 1");
            await result.DisposeAsync();

            Assert.Throws<ObjectDisposedException>(() => result.EnumerateOnce());
        }

        [Test]
        public async Task EnumerateMultipleTimes()
        {
            var sqlService = await Client.GetSqlServiceAsync();

            var result = await sqlService.ExecuteQueryAsync($"SELECT * FROM {MapName} LIMIT 1");
            await result.MoveNextAsync();

            Assert.Throws<InvalidOperationException>(() => result.EnumerateOnce());
            Assert.Throws<InvalidOperationException>(() => result.EnumerateOnceAsync());
        }

        [Test]
        public async Task DisposeMultipleTimes()
        {
            var sqlService = await Client.GetSqlServiceAsync();

            var result = await sqlService.ExecuteQueryAsync($"SELECT * FROM {MapName} LIMIT 1");
            await result.DisposeAsync();

            Assert.DoesNotThrowAsync(async () =>
            {
                await result.DisposeAsync();
                await result.DisposeAsync();
            });
        }
    }
}
