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
using Hazelcast.Serialization;

namespace Hazelcast.Examples.Models
{
    public class Employee : IIdentifiedDataSerializable
    {
        public const int TypeId = 100;

        public int Id { get; set; }
        public string Name { get; set; }

        public void ReadData(IObjectDataInput input)
        {
            Id = input.ReadInt();
            Name = input.ReadString();
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteInt(Id);
            output.WriteString(Name);
        }

        public int FactoryId => ExampleDataSerializableFactory.FactoryId;

        public int ClassId => TypeId;

        public override string ToString()
        {
            return string.Format("Id: {0}, Name: {1}", Id, Name);
        }
    }

    public class ExampleDataSerializableFactory : IDataSerializableFactory
    {
        public const int FactoryId = 1000;
        public IIdentifiedDataSerializable Create(int typeId)
        {
            if (typeId == 100) return new Employee();
            throw new InvalidOperationException("Unknown type id");
        }
    }
}
