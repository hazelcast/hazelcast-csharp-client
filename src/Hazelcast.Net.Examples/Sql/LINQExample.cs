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
using System.Linq;
using System.Threading.Tasks;
using Hazelcast.Linq;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Examples.Sql
{
    public class LinqExample
    {
        public static async Task Main(string[] args)
        {
            var options = new HazelcastOptionsBuilder()
                .With(args)
                .WithConsoleLogger()
                .With("Logging:LogLevel:Hazelcast", "Debug") // To see SQL statement on logs.
                .With("Logging:LogLevel:Hazelcast.Examples", "Information")
                .Build();

            // Create a Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            var logger = client.Options.LoggerFactory.CreateLogger<LinqExample>();

            // In this example, we will create a map City to its Mayor.
            // We will de/serialize the object by Compact with zero config approach.
            // Please, visit Compact Serialization for more details.
            // https://docs.hazelcast.com/hazelcast/5.2/serialization/compact-serialization
            // http://hazelcast.github.io/hazelcast-csharp-client/latest/doc/serialization.html#compact-serialization

            // Get a map with City to Mayor type.
            var map = await client.GetMapAsync<City, Mayor>("cityMayor");

            // Put some data into it.
            var cityAnkara = new City(1,"Ankara",true);
            var mayorAnkara = new Mayor() {MayorName = "Zeki Müren", Age = 35};
            await map.PutAsync(cityAnkara, mayorAnkara);

            var cityDC = new City(2,  "Washington, DC", true);
            var mayorDC = new Mayor() {MayorName = "Marvin Gaye", Age = 45};
            await map.PutAsync(cityDC, mayorDC);

            var cityCA = new City( 3,  "California", false);
            var mayorCA = new Mayor() {MayorName = "Dave Brubeck", Age = 92};
            await map.PutAsync(cityCA, mayorCA);

            // Create mapping to be able to run SQL queries over the map.
            // More details: https://docs.hazelcast.com/hazelcast/latest/sql/mapping-to-maps
            // Note: Column names are case sensitive. Hazelcast Linq provider passes property names as it is. 
            await client.Sql.ExecuteCommandAsync(
                "CREATE OR REPLACE MAPPING cityMayor (" +
                "Code INT EXTERNAL NAME \"__key.Code\"," +
                "CityName VARCHAR EXTERNAL NAME \"__key.CityName\"," +
                "IsCapital BOOLEAN EXTERNAL NAME \"__key.IsCapital\"," +
                "MayorName VARCHAR," +
                "Age INT) " +
                "TYPE IMap OPTIONS (" +
                "'keyFormat' = 'compact'," +
                "'keyCompactTypeName' = 'city'," +
                "'valueFormat' = 'compact'," +
                "'valueCompactTypeName' = 'mayor')"
            );

            // Now, we can run SQL queries.
            var capitalsAndMayors = map.AsAsyncQueryable()
                .Where(p => p.Key.IsCapital)
                .Select(p => new {City = p.Key.CityName, Mayor = p.Value.MayorName});

            await foreach (var entry in capitalsAndMayors)
                logger.LogInformation($"City: {entry.City}, Mayor: {entry.Mayor}");

            // SQL Statement: "SELECT m0.CityName, m0.MayorName FROM cityMayor m0 WHERE m0.IsCapital != FALSE"
            // Output:
            // City: Ankara, Mayor: Zeki Müren
            // City: Washington, DC, Mayor: Marvin Gaye


            var youngMayors = map.AsAsyncQueryable()
                .Where(p => p.Value.Age < 50);

            await foreach (var entry in youngMayors)
                logger.LogInformation($"City: {entry.Key.CityName}, Mayor: {entry.Value.MayorName}, Age: {entry.Value.Age}");

            // SQL Statement: "SELECT m0.CityName, m0.Code, m0.IsCapital, m0.MayorName, m0.Age FROM cityMayor m0 WHERE (m0.Age < ?)"
            // Output:
            // City: Ankara, Mayor: Zeki Müren, Age: 35
            // City: Washington, DC, Mayor: Marvin Gaye, Age: 45

            var zMuren = map.AsAsyncQueryable()
                .Where(p => p.Value.MayorName == "Zeki Müren")
                .Select(p => p.Value.MayorName);

            await foreach (var entry in zMuren)
                logger.LogInformation($"Mayor: {entry}");

            // SQL Statement: "SELECT m0.MayorName FROM cityMayor m0 WHERE (m0.MayorName = ?)"
            // Output:
            // Mayor: Zeki Müren
        }


        public record City(int Code, string CityName, bool IsCapital)
        {
            // To construct the City object, there is need for a default constructor.
            public City() : this(0, "", false){} 
        };

        public class Mayor
        {
            public string MayorName { get; set; }
            public int Age { get; set; }
        }
    }
}

// It's fix for versions < .Net5. 
// https://developercommunity.visualstudio.com/t/error-cs0518-predefined-type-systemruntimecompiler/1244809
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit {}
}
