// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Hazelcast.Serialization;

namespace Hazelcast.Examples.Map
{
    // ReSharper disable once UnusedMember.Global
    public class MapCustomSerializerExample
    {
        public static async Task Run()
        {
            static void Configure(HazelcastOptions configuration)
            {
                configuration.Serialization.Serializers.Add(new SerializerOptions
                {
                        SerializedType = typeof(Person),
                        Creator = () => new CustomSerializer()
                });
            }

            // create an Hazelcast client and connect to a server running on localhost
            var hz = new HazelcastClientFactory(HazelcastOptions.Build()).CreateClient(Configure);
            await hz.OpenAsync();

            var mapCustomers = await hz.GetMapAsync<string, Person>("persons");
            await mapCustomers.AddOrReplaceAsync("1", new Person("Joe", "Smith"));
            await mapCustomers.AddOrReplaceAsync("2", new Person("Ali", "Selam"));
            await mapCustomers.AddOrReplaceAsync("3", new Person("Avi", "Noyan"));

            var persons = await mapCustomers.GetValuesAsync();
            foreach (var person in persons)
            {
                Console.WriteLine(person.ToString());
            }

            // destroy the map
            mapCustomers.Destroy();

            // terminate the client
            await hz.DisposeAsync();
        }
    }

    internal class CustomSerializer : IStreamSerializer<Person>
    {
        public int GetTypeId()
        {
            return 10;
        }

        public void Destroy()
        {
            //NOOP
        }

        public void Write(IObjectDataOutput output, Person t)
        {
            byte[] array;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, t);
                array = ms.ToArray();
            }

            output.WriteInt(array.Length);
            output.Write(array);
        }

        public Person Read(IObjectDataInput input)
        {
            var bf = new BinaryFormatter();
            var len = input.ReadInt();

            var buffer = new byte[len];
            input.ReadFully(buffer);

            using var ms = new MemoryStream(buffer);
            return (Person) bf.Deserialize(ms);
        }
    }

    [Serializable]
    public class Person
    {
        public Person(string name, string surname)
        {
            Name = name;
            Surname = surname;
        }

        public string Name { get; set; }

        public string Surname { get; set; }

        public override string ToString()
        {
            return "Person{ name:" + Name + ", surname:" + Surname + " }";
        }
    }
}
