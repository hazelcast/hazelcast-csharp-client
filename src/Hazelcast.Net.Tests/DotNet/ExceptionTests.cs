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
using System.Runtime.ExceptionServices;
using NUnit.Framework;
using Hazelcast.Core;

namespace Hazelcast.Tests.DotNet
{
    [TestFixture]
    public class ExceptionTests
    {
        [Test]
        public void MustThrowToGetStackTrace()
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

        [Test]
        public void NewExceptionStackTrace()
        {
            var e = new Exception("bang!");
            Assert.That(e.Message, Is.EqualTo("bang!"));
            Assert.That(e.InnerException, Is.Null);
            Assert.That(e.StackTrace, Is.Null); // that is annoying

            // and this is not going to help
            var i = ExceptionDispatchInfo.Capture(e);
            Assert.That(e.StackTrace, Is.Null);
            Assert.That(i.SourceException.StackTrace, Is.Null);

            e = e.Thrown();
            Assert.That(e.StackTrace, Is.Not.Null); // that is better
        }
    }
}
