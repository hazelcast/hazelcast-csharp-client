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
using System.Runtime.ExceptionServices;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.DotNet
{
    [TestFixture]
    public class ExceptionTests
    {
        // this test shows that a newly instantiated exception does not have a
        // stacktrace until it has actually been thrown, meaning that it is not
        // a good idea to simply add new exceptions to an aggregate and then
        // throw the aggregate.

        [Test]
        public void NewExceptionMissingStackTrace()
        {
            var e = new Exception("bang!");
            Assert.That(e.Message, Is.EqualTo("bang!"));
            Assert.That(e.InnerException, Is.Null);
            Assert.That(e.StackTrace, Is.Null); // that is annoying

            try
            {
                throw e;
            }
            catch { }

            Assert.That(e.StackTrace, Is.Not.Null); // that is better
        }

        // this test shows that capturing the exception via ExceptionDispatchInfo
        // is not going to add a stacktrace to it, so it is not a solution to the
        // missing stacktrace problem

        [Test]
        public void ExceptionDispatchCaptureDoesNotSetStackTrace()
        {
            var e = new Exception("bang!");
            Assert.That(e.Message, Is.EqualTo("bang!"));
            Assert.That(e.InnerException, Is.Null);
            Assert.That(e.StackTrace, Is.Null); // that is annoying

            // and this is not going to help
            var i = ExceptionDispatchInfo.Capture(e);
            Assert.That(e.StackTrace, Is.Null);
            Assert.That(i.SourceException.StackTrace, Is.Null);
        }

        // this test shows that starting with .NET 5, ExceptionDispatchInfo
        // has a way to set the stacktrace of a new exception - alas, it is
        // not available with .NET Framework, or .NET Core pre-5.

        [Test]
        public void ExceptionDispatchCanSetStackTrace()
        {
            var e = new Exception("bang!");
            Assert.That(e.Message, Is.EqualTo("bang!"));
            Assert.That(e.InnerException, Is.Null);
            Assert.That(e.StackTrace, Is.Null); // that is annoying

#if NET5_0_OR_GREATER
            e = ExceptionDispatchInfo.SetCurrentStackTrace(e);
            Assert.That(e.StackTrace, Is.Not.Null); // that is better
#endif
        }

        // this test shows that our exception method, which uses a combination
        // of different means, can set the stacktrace for all .NET versions

        [Test]
        public void ExtensionMethodCanSetStackTrace()
        {
            var e = new Exception("bang!");
            Assert.That(e.Message, Is.EqualTo("bang!"));
            Assert.That(e.InnerException, Is.Null);
            Assert.That(e.StackTrace, Is.Null); // that is annoying

            e = e.SetCurrentStackTrace();
            Assert.That(e.StackTrace, Is.Not.Null); // that is better

            // and, the very first line is this current method
            var lines = e.StackTrace.Split('\n');
            Assert.That(lines[0], Does.Contain(nameof(ExtensionMethodCanSetStackTrace)));
        }
    }
}
