// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Examples.Org.Website.Samples
{
    public class Employee : IIdentifiedDataSerializable
    {
        private const int ClassId = 100;

        public int Id { get; set; }
        public string Name { get; set; }

        public void ReadData(IObjectDataInput input)
        {
            Id = input.ReadInt();
            Name = input.ReadUTF();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteInt(Id);
            output.WriteUTF(Name);
        }

        public int GetFactoryId()
        {
            return SampleDataSerializableFactory.FactoryId;
        }

        public int GetId()
        {
            return ClassId;
        }
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

    public class IdentifiedDataSerializableSample
    {
        public static void Run(string[] args)
        {
            var clientConfig = new ClientConfig();
            clientConfig.GetSerializationConfig()
                .AddDataSerializableFactory(SampleDataSerializableFactory.FactoryId,
                    new SampleDataSerializableFactory());
            // Start the Hazelcast Client and connect to an already running Hazelcast Cluster on 127.0.0.1
            var hz = HazelcastClient.NewHazelcastClient(clientConfig);

            //Employee can be used here
            
            // Shutdown this Hazelcast Client
            hz.Shutdown();
        }
    }
}