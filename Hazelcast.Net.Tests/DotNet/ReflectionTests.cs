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
