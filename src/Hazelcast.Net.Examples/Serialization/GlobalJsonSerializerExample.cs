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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Serialization;
using Newtonsoft.Json;

namespace Hazelcast.Examples.Serialization
{
    // ReSharper disable once UnusedMember.Global
    public class GlobalJsonSerializerExample
    {
        public static async Task Main(string[] args)
        {
            var options = new HazelcastOptionsBuilder()
                .With(args)
                .WithConsoleLogger()
                .Build();

            // register the global json serializer
            options.Serialization.GlobalSerializer.Creator = () => new GlobalJsonSerializer();

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            // get the distributed maps from the cluster
            await using var accountMap = await client.GetMapAsync<int, Account>("account-map");
            await using var customerMap = await client.GetMapAsync<int, Customer>("customer-map");

            //GlobalSerializer will serialize/deserialize all non-builtin types
            var account = new Account
            {
                Email = "james@example.com",
                Active = true,
                CreatedDate = new DateTime(2013, 1, 20, 0, 0, 0, DateTimeKind.Utc),
                Roles = new List<string> { "User", "Admin" }
            };

            var customer = new Customer
            {
                Id = 1001,
                Name = "James",
                Surname = "Doe",
                Birthday = new DateTime(1990, 5, 2, 0, 0, 0, DateTimeKind.Utc),
                AccountId = 1
            };

            await accountMap.PutAsync(1, account);
            await customerMap.PutAsync(1001, customer);

            Console.WriteLine(await accountMap.GetAsync(1));
            Console.WriteLine(await customerMap.GetAsync(1001));

            await accountMap.DestroyAsync();
            await customerMap.DestroyAsync();
        }

        public class GlobalJsonSerializer : IStreamSerializer<object>
        {
            public int TypeId => 20;

            public void Dispose()
            { }

            public void Write(IObjectDataOutput output, object obj)
            {
                var json = JsonConvert.SerializeObject(obj, obj.GetType(), Formatting.Indented,
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                output.WriteString(json);
            }

            public object Read(IObjectDataInput input)
            {
                var json = input.ReadString();
                var deserializeObject =
                    JsonConvert.DeserializeObject(json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
                return deserializeObject;
            }
        }

        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public DateTime Birthday { get; set; }
            public int AccountId { get; set; }

            public override string ToString()
            {
                return $"Id: {Id}, Name: {Name}, Surname: {Surname}, Birthday: {Birthday}, AccountId: {AccountId}";
            }
        }

        public class Account
        {
            public int Id { get; set; }
            public string Email { get; set; }
            public bool Active { get; set; }
            public DateTime CreatedDate { get; set; }
            public IList<string> Roles { get; set; }

            public override string ToString()
            {
                return $"Id: {Id}, Email: {Email}, Active: {Active}, CreatedDate: {CreatedDate}, Roles: {string.Join(", ", Roles)}";
            }
        }
    }
}
