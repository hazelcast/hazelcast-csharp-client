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
using System.Threading.Tasks;
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

        [Test]
        public void MulticastFunctionTest()
        {
            Func<string, string> f;

            f = s => s + "-world";
            f += s => s + "-again";

            Assert.That(f("hello"), Is.EqualTo("hello-again"));

            var x = "hello";
            foreach (var d in f.GetInvocationList())
            {
                x = ((Func<string, string>) d)(x);
            }

            Assert.That(x, Is.EqualTo("hello-world-again"));
        }

        [Test]
        public void NullTest()
        {
            Func<string, string> f = null;

            Assert.Throws<NullReferenceException>(() =>
            {
                var x = f("hello");
            });

            Assert.Throws<NullReferenceException>(() =>
            {
                var x = f.GetInvocationList();
            });

            var y = f?.Invoke("hello");
            Assert.That(y, Is.Null);
        }

       
    }
}
