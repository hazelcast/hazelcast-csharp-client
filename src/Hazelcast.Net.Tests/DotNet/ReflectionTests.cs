// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using System.Linq;
using System.Reflection;
using Hazelcast.Configuration.Binding;
using NUnit.Framework;

namespace Hazelcast.Tests.DotNet
{
    [TestFixture]
    public class ReflectionTests
    {
        [Test]
        public void Test()
        {
            var instance = (TestClass) CreateInstance(typeof(TestClass), null);
            Assert.AreEqual(0, instance.Ctor);
            instance = (TestClass) CreateInstance(typeof(TestClass), null, "foo");
            Assert.AreEqual(1, instance.Ctor);
            instance = (TestClass) CreateInstance(typeof(TestClass), null, "foo", 33);
            Assert.AreEqual(2, instance.Ctor);
            instance = (TestClass) CreateInstance(typeof(TestClass), null, 33, "foo");
            Assert.AreEqual(2, instance.Ctor);
            instance = (TestClass) CreateInstance(typeof(TestClass), new Dictionary<string, string>
            {
                { "s", "foo" }
            });
            Assert.AreEqual(1, instance.Ctor);
            instance = (TestClass) CreateInstance(typeof(TestClass), new Dictionary<string, string>
            {
                { "s", "foo" },
                { "x", "bar" }
            });
            Assert.AreEqual(1, instance.Ctor);
            instance = (TestClass) CreateInstance(typeof(TestClass), new Dictionary<string, string>
            {
                { "s", "foo" },
                { "i", "33" }
            });
            Assert.AreEqual(2, instance.Ctor);
            instance = (TestClass) CreateInstance(typeof(TestClass), new Dictionary<string, string>
            {
                { "s", "foo" }
            }, 33);
            Assert.AreEqual(2, instance.Ctor);
        }

        public object CreateInstance(Type type, IDictionary<string, string> args1, params object[] args2)
        {
            var ctors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                .OrderByDescending(x => x.GetParameters().Length);

            var args = new List<object>();
            foreach (var ctor in ctors)
            {
                args.Clear();

                var good = true;
                foreach (var param in ctor.GetParameters())
                {
                    var arg2 = args2.FirstOrDefault(x => param.ParameterType.IsAssignableFrom(x.GetType()));
                    if (arg2 != null)
                    {
                        args.Add(arg2);
                        continue;
                    }

                    if (args1 != null && args1.TryGetValue(param.Name, out var arg1))
                    {
                        if (ConfigurationBinder.TryConvertValue(param.ParameterType, arg1, "", out var v, out _))
                        {
                            args.Add(v);
                            continue;
                        }
                    }

                    good = false;
                    break;
                }

                if (good)
                    return ctor.Invoke(args.ToArray());
            }

            throw new Exception("failed");
        }

        [Test]
        public void Constructors()
        {
            // a class has, by default, an empty constructor
            Assert.That(typeof (SimpleClass).GetConstructors().Length, Is.EqualTo(1));
            Activator.CreateInstance(typeof (SimpleClass)); // and can be created

            // a class cannot be activated without an (explicit or implicit) empty constructor
            Assert.Throws<MissingMethodException>(() => Activator.CreateInstance(typeof (SimpleClass2)));

            // a struct does not, by default, have an empty constructor
            Assert.That(typeof (SimpleStruct).GetConstructors().Length, Is.EqualTo(0));
            Activator.CreateInstance(typeof (SimpleStruct)); // still, it *can* be created

            // unless explicitly defined
            Assert.That(typeof (SimpleStruct2).GetConstructors().Length, Is.EqualTo(1));
            Activator.CreateInstance(typeof (SimpleStruct2)); // then of course it can be created

            // and not if another ctor exists
            Assert.That(typeof (SimpleStruct3).GetConstructors().Length, Is.EqualTo(1));
            Activator.CreateInstance(typeof (SimpleStruct3)); // and it *still* can be created
        }

        public class SimpleClass { }

        public class SimpleClass2
        {
            public SimpleClass2(int i) {}
        }

        public struct SimpleStruct { }

        public struct SimpleStruct2
        {
            public SimpleStruct2 () { }
        }

        public struct SimpleStruct3
        {
            public SimpleStruct3(int i) { }
        }

        public class TestClass
        {
            public TestClass()
            {
                Ctor = 0;
            }

            public TestClass(string s)
            {
                Ctor = 1;
            }

            public TestClass(string s, int i)
            {
                Ctor = 2;
            }

            private TestClass(Guid guid)
            {
                Ctor = 3;
            }

            public int Ctor { get; private set; }
        }
    }
}
