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
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class EnumExtensionsTests
    {
        [Test]
        public void HasAll()
        {
            var v = SomeEnum.A;

            Assert.That(v.HasAll(SomeEnum.A), Is.True);
            Assert.That(v.HasAll(SomeEnum.A | SomeEnum.B), Is.False);
            Assert.That(v.HasAll(SomeEnum.B), Is.False);

            v = SomeEnum.A | SomeEnum.B;

            Assert.That(v.HasAll(SomeEnum.A), Is.True);
            Assert.That(v.HasAll(SomeEnum.A | SomeEnum.B), Is.True);
            Assert.That(v.HasAll(SomeEnum.B), Is.True);
        }

        [Test]
        public void HasAny()
        {
            var v = SomeEnum.A;

            Assert.That(v.HasAny(SomeEnum.A), Is.True);
            Assert.That(v.HasAny(SomeEnum.A | SomeEnum.B), Is.True);
            Assert.That(v.HasAny(SomeEnum.B), Is.False);

            v = SomeEnum.A | SomeEnum.B;

            Assert.That(v.HasAny(SomeEnum.A), Is.True);
            Assert.That(v.HasAny(SomeEnum.A | SomeEnum.B), Is.True);
            Assert.That(v.HasAny(SomeEnum.B), Is.True);
        }

        [Flags]
        private enum SomeEnum
        {
            None = 0, // (default)
            A = 1,
            B = 2,
            C = 4,
            D = 8,
            E = 16
        }
    }
}
