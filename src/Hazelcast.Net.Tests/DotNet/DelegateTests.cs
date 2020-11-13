using System;
using NUnit.Framework;

namespace Hazelcast.Tests.DotNet
{
    [TestFixture]
    public class DelegateTests
    {
        // Part 15.4 of the C# 4.0 language specification
        //
        // Invocation of a delegate instance whose invocation list contains multiple
        // entries proceeds by invoking each of the methods in the invocation list,
        // synchronously, in order. ... If the delegate invocation includes output
        // parameters or a return value, their final value will come from the invocation
        // of the last delegate in the list.
        //
        // ie only the last result of functions is returned
        // but each function or action runs

        [Test]
        public void FunctionTest()
        {
            Func<string, string> f;

            f = s => s + "-world";

            Assert.That(f("hello"), Is.EqualTo("hello-world"));

            f += s => s + "-again";

            Assert.That(f("hello"), Is.EqualTo("hello-again"));
        }

        [Test]
        public void ActionTest()
        {
            Action<string> a;

            var x = "hello";

            a = s => x += "-world";
            a += s => x += "-again";

            a("");

            Assert.That(x, Is.EqualTo("hello-world-again"));
        }

        [Test]
        public void InitialNullTest()
        {
            Action<string> a = null; // eg a property

            var x = "hello";

            // it is ok to += a null action
            a += s => x += "-world";
            a += s => x += "-again";

            a("");

            Assert.That(x, Is.EqualTo("hello-world-again"));
        }
    }
}
