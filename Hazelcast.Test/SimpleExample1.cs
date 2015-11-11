// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Config;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Client.Example
{
    public class SimpleExample1
    {
        private static void Main111(string[] args)
        {
            var clientConfig = new ClientConfig();
            clientConfig.GetNetworkConfig().AddAddress("127.0.0.1");
            //clientConfig.GetNetworkConfig().AddAddress("10.0.0.2:5702");

            //Portable Serialization setup up for Customer CLass
            clientConfig.GetSerializationConfig()
                .AddPortableFactory(MyPortableFactory.FactoryId, new MyPortableFactory());


            var client = HazelcastClient.NewHazelcastClient(clientConfig);
            //All cluster operations that you can do with ordinary HazelcastInstance
            var mapCustomers = client.GetMap<string, Customer>("customers");
            mapCustomers.Put("1", new Customer("Joe", "Smith"));
            mapCustomers.Put("2", new Customer("Ali", "Selam"));
            mapCustomers.Put("3", new Customer("Avi", "Noyan"));


            var customer1 = mapCustomers.Get("1");
            Console.WriteLine(customer1);

            //ICollection<Customer> customers = mapCustomers.Values();
            //foreach (var customer in customers)
            //{
            //    //customer
            //}
        }
    }

    public class MyPortableFactory : IPortableFactory
    {
        public const int FactoryId = 1;

        public IPortable Create(int classId)
        {
            if (Customer.Id == classId)
                return new Customer();
            return null;
        }
    }

    public class Customer : IPortable
    {
        public const int Id = 5;
        private string name;
        private string surname;

        public Customer(string name, string surname)
        {
            this.name = name;
            this.surname = surname;
        }

        public Customer()
        {
        }


        public int GetFactoryId()
        {
            return MyPortableFactory.FactoryId;
        }

        public int GetClassId()
        {
            return Id;
        }

        public void WritePortable(IPortableWriter writer)
        {
            writer.WriteUTF("n", name);
            writer.WriteUTF("s", surname);
        }

        public void ReadPortable(IPortableReader reader)
        {
            name = reader.ReadUTF("n");
            surname = reader.ReadUTF("s");
        }
    }
}