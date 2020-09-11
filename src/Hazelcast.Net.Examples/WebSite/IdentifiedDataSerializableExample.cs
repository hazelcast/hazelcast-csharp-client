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

using System.Threading.Tasks;
using Hazelcast.Serialization;

namespace Hazelcast.Examples.WebSite
{
    public class Employee : IIdentifiedDataSerializable
    {
        private const int ClassIdConst = 100;

        public int Id { get; set; }
        public string Name { get; set; }

        public void ReadData(IObjectDataInput input)
        {
            Id = input.ReadInt();
            Name = input.ReadString();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.Write(Id);
            output.Write(Name);
        }

        public int FactoryId => SampleDataSerializableFactory.FactoryId;

        public int ClassId => ClassIdConst;
    }

    public class SampleDataSerializableFactory : IDataSerializableFactory
    {
        public const int FactoryId = 1000;

        public IIdentifiedDataSerializable Create(int typeId)
        {
            if (typeId == 100) return new Employee();
            return null;
        }
    }

    // ReSharper disable once UnusedMember.Global
    public class IdentifiedDataSerializableExample : ExampleBase
    {
        public static async Task Run(string[] args)
        {
            // create an Hazelcast client and connect to a server running on localhost
            var options = BuildExampleOptions(args);
            options.Serialization.AddDataSerializableFactory(SampleDataSerializableFactory.FactoryId, new SampleDataSerializableFactory());
            await using var client = HazelcastClientFactory.CreateClient(options);
            await client.StartAsync();
        }
    }
}
