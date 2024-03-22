// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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

using System.Text.Json;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Examples.Sql
{
    public class SqlJsonExample
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

            var logger = client.Options.LoggerFactory.CreateLogger<SqlJsonExample>();

            // get the distributed map from the cluster and populate it since we put json object,
            //HazelcastJsonValue is used as value.
            await using var map = await client.GetMapAsync<int, HazelcastJsonValue>(nameof(SqlJsonExample));

            //Before you can query data in a map, you need to create a mapping to one, using the map connector.
            //see details: https://docs.hazelcast.com/hazelcast/latest/sql/create-mapping
            await client.Sql.ExecuteCommandAsync($"CREATE MAPPING {map.Name} TYPE IMap OPTIONS ('keyFormat'='int', 'valueFormat'='json')");

            //Create and put some json objects.
            var student1 = new Student { Name = "Bertie Higgins", Id = 1 };
            await PutStudent(map, student1);

            var student2 = new Student { Name = "Bobay Womack", Id = 2 };
            await PutStudent(map, student2);

            //Query json objects on the map.
            var sqlResult = await client.Sql.ExecuteQueryAsync($"SELECT * FROM {map.Name} WHERE CAST(JSON_VALUE(this, '$.Id') AS TINYINT)  > 1");

            await foreach (var row in sqlResult)
            {
                var jsonValue = row.GetValue<HazelcastJsonValue>();
                var student = JsonSerializer.Deserialize<Student>(jsonValue.ToString());
                logger.LogInformation($"Id:{ student.Id},   Name:{student.Name}");
                // Output:
                // Id:2,   Name:Bobay Womack
            }
        }

        private static async Task PutStudent(IHMap<int, HazelcastJsonValue> map, Student student1)
        {
            //Object needs to be serialized to json.
            var json = JsonSerializer.Serialize(student1);
            var jsonObject = new HazelcastJsonValue(json);
            await map.PutAsync(student1.Id, jsonObject);
        }

        public class Student
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
