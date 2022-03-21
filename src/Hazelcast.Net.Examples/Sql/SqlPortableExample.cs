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
using System.Threading.Tasks;
using Hazelcast.Serialization;

namespace Hazelcast.Examples.Sql
{
    public class SqlPortableExample
    {

        static async Task Main(string[] args)
        {
            var options = new HazelcastOptionsBuilder()
            .With(args)
            .With(c => c.Networking.Addresses.Add("localhost:5701"))
            .WithConsoleLogger()
            .With("Logging:LogLevel:Hazelcast.Examples", "Information")
            .Build();

            //Registering the portable factory
            var factory = new PortableFactory();
            options.Serialization.AddPortableFactory(PortableFactory.FactoryId, factory);

            //Creating a client
            var client = await HazelcastClientFactory.StartNewClientAsync(options);

            //Map name that will be used in the example
            var mapName = "employeeMap";
            //Some data sample for the example
            var countries = new string[] { "Turkey", "United States", "United Kingdom" };

            //Creating the mapping to be able to query
            //Details: https://docs.hazelcast.com/hazelcast/latest/sql/create-mapping
            //And https://docs.hazelcast.com/hazelcast/latest/sql/mapping-to-maps#portable-objects
            await client.Sql.ExecuteCommandAsync($"CREATE OR REPLACE " +
                $"MAPPING {mapName}(" +
                    $"__key int, " +
                    $"id int," +
                    $"firstName varchar, " +
                    $"lastName varchar, " +
                    $"country varchar, " +
                    $"age int) " +
                $"TYPE IMap OPTIONS (" +
                    $"'keyFormat'='int', " +
                    $"'valueFormat' = 'portable', " +
                    $"'valuePortableFactoryId' = '1',    " +
                    $"'valuePortableClassId' = '1'" +
                $")");

            //creating/getting the map 
            var map = await client.GetMapAsync<int, Employee>(mapName);

            // Creating index via SQL 
            // Details: https://docs.hazelcast.com/hazelcast/latest/query/indexing-maps
            //await client.Sql.ExecuteCommandAsync($"CREATE INDEX IF NOT EXISTS employeeAge ON {mapName}(age); ");

            //Creating index programatically
            await map.AddIndexAsync(Hazelcast.Models.IndexType.Sorted, "age");

            //Seeding some sample data
            for (int i = 0; i < 1000; i++)
            {
                var employee = new Employee
                {
                    Age = i % 75,
                    Country = countries[i % 3],
                    FirstName = "Boby" + (i % 100),
                    LastName = "Womack" + i,
                    Id = i
                };

                _ = await map.PutAsync(i, employee);

                // Alternatively, insert via SQL
                //await client.Sql.ExecuteCommandAsync($"insert into {mapName} (__key, id, firstName, lastName, country, age) values (?,?,?,?,?,?)",
                //  new object[] { i, i, "Boby" + (i % 100), "Womack" + i, countries[i % 3], i % 75 });
            }

            // Queriying the oldest employees
            // "__key" is the key of the value. "this" is the whole value object which is Employee.
            // We need "this" to deserialize to Employee
            //https://docs.hazelcast.com/hazelcast/latest/sql/querying-maps-sql
            await using var oldestEmployees = await client.Sql.ExecuteQueryAsync($"select __key, this from {mapName} order by age desc limit 30");

            Console.WriteLine("ID \t First Name \t Last Name \t Age \t Country");

            await foreach (var row in oldestEmployees)
            {
                //object which is "this" column on the row will be deserialized to Employee since the factory and portable object registered and configured accordingly.
                var oldestEmployee = row.GetValue<Employee>();

                Console.WriteLine($"{oldestEmployee.Id} \t {oldestEmployee.FirstName} \t {oldestEmployee.LastName} \t {oldestEmployee.Age} \t {oldestEmployee.Country}");
            }

            /*
             Output:
                ID       First Name      Last Name       Age     Country
                449      Boby49          Womack449       74      United Kingdom
                599      Boby99          Womack599       74      United Kingdom
                224      Boby24          Womack224       74      United Kingdom
                524      Boby24          Womack524       74      United Kingdom
                749      Boby49          Womack749       74      United Kingdom
                74       Boby74          Womack74        74      United Kingdom
                149      Boby49          Womack149       74      United Kingdom
                899      Boby99          Womack899       74      United Kingdom
                824      Boby24          Womack824       74      United Kingdom
                299      Boby99          Womack299       74      United Kingdom
                974      Boby74          Womack974       74      United Kingdom
                374      Boby74          Womack374       74      United Kingdom
                674      Boby74          Womack674       74      United Kingdom
                223      Boby23          Womack223       73      United States
                673      Boby73          Womack673       73      United States
                973      Boby73          Womack973       73      United States
                523      Boby23          Womack523       73      United States
                373      Boby73          Womack373       73      United States
                73       Boby73          Womack73        73      United States
                823      Boby23          Womack823       73      United States
                448      Boby48          Womack448       73      United States
                598      Boby98          Womack598       73      United States
                148      Boby48          Womack148       73      United States
                898      Boby98          Womack898       73      United States
                298      Boby98          Womack298       73      United States
                748      Boby48          Womack748       73      United States
                447      Boby47          Womack447       72      Turkey
                372      Boby72          Womack372       72      Turkey
                822      Boby22          Womack822       72      Turkey
                222      Boby22          Womack222       72      Turkey
             */
        }

    }

    //See: https://docs.hazelcast.com/hazelcast/latest/serialization/implementing-portable-serialization
    //.Net Client: http://hazelcast.github.io/hazelcast-csharp-client/latest/doc/serialization.html#portable-serialization
    internal class Employee : IPortable
    {
        public const int ClassId = 1;
        int IPortable.ClassId => ClassId;
        int IPortable.FactoryId => PortableFactory.FactoryId;
        public int FactoryId => 1;

        public int Id { get; set; }
        public int Age { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Country { get; set; }

        public void ReadPortable(IPortableReader reader)
        {
            Id = reader.ReadInt("id");
            Country = reader.ReadString("country");
            FirstName = reader.ReadString("firstName");
            LastName = reader.ReadString("lastName");
            Age = reader.ReadInt("age");
        }

        public void WritePortable(IPortableWriter writer)
        {
            writer.WriteInt("id", Id);
            writer.WriteInt("age", Age);
            writer.WriteString("firstName", FirstName);
            writer.WriteString("lastName", LastName);
            writer.WriteString("country", Country);
        }
    }

    internal class PortableFactory : IPortableFactory
    {
        public const int FactoryId = 1;

        public IPortable Create(int classId)
        {
            if (classId == Employee.ClassId)
            {
                return new Employee();
            }

            return null;
        }
    }

}
