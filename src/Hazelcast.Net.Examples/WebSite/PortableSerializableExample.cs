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
using System.Threading.Tasks;
using Hazelcast.Serialization;

namespace Hazelcast.Examples.WebSite
{
    // ReSharper disable once UnusedMember.Global
    public class PortableSerializableExample : ExampleBase
    {
        public class Customer : IPortable
        {
            public const int TheClassId = 1;

            public string Name { get; set; }
            public int Id { get; set; }
            public DateTime LastOrder { get; set; }

            public int FactoryId => SamplePortableFactory.FactoryId;

            public int ClassId => TheClassId;

            public void WritePortable(IPortableWriter writer)
            {
                writer.WriteInt("id", Id);
                writer.WriteUTF("name", Name);
                writer.WriteLong("lastOrder", LastOrder.ToFileTimeUtc());
            }

            public void ReadPortable(IPortableReader reader)
            {
                Id = reader.ReadInt("id");
                Name = reader.ReadUTF("name");
                LastOrder = DateTime.FromFileTimeUtc(reader.ReadLong("lastOrder"));
            }
        }

        public class SamplePortableFactory : IPortableFactory
        {
            public const int FactoryId = 1;

            public IPortable Create(int classId)
            {
                if (classId == Customer.TheClassId)  return new Customer();
                return null;
            }
        }

        public async Task Run(string[] args)
        {
            // create an Hazelcast client and connect to a server running on localhost
            var options = BuildExampleOptions(args);
            options.Serialization.AddPortableFactory(SamplePortableFactory.FactoryId, new SamplePortableFactory());
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);
        }
    }
}
