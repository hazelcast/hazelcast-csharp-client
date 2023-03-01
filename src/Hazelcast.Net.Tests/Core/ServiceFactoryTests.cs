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

using System;
using System.Collections.Generic;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class ServiceFactoryTests
    {
        [Test]
        public void CreateInstanceByType()
        {
            var thing1 = ServiceFactory.CreateInstance(typeof (Thing1), null);
            Assert.That(thing1, Is.Not.Null);
            Assert.That(thing1, Is.InstanceOf<Thing1>());

            thing1 = ServiceFactory.CreateInstance(typeof(Thing1), new Dictionary<string, string>{ { "a", "b" } }, 1, "foo");
            Assert.That(thing1, Is.Not.Null);
            Assert.That(thing1, Is.InstanceOf<Thing1>());

            Assert.Throws<ServiceFactoryException>(() => ServiceFactory.CreateInstance(typeof(Thing2), null));

            var thing2 = ServiceFactory.CreateInstance(typeof (Thing2), new Dictionary<string, string> { { "a", "b" } }, "foo", 3);
            Assert.That(thing2, Is.Not.Null);
            Assert.That(thing2, Is.InstanceOf<Thing2>());
            Assert.That(((Thing2) thing2).Value, Is.EqualTo(3));

            thing2 = ServiceFactory.CreateInstance(typeof(Thing2), new Dictionary<string, string> { { "value", "3" } }, "foo");
            Assert.That(thing2, Is.Not.Null);
            Assert.That(thing2, Is.InstanceOf<Thing2>());
            Assert.That(((Thing2)thing2).Value, Is.EqualTo(3));

            Assert.Throws<ServiceFactoryException>(() =>
                ServiceFactory.CreateInstance(typeof(Thing2), new Dictionary<string, string> { { "_value", "3" } }, "foo"));

            thing2 = ServiceFactory.CreateInstance(typeof(Thing2), new Dictionary<string, string> { { "value", "4" } }, 4);
            Assert.That(thing2, Is.Not.Null);
            Assert.That(thing2, Is.InstanceOf<Thing2>());
            Assert.That(((Thing2)thing2).Value, Is.EqualTo(4));
        }

        [Test]
        public void CreateInstanceByName()
        {
            var nameOfThing1 = typeof (Thing1).AssemblyQualifiedName;
            Console.WriteLine(nameOfThing1);

            var thing1 = ServiceFactory.CreateInstance(nameOfThing1, null);
            Assert.That(thing1, Is.Not.Null);
            Assert.That(thing1, Is.InstanceOf<Thing1>());

            var iThing1 = ServiceFactory.CreateInstance<IThing1>(nameOfThing1, null);
            Assert.That(iThing1, Is.Not.Null);
            Assert.That(iThing1, Is.InstanceOf<Thing1>());

            Assert.Throws<ServiceFactoryException>(() => ServiceFactory.CreateInstance("doh", null));
        }

        [Test]
        public void CreateInstanceGeneric()
        {
            var thing1 = ServiceFactory.CreateInstance<Thing1>(null);
            Assert.That(thing1, Is.Not.Null);
            Assert.That(thing1, Is.InstanceOf<Thing1>());

            var iThing1 = ServiceFactory.CreateInstance<IThing1>(typeof(Thing1), null);
            Assert.That(iThing1, Is.Not.Null);
            Assert.That(iThing1, Is.InstanceOf<Thing1>());

            var nameOfThing2 = typeof(Thing2).AssemblyQualifiedName;

            Assert.Throws<ServiceFactoryException>(() => ServiceFactory.CreateInstance<Thing1>(nameOfThing2, null));
            Assert.Throws<ServiceFactoryException>(() => ServiceFactory.CreateInstance<Thing1>(typeof(Thing2), null));
            Assert.Throws<ServiceFactoryException>(() => ServiceFactory.CreateInstance<Thing2>(null));
            Assert.Throws<ServiceFactoryException>(() => ServiceFactory.CreateInstance<Thing2>(new Dictionary<string, string>()));
        }

        [Test]
        public void Arguments()
        {
            Assert.Throws<ArgumentNullException>(() => ServiceFactory.CreateInstance((Type) null, null));
            Assert.Throws<ArgumentNullException>(() => ServiceFactory.CreateInstance(typeof(Thing1), null, null));

            Assert.Throws<ArgumentException>(() => ServiceFactory.CreateInstance((string) null, null));
            Assert.Throws<ArgumentException>(() => ServiceFactory.CreateInstance("  ", null));
            Assert.Throws<ArgumentNullException>(() => ServiceFactory.CreateInstance("xxx", null, null));

            Assert.Throws<ArgumentNullException>(() => ServiceFactory.CreateInstance<Thing1>((Type) null, null));
            Assert.Throws<ArgumentNullException>(() => ServiceFactory.CreateInstance<Thing1>(typeof(Thing1), null, null));

            Assert.Throws<ArgumentException>(() => ServiceFactory.CreateInstance<Thing1>((string) null, null));
            Assert.Throws<ArgumentException>(() => ServiceFactory.CreateInstance<Thing1>("  ", null));
            Assert.Throws<ArgumentNullException>(() => ServiceFactory.CreateInstance<Thing1>("xxx", null, null));

            //Assert.Throws<ArgumentNullException>(() => ServiceFactory.CreateInstance<Thing1>((IDictionary<string, string>) null));
            Assert.Throws<ArgumentNullException>(() => ServiceFactory.CreateInstance<Thing1>(new Dictionary<string, string>(), null));

            Assert.Throws<ArgumentNullException>(() => ServiceFactory.CreateInstanceInternal((Type) null, null));
            Assert.Throws<ArgumentNullException>(() => ServiceFactory.CreateInstanceInternal(typeof (Thing1), null, null));
            Assert.Throws<ArgumentException>(() => ServiceFactory.CreateInstanceInternal((string) null, null));
            Assert.Throws<ArgumentException>(() => ServiceFactory.CreateInstanceInternal("   ", null));
        }

        [Test]
        public void AsT()
        {
            Assert.That(ServiceFactory.As<int>(33), Is.EqualTo(33));
            Assert.Throws<ArgumentNullException>(() => ServiceFactory.As<object>(null));
            Assert.Throws<InvalidCastException>(() => ServiceFactory.As<int>("string"));
        }

        private interface IThing1
        { }

        private class Thing1 : IThing1
        { }

        private class Thing2
        {
            // private = cannot be used, only public constructors are used
            private Thing2()
            { }

            public Thing2(int value)
            {
                Value = value;
            }
            public int Value { get; }
        }
    }
}
