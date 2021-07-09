using System;
using System.Threading.Tasks;
using Hazelcast.Sql;
using NUnit.Framework;

namespace Hazelcast.Tests.Sql
{
    [TestFixture]
    public class SqlResultTests: SqlTestBase
    {
        [Test]
        public async Task EnumerateThrowsAfterClose()
        {
            var sqlService = await Client.GetSqlServiceAsync();

            var result = await sqlService.ExecuteQueryAsync($"SELECT * FROM {MapName}");
            await result.DisposeAsync();

            Assert.Throws<ObjectDisposedException>(() => result.EnumerateOnce());
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(.5)]
        [TestCase(.33)]
        public async Task Enumerate(double pageSizeRatio)
        {
            var sqlService = await Client.GetSqlServiceAsync();

            var result = await sqlService.ExecuteQueryAsync($"SELECT * FROM {MapName}",
                options: new SqlStatementOptions { CursorBufferSize = (int)(MapValues.Count * pageSizeRatio) }
            );

            Assert.DoesNotThrowAsync(async () =>
            {
                await foreach (var row in result.EnumerateOnceAsync())
                    GC.KeepAlive(row);
            });
        }
    }
}
