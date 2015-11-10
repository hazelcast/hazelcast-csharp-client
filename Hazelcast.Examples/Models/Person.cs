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

using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Examples.Models
{
    internal class Person : IDataSerializable
    {
        public Person()
        {
        }

        public Person(int id, string name, int age)
        {
            Id = id;
            Name = name;
            Age = age;
        }

        public string Name { get; private set; }
        public int Age { get; private set; }
        public int Id { get; private set; }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteInt(Id);
            output.WriteUTF(Name);
            output.WriteInt(Age);
        }

        public void ReadData(IObjectDataInput input)
        {
            Id = input.ReadInt();
            Name = input.ReadUTF();
            Age = input.ReadInt();
        }

        public string GetJavaClassName()
        {
            return "com.hazelcast.examples.Person";
        }

        public override string ToString()
        {
            return string.Format("Name: {0}, Age: {1}, Id: {2}", Name, Age, Id);
        }
    }
}