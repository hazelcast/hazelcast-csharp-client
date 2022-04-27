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
using NUnit.Framework;

namespace Hazelcast.Tests.DotNet
{
    [TestFixture]
    public class DotNetTests
    {
        [Test]
        public void DefaultTimeSpanIsZero()
        {
            Assert.AreEqual(TimeSpan.Zero, default(TimeSpan));
        }

        [Test]
        public void ParamsArgs()
        {
            ParamsArgsMethod(false);
            ParamsArgsMethod(false, "a", "b");
            ParamsArgsMethod(false, (string)null);
            ParamsArgsMethod(true, null);
        }

        private static void ParamsArgsMethod(bool isnull, params object[] objects)
        {
            Assert.That(objects, isnull ? Is.Null : Is.Not.Null);
        }

        [Test]
        public void GetInterfacesIsRecursive()
        {
            var interfaces = typeof (C2).GetInterfaces();
            Assert.That(interfaces.Length, Is.EqualTo(3));
            Assert.That(interfaces, Does.Contain(typeof(I1))); // through I1
            Assert.That(interfaces, Does.Contain(typeof(I2))); // through base class
            Assert.That(interfaces, Does.Contain(typeof(I3))); // direct
        }

        private interface I1
        { }

        private interface I2
        { }

        private interface I3 : I1
        { }

        private class C1 : I2
        { }

        private class C2 : C1, I3
        { }
    }
}
