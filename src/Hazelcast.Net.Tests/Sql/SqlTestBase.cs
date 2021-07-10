using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Models;
using Hazelcast.Testing;
using NUnit.Framework;

namespace Hazelcast.Tests.Sql
{
    public class SqlTestBase : SingleMemberClientRemoteTestBase
    {
        protected const string MapName = "MyMap";

        protected readonly Dictionary<string, int> MapValues = Enumerable.Range(1, 10)
            .ToDictionary(i => $"{i}", i => i);

        [OneTimeSetUp]
        protected async Task InitAll()
        {
            var map = await Client.GetMapAsync<string, int>(MapName);
            await map.AddIndexAsync(IndexType.Sorted, "__key");
            await map.SetAllAsync(MapValues);
        }

        [OneTimeTearDown]
        protected async Task DisposeAll()
        {
            var map = await Client.GetMapAsync<string, int>(MapName);
            await map.ClearAsync();
        }
    }
}