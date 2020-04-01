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
using Hazelcast.IO.Serialization;
using NUnit.Framework;

namespace Hazelcast.Client.Test.Serialization
{
    internal class PortableTest
    {

        [Test]
        public virtual void TestNestedPortableVersionedSerializer()
        {
            SerializationServiceBuilder builder1 = new SerializationServiceBuilder();
            builder1.SetPortableVersion(6);
            builder1.AddPortableFactory(1, new MyPortableFactory());
            ISerializationService ss1 = builder1.Build();

            SerializationServiceBuilder builder2 = new SerializationServiceBuilder();
            builder2.SetPortableVersion(6);
            builder2.AddPortableFactory(1, new MyPortableFactory());
            ISerializationService ss2 = builder2.Build();

            //make sure ss2 cached class definition of Child
            ss2.ToData(new Child("ubeyd"));

            //serialized parent from ss1
            Parent parent = new Parent(new Child("ubeyd"));
            IData data = ss1.ToData(parent);

            // cached class definition of Child and the class definition from data coming from ss1 should be compatible
            Assert.AreEqual(parent, ss2.ToObject<Parent>(data));
        }

        private class MyPortableFactory : IPortableFactory
        {
            public IPortable Create(int classId)
            {
                if (classId == 1)
                {
                    return new Parent();
                }
                else if (classId == 2)
                {
                    return new Child();
                }
                return null;
            }
        }


        private class Child : IPortable
        {
            private string name;

            public Child()
            {

            }

            public Child(string name)
            {
                this.name = name;
            }

            public int GetClassId()
            {
                return 2;
            }

            public int GetFactoryId()
            {
                return 1;
            }

            public void ReadPortable(IPortableReader reader)
            {
                name = reader.ReadUTF("name");
            }

            public void WritePortable(IPortableWriter writer)
            {
                writer.WriteUTF("name", name);
            }

            public override bool Equals(object obj)
            {
                if(ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj == null || !GetType().Equals(obj.GetType()))
                {
                    return false;
                }

                Child child = (Child)obj;
                return name != null ? name.Equals(child.name) : child.name == null;
            }
        }

        private class Parent : IPortable
        {
            private Child child;

            public Parent()
            {

            }

            public Parent(Child child)
            {
                this.child = child;
            }

            public int GetClassId()
            {
                return 1;
            }

            public int GetFactoryId()
            {
                return 1;
            }

            public void ReadPortable(IPortableReader reader)
            {
                child = reader.ReadPortable<Child>("child");
            }

            public void WritePortable(IPortableWriter writer)
            {
                writer.WritePortable("child", child);
            }

            public override bool Equals(object obj)
            {
                if(ReferenceEquals(this, obj))
                {
                    return true;
                }
                if(obj == null || !GetType().Equals(obj.GetType())){
                    return false;
                }
                Parent parent = (Parent)obj;
                return child != null ? child.Equals(parent.child) : parent.child == null;
            }
        }
    }
}