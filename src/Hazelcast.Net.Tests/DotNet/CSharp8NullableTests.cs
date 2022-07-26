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

using NUnit.Framework;

namespace Hazelcast.Tests.DotNet
{
    // https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references
    // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/attributes/nullable-analysis
    // https://docs.microsoft.com/en-us/dotnet/csharp/nullable-migration-strategies
    // https://www.meziantou.net/csharp-8-nullable-reference-types.htm
    // https://www.meziantou.net/how-to-use-nullable-reference-types-in-dotnet-standard-2-0-and-dotnet-.htm

    [TestFixture]
    public class CSharp8NullableTests
    {
        #nullable enable

        private string _nonNullString = "a";
        private string? _nullString;

        private class Thing<T>
        {
            private readonly T _t;

            public Thing()
            {
                // warning: possible null reference assignment
                // BUT! _t cannot be T? as that requires T to be a reference type (class)
                // https://github.com/dotnet/roslyn/issues/30953
                // https://github.com/dotnet/csharplang/issues/2194
                //
                // so _t remains T and we have to use the ! here
                //
                _t = default!;
            }

            public Thing(T t)
            {
                _t = t;
            }

            // this cannot return T? as it would return null only for reference types (classes)
            // but would return default(T) for structs - hence the MaybeNull attribute that only
            // exists with netstandard 2.1+ or can be redefined for netstandard 2.0 in our own code
            [return: MaybeNull]
            public T GetT() => _t;
        }

        [Test]
        public void Test()
        {
            _nullString = GetString();
            _nonNullString = GetString();

            Assert.IsNull(_nullString);
            Assert.IsNull(_nonNullString);

            var thing1 = new Thing<object>(new object());
            // would cause a warning because GetT() can be null
            //object o = thing1.GetT();
            var o = thing1.GetT();
            if (o == null)
            { }

            var thing2 = new Thing<int>(2);
            _ = thing2.GetT();
        }

        private static string GetString()
        {
            // this generates a warning *but* does not prevent the code from compiling
            //return null;
            return null!; // no more warning!
        }
    }
}
