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
    public class EndiannessExtensionsTests
    {
        [Test]
        public void IsBigOrLittleEndian()
        {
            Assert.That(Endianness.BigEndian.IsBigEndian(), Is.True);
            Assert.That(Endianness.BigEndian.IsLittleEndian(), Is.False);

            Assert.That(Endianness.LittleEndian.IsBigEndian(), Is.False);
            Assert.That(Endianness.LittleEndian.IsLittleEndian(), Is.True);
        }

        [Test]
        public void Resolve()
        {
            Assert.That(Endianness.Unspecified.Resolve(), Is.EqualTo(Endianness.BigEndian));
            Assert.That(Endianness.Unspecified.Resolve(Endianness.LittleEndian), Is.EqualTo(Endianness.LittleEndian));
            Assert.That(Endianness.Unspecified.Resolve(Endianness.Unspecified), Is.EqualTo(Endianness.Unspecified));

            Assert.That(Endianness.LittleEndian.Resolve(), Is.EqualTo(Endianness.LittleEndian));
            Assert.That(Endianness.BigEndian.Resolve(Endianness.LittleEndian), Is.EqualTo(Endianness.BigEndian));

            Assert.Throws<NotSupportedException>(() => ((Endianness) (-1)).Resolve());

            Assert.That(Endianness.Native.Resolve(), Is.EqualTo(EndiannessExtensions.NativeEndianness));
        }
    }
}
