// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Models;
using Hazelcast.Sql;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Examples.Sql
{
    // ReSharper disable once UnusedMember.Global
    public class SqlBasicQueryExample
    {
        public static async Task Main(params string[] args)
        {
            var options = new HazelcastOptionsBuilder()
                .With(args)
                .WithConsoleLogger()
                .With("Logging:LogLevel:Hazelcast.Examples", "Information")
                .Build();

            var logger = options.LoggerFactory.Service.CreateLogger<SqlBasicQueryExample>();

            // create a Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            // get the distributed map from the cluster and populate it
            await using var map = await client.GetMapAsync<int, string>(nameof(SqlBasicQueryExample));
            await map.SetAllAsync(Enumerable.Range(1, 10).ToDictionary(v => v, v => $"Value #{v}"));

            // query and print all rows
            {
                var allRows = await client.Sql.ExecuteQueryAsync($"SELECT __key, this FROM {map.Name}");

                var count = 1;
                await foreach (var row in allRows.EnumerateOnceAsync())
                    logger.LogInformation("Row #{RowCount}: {RowKey}, {RowValue}", count++, row.GetKey<int>(), row.GetValue<string>());
            }

            // query and print all rows sorted by key descending
            {
                // index must be added to be able to sort by attribute
                await map.AddIndexAsync(IndexType.Sorted, "__key");

                var allRows = await client.Sql.ExecuteQueryAsync($"SELECT __key, this FROM {map.Name} ORDER BY __key DESC");

                var count = 1;
                await foreach (var row in allRows.EnumerateOnceAsync())
                    logger.LogInformation("Row (sorted) #{RowCount}: {RowKey}, {RowValue}", count++, row.GetKey<int>(), row.GetValue<string>());
            }

            // query and print rows filtered via parameters
            {
                var (min, max) = (3, 7);
                var filteredRows = await client.Sql.ExecuteQueryAsync(
                    $"SELECT __key, this FROM {map.Name} WHERE __key >= ? and __key <= ?",
                    min, max
                );

                var count = 1;
                await foreach (var row in filteredRows.EnumerateOnceAsync())
                    logger.LogInformation("Row (filtered) #{RowCount}: {RowKey}, {RowValue}", count++, row.GetKey<int>(), row.GetValue<string>());
            }
        }
    }
}
