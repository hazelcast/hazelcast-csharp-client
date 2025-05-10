// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Examples.Sql
{
    // ReSharper disable once UnusedMember.Global
    public class SqlLinqEnumerationExample
    {
        public static async Task Main(params string[] args)
        {
            var options = new HazelcastOptionsBuilder()
                .With(args)
                .WithConsoleLogger()
                .With("Logging:LogLevel:Hazelcast.Examples", "Information")
                .Build();

            // create a Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            var logger = client.Options.LoggerFactory.CreateLogger<SqlBasicQueryExample>();

            // get the distributed map from the cluster and populate it
            await using var map = await client.GetMapAsync<int, string>(nameof(SqlBasicQueryExample));
            await map.SetAllAsync(Enumerable.Range(1, 10).ToDictionary(v => v, v => $"Value #{v}"));

            // LINQ over IAsyncEnumerable implemented by ISqlQueryResult is not provided by base .NET.
            // To use it, you will need to install separate package - System.Linq.Async (https://github.com/dotnet/reactive)
            // You can install it via Visual Studio Package Manager Console:
            // Install-Package System.Linq.Async
            // Or add directly to the project file, see Hazelcast.Net.Examples.csproj for example:
            // <PackageReference Include="System.Linq.Async" Version="5.0.0"
            
            //Before you can query data in a map, you need to create a mapping to one, using the map connector.
            //see details: https://docs.hazelcast.com/hazelcast/latest/sql/create-mapping
            await client.Sql.ExecuteCommandAsync(
                $"CREATE OR REPLACE MAPPING {map.Name} TYPE IMap OPTIONS ('keyFormat'='int', 'valueFormat'='varchar')");

            await using var result = await client.Sql.ExecuteQueryAsync($"SELECT __key, this FROM {map.Name}");

            var items = await result
                .Select(row => (key: row.GetKey<int>(), value: row.GetValue<string>()))
                .ToListAsync();

            var count = 0;
            foreach (var (key, value) in items)
                logger.LogInformation("Row #{RowCount}: {RowKey}, {RowValue}", ++count, key, value);

            // note: one might want to do something link:

            //// Get first 5 results
            //var batchOf5 = await result
            //    .Select(row => (key: row.GetKey<int>(), value: row.GetValue<string>()))
            //    .Take(5)
            //    .ToListAsync();
            //
            //// Get next 5 results
            //batchOf5 = await result
            //    .Select(row => (key: row.GetKey<int>(), value: row.GetValue<string>()))
            //    .ToListAsync();
            //
            // BUT: this cannot work because the result of a query cannot be enumerated more than once.
        }
    }
}
