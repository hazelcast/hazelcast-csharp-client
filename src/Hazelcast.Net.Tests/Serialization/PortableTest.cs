// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Serialization
{
    internal class PortableTest
    {

        [Test]
        public virtual void TestNestedPortableVersionedSerializer()
        {
            SerializationServiceBuilder builder1 = new SerializationServiceBuilder(new NullLoggerFactory());
            builder1.SetPortableVersion(6);
            builder1.AddPortableFactory(1, new MyPortableFactory());
            SerializationService ss1 = builder1.Build();

            SerializationServiceBuilder builder2 = new SerializationServiceBuilder(new NullLoggerFactory());
            builder2.SetPortableVersion(6);
            builder2.AddPortableFactory(1, new MyPortableFactory());
            SerializationService ss2 = builder2.Build();

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
            private string _name;

            public Child()
            { }

            public Child(string name)
            {
                _name = name;
            }

            public int ClassId => 2;

            public int FactoryId => 1;

            public void ReadPortable(IPortableReader reader)
            {
                _name = reader.ReadString("name");
            }

            public void WritePortable(IPortableWriter writer)
            {
                writer.WriteString("name", _name);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(this, obj))
                    return true;

                if (!(obj is Child child))
                    return false;

                return _name == child._name;
            }

            public override int GetHashCode()
            {
                // ReSharper disable once NonReadonlyMemberInGetHashCode
                return _name.GetHashCode();
            }
        }

        private class Parent : IPortable
        {
            private Child _child;

            public Parent()
            { }

            public Parent(Child child)
            {
                _child = child;
            }

            public int ClassId => 1;

            public int FactoryId => 1;

            public void ReadPortable(IPortableReader reader)
            {
                _child = reader.ReadPortable<Child>("child");
            }

            public void WritePortable(IPortableWriter writer)
            {
                writer.WritePortable("child", _child);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(this, obj))
                    return true;

                if (!(obj is Parent parent))
                    return false;

                return _child.Equals(parent._child);
            }

            public override int GetHashCode()
            {
                // ReSharper disable once NonReadonlyMemberInGetHashCode
                return _child.GetHashCode();
            }
        }
    }
}
