// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.DotNet
{
    [TestFixture]
    public class TypeTests
    {
        // note for self:
        // runtime does this to avoid warning on NotImplementedException and that's nice
        // public virtual string? FullName => throw NotImplemented.ByDesign;

        [Test]
        public void GetTypeNames()
        {
            Console.WriteLine(typeof(Thing).AssemblyQualifiedName);
            // -> Hazelcast.Tests.DotNet.TypeTests+Thing, Hazelcast.Net.Tests, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null

            Console.WriteLine(typeof(Thing<int>).AssemblyQualifiedName);
            // -> Hazelcast.Tests.DotNet.TypeTests+Thing`1[[System.Int32, System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]], Hazelcast.Net.Tests, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null

            Console.WriteLine(typeof(Thing).FullName);
            // -> Hazelcast.Tests.DotNet.TypeTests+Thing

            Console.WriteLine(typeof(Thing<int>).FullName);
            // -> Hazelcast.Tests.DotNet.TypeTests+Thing`1[[System.Int32, System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]

            Console.WriteLine(typeof(Thing).Name);
            // -> Thing

            Console.WriteLine(typeof(Thing<int>).Name);
            // -> Thing`1

            Console.WriteLine(typeof(Thing).ToString());
            // -> Hazelcast.Tests.DotNet.TypeTests+Thing

            Console.WriteLine(typeof(Thing<int>).ToString());
            // -> Hazelcast.Tests.DotNet.TypeTests+Thing`1[System.Int32]

            Console.WriteLine(typeof(Thing).GetQualifiedTypeName());
            // -> Hazelcast.Tests.DotNet.TypeTests+Thing, Hazelcast.Net.Tests

            Console.WriteLine(typeof(Thing<int>).GetQualifiedTypeName());
            // -> Hazelcast.Tests.DotNet.TypeTests+Thing`1[[System.Int32, System.Private.CoreLib]], Hazelcast.Net.Tests
        }

        // type name is "The assembly-qualified name of the type to get. See AssemblyQualifiedName. If the type
        // is in the currently executing assembly or in mscorlib.dll/System.Private.CoreLib.dll, it is sufficient
        // to supply the type name qualified by its namespace."

        // works with type.AssemblyQualifiedName
        [TestCase("Hazelcast.Tests.DotNet.TypeTests+Thing, Hazelcast.Net.Tests, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null", typeof(Thing), true)]
        [TestCase("Hazelcast.Tests.DotNet.TypeTests+Thing`1[[System.Int32, System.Private.CoreLib, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]], Hazelcast.Net.Tests, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null", typeof(Thing<int>), true)]

        // missing namespace = cannot work
        [TestCase("Thing", typeof(Thing), false)]
        [TestCase("TypeTests+Thing", typeof(Thing), false)]

        // namespace only = ok if same assembly else fails
        [TestCase("Hazelcast.Tests.DotNet.TypeTests+Thing", typeof(Thing), true)]
        [TestCase("Hazelcast.HazelcastOptions", typeof(HazelcastOptions), false)]

        // not same assembly = also requires assembly
        [TestCase("Hazelcast.HazelcastOptions, Hazelcast.Net", typeof(HazelcastOptions), true)]

        // works with assembly, and more
        [TestCase("Hazelcast.Tests.DotNet.TypeTests+Thing, Hazelcast.Net.Tests", typeof(Thing), true)]
        [TestCase("Hazelcast.Tests.DotNet.TypeTests+Thing, Hazelcast.Net.Tests, Version=5.0.0.0", typeof(Thing), true)]
        [TestCase("Hazelcast.Tests.DotNet.TypeTests+Thing, Hazelcast.Net.Tests, Version=5.0.0.0, Culture=neutral", typeof(Thing), true)]
        [TestCase("Hazelcast.Tests.DotNet.TypeTests+Thing, Hazelcast.Net.Tests, Version=5.0.0.0, Culture=neutral, PublicKeyToken=null", typeof(Thing), true)]

        // different version... random public key... does not matter
        // regardless of whether the assemblies are signed or not - Type.GetType does not care
        [TestCase("Hazelcast.Tests.DotNet.TypeTests+Thing, Hazelcast.Net.Tests, Version=2.0.0.0", typeof(Thing), true)]
        [TestCase("Hazelcast.Tests.DotNet.TypeTests+Thing, Hazelcast.Net.Tests, Version=99.0.0.0", typeof(Thing), true)]
        [TestCase("Hazelcast.Tests.DotNet.TypeTests+Thing, Hazelcast.Net.Tests, Version=5.0.0.0, Culture=neutral, PublicKeyToken=7caaaaaabea7798e", typeof(Thing), true)]

        // also works for generics
        [TestCase("Hazelcast.Tests.DotNet.TypeTests+Thing`1[[System.Int32, System.Private.CoreLib]], Hazelcast.Net.Tests", typeof(Thing<int>), true)]

        // qualified names is probably best for us
        [TestCase("Hazelcast.Tests.DotNet.TypeTests+Thing, Hazelcast.Net.Tests", typeof(Thing), true)]
        [TestCase("Hazelcast.Tests.DotNet.TypeTests+Thing`1[[System.Int32, System.Private.CoreLib]], Hazelcast.Net.Tests", typeof(Thing<int>), true)]

        public void TestCreateType(string name, Type type, bool succeeds)
        {
            if (succeeds) AssertCanCreate(name, type);
            else AssertCannotCreate(name);
        }

        private static void AssertCannotCreate(string name)
        {
            var type = Type.GetType(name);
            Assert.That(type, Is.Null);
        }

        private static void AssertCanCreate(string name, Type expectedType)
        {
            var type = Type.GetType(name);
            Assert.That(type, Is.Not.Null);
            var instance = Activator.CreateInstance(type);
            Assert.That(instance, Is.Not.Null);
            Assert.That(instance, Is.InstanceOf(expectedType));
        }

        public class Thing
        { }

        public class Thing<T>
        { }
    }
}
