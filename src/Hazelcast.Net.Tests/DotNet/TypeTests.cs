// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
    /// <summary>
    /// Tests types and their names.
    /// </summary>
    /// <remarks>
    /// <para>Tests the various ways to obtain the name of a CLR type. Tests which names can be
    /// used to retrieve the actual CLR type and to instantiate an actual object of that type.
    /// </para>
    /// <para>We do not want to use the full name including culture and public key, as that
    /// end up creating type names that are not portable from e.g. .NET Framework to .NET Core,
    /// so we have to use an intermediate "qualified" name.</para>
    /// <para>We want to have this test to ensure that our assumptions remain verified by new
    /// versions of .NET.</para>
    /// </remarks>
    [TestFixture]
    public class TypeTests
    {
        // note for self:
        // runtime does this to avoid warning on NotImplementedException and that's nice
        // public virtual string? FullName => throw NotImplemented.ByDesign;

        private static readonly string OurVersion = typeof(TypeTests).Assembly.GetName().Version.ToString();
        private static readonly string OurCulture = "neutral";
        private static readonly byte[] OurPublicKeyBytes = typeof (TypeTests).Assembly.GetName().GetPublicKeyToken();
        private static readonly string OurPublicKeyToken = OurPublicKeyBytes.Length == 0 ? "null" : OurPublicKeyBytes.Dump(formatted: false);
        private static readonly string OurDetails = $"Version={OurVersion}, Culture={OurCulture}, PublicKeyToken={OurPublicKeyToken}";

        private static readonly string IntVersion = typeof(int).Assembly.GetName().Version.ToString();
        private static readonly string IntCulture = "neutral";
        private static readonly byte[] IntPublicKeyBytes = typeof(int).Assembly.GetName().GetPublicKeyToken();
        private static readonly string IntPublicKeyToken = IntPublicKeyBytes.Length == 0 ? "null" : IntPublicKeyBytes.Dump(formatted: false);
        private static readonly string IntDetails = $"Version={IntVersion}, Culture={IntCulture}, PublicKeyToken={IntPublicKeyToken}";

        private static readonly string IntAssembly = typeof (int).Assembly.GetName().Name;

#if NET462 || NET48
        private static readonly bool IsFramework = true;
#else
        private static readonly bool IsFramework = false;
#endif

        private static readonly (string, string)[] GetTypeNameSource =
        {

            (typeof (Thing).AssemblyQualifiedName, $"Hazelcast.Tests.DotNet.TypeTests+Thing, Hazelcast.Net.Tests, {OurDetails}"),
            (typeof (Thing<int>).AssemblyQualifiedName, $"Hazelcast.Tests.DotNet.TypeTests+Thing`1[[System.Int32, {IntAssembly}, {IntDetails}]], Hazelcast.Net.Tests, {OurDetails}"),

            (typeof (Thing).FullName, "Hazelcast.Tests.DotNet.TypeTests+Thing"),
            (typeof (Thing<int>).FullName, $"Hazelcast.Tests.DotNet.TypeTests+Thing`1[[System.Int32, {IntAssembly}, {IntDetails}]]"),

            (typeof (Thing).Name, "Thing"),
            (typeof (Thing<int>).Name, "Thing`1"),

            (typeof (Thing).ToString(), "Hazelcast.Tests.DotNet.TypeTests+Thing"),
            (typeof (Thing<int>).ToString(), "Hazelcast.Tests.DotNet.TypeTests+Thing`1[System.Int32]"),

            (typeof (Thing).GetQualifiedTypeName(), "Hazelcast.Tests.DotNet.TypeTests+Thing, Hazelcast.Net.Tests"),
            (typeof (Thing<int>).GetQualifiedTypeName(), $"Hazelcast.Tests.DotNet.TypeTests+Thing`1[[System.Int32, {IntAssembly}]], Hazelcast.Net.Tests"),

        };

        [TestCaseSource(nameof(GetTypeNameSource))]
        public void GetTypeName((string name, string expected) test)
        {
            Assert.That(test.name, Is.EqualTo(test.expected), $"Expected '{test.expected}' but got '{test.name}'.");
        }

        // type name is "The assembly-qualified name of the type to get. See AssemblyQualifiedName. If the type
        // is in the currently executing assembly or in mscorlib.dll/System.Private.CoreLib.dll, it is sufficient
        // to supply the type name qualified by its namespace."

#if NETFRAMEWORK && ASSEMBLY_SIGNING
        private static string PublicKeyToken = AssemblySigning.PublicKeyToken;
#else
        private static string PublicKeyToken = "null";
#endif

        private static readonly (string, Type, bool)[] CreateTypeSource =
        {
            // works with type.AssemblyQualifiedName
            ($"Hazelcast.Tests.DotNet.TypeTests+Thing, Hazelcast.Net.Tests, Version={OurVersion}, Culture=neutral, PublicKeyToken={PublicKeyToken}", typeof(Thing), true),
            ($"Hazelcast.Tests.DotNet.TypeTests+Thing`1[[System.Int32, {IntAssembly}, {IntDetails}]], Hazelcast.Net.Tests, Version={OurVersion}, Culture=neutral, PublicKeyToken={PublicKeyToken}", typeof(Thing<int>), true),

            // missing namespace = cannot work
            ("Thing", typeof(Thing), false),
            ("TypeTests+Thing", typeof(Thing), false),

            // namespace only = ok if same assembly else fails
            ("Hazelcast.Tests.DotNet.TypeTests+Thing", typeof(Thing), true),
            ("Hazelcast.HazelcastOptions", typeof(HazelcastOptions), false),

            // not same assembly = also requires assembly
            ("Hazelcast.HazelcastOptions, Hazelcast.Net", typeof(HazelcastOptions), true),

            // works with assembly, and more
            ("Hazelcast.Tests.DotNet.TypeTests+Thing, Hazelcast.Net.Tests", typeof(Thing), true),
            ($"Hazelcast.Tests.DotNet.TypeTests+Thing, Hazelcast.Net.Tests, Version={OurVersion}", typeof(Thing), true),
            ($"Hazelcast.Tests.DotNet.TypeTests+Thing, Hazelcast.Net.Tests, Version={OurVersion}, Culture=neutral", typeof(Thing), true),
            ($"Hazelcast.Tests.DotNet.TypeTests+Thing, Hazelcast.Net.Tests, Version={OurVersion}, Culture=neutral, PublicKeyToken=null", typeof(Thing), true),

            // different version... random public key... does not matter
            // regardless of whether the assemblies are signed or not - Type.GetType does not care
#if NETFRAMEWORK && ASSEMBLY_SIGNING
            // except when signing with net framework of course
            ($"Hazelcast.Tests.DotNet.TypeTests+Thing, Hazelcast.Net.Tests, Version=2.0.0.0", typeof(Thing), false),
            ($"Hazelcast.Tests.DotNet.TypeTests+Thing, Hazelcast.Net.Tests, Version=99.0.0.0", typeof(Thing), false),
#else
            ("Hazelcast.Tests.DotNet.TypeTests+Thing, Hazelcast.Net.Tests, Version=2.0.0.0", typeof(Thing), true),
            ("Hazelcast.Tests.DotNet.TypeTests+Thing, Hazelcast.Net.Tests, Version=99.0.0.0", typeof(Thing), true),
#endif

            // except, .NET Framework wants the public key token to make sense & match
            ($"Hazelcast.Tests.DotNet.TypeTests+Thing, Hazelcast.Net.Tests, Version={OurVersion}, Culture=neutral, PublicKeyToken=7caaaaaabea7798e", typeof(Thing), !IsFramework),

            // also works for generics
            ($"Hazelcast.Tests.DotNet.TypeTests+Thing`1[[System.Int32, {IntAssembly}]], Hazelcast.Net.Tests", typeof(Thing<int>), true),

            // qualified names is probably best for us
            ("Hazelcast.Tests.DotNet.TypeTests+Thing, Hazelcast.Net.Tests", typeof(Thing), true),
            ($"Hazelcast.Tests.DotNet.TypeTests+Thing`1[[System.Int32, {IntAssembly}]], Hazelcast.Net.Tests", typeof(Thing<int>), true),
        };

        [TestCaseSource(nameof(CreateTypeSource))]
        public void TestCreateType((string name, Type type, bool succeeds) test)
        {
            if (test.succeeds) AssertCanCreate(test.name, test.type);
            else AssertCannotCreate(test.name);
        }

        private static void AssertCannotCreate(string name)
        {
            Type type;
            try
            {
                type = Type.GetType(name);
            }
            catch
            {
                type = null;
            }
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
