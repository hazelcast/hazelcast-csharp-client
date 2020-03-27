﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Client;
using Hazelcast.Config;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Examples.Org.Website.Samples
{
    public class PortableSerializableSample
    {
        public class Customer : IPortable
        {
            public const int ClassId = 1;

            public string Name { get; set; }
            public int Id { get; set; }
            public DateTime LastOrder { get; set; }

            public int GetFactoryId()
            {
                return SamplePortableFactory.FactoryId;
            }

            public int GetClassId()
            {
                return ClassId;
            }

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
                if (classId == Customer.ClassId)  return new Customer();
                return null;
            }
        }

        public static void Run(string[] args)
        {
            var clientConfig = new Configuration();
            clientConfig.SerializationConfig
                .PortableFactories.Add(SamplePortableFactory.FactoryId, new SamplePortableFactory());
            // Start the Hazelcast Client and connect to an already running Hazelcast Cluster on 127.0.0.1
            var hz = HazelcastClient.NewHazelcastClient(clientConfig);
            
            //Customer can be used here

            // Shutdown this Hazelcast Client
            hz.Shutdown();
        }
    }
}