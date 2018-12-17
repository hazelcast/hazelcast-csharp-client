// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Client;
using Hazelcast.Config;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Newtonsoft.Json;

namespace Hazelcast.Examples.Serialization
{
    public class GlobalJsonSerializer : IStreamSerializer<object>
    {
        public int GetTypeId()
        {
            return 20;
        }

        public void Destroy()
        {
        }

        public void Write(IObjectDataOutput output, object obj)
        {
            var json = JsonConvert.SerializeObject(obj, obj.GetType(), Formatting.Indented,
                new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All});
            output.WriteUTF(json);
        }

        public object Read(IObjectDataInput input)
        {
            var json = input.ReadUTF();
            var deserializeObject =
                JsonConvert.DeserializeObject(json, new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All});
            return deserializeObject;
        }
    }

    public class GlobalSerializerSample
    {
        public static void Run(string[] args)
        {
            var clientConfig = new ClientConfig();
            clientConfig.GetSerializationConfig().SetGlobalSerializerConfig(
                new GlobalSerializerConfig().SetImplementation(new GlobalJsonSerializer()));
            // Start the Hazelcast Client and connect to an already running Hazelcast Cluster on 127.0.0.1
            var client1 = HazelcastClient.NewHazelcastClient(clientConfig);

            //GlobalSerializer will serialize/deserialize all non-builtin types 
            var account = new Account
            {
                Email = "james@example.com",
                Active = true,
                CreatedDate = new DateTime(2013, 1, 20, 0, 0, 0, DateTimeKind.Utc),
                Roles = new List<string> {"User", "Admin"}
            };

            var customer = new Customer
            {
                Id = 1001,
                Name = "James",
                Surname = "Doe",
                Birthday = new DateTime(1990, 5, 2, 0, 0, 0, DateTimeKind.Utc),
                AccountId = 1
            };
            var accountMap = client1.GetMap<int, Account>("accountMap");
            var customerMap = client1.GetMap<int, Customer>("customerMap");

            accountMap.Put(1, account);
            customerMap.Put(1001, customer);

            Console.WriteLine(accountMap.Get(1));
            Console.WriteLine(customerMap.Get(1001));
            
            // Shutdown this Hazelcast Client
            client1.Shutdown();
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
            return string.Format("Id: {0}, Name: {1}, Surname: {2}, Birthday: {3}, AccountId: {4}", Id, Name, Surname, Birthday,
                AccountId);
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
            return string.Format("Id: {0}, Email: {1}, Active: {2}, CreatedDate: {3}, Roles: {4}", Id, Email, Active, CreatedDate,
                string.Join(", ", Roles));
        }
    }
}