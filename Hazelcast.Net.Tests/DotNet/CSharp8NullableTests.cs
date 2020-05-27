using System.Diagnostics.CodeAnalysis;
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
            // exists with ns 2.1+ 
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
