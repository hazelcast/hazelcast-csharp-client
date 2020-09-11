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
using System.Collections;
using System.Collections.Generic;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Tests.Core
{
    [TestFixture]
    public class CollectionBinderTests
    {
        [Test]
        public void Test()
        {
            var l = new List<int>();
            var binder = new CollectionBinder<int>(value => l.Add(value));

            Assert.Throws<NotSupportedException>(() => _ = binder.Count);
            Assert.Throws<NotSupportedException>(() => _ = binder.IsReadOnly);
            Assert.Throws<NotSupportedException>(() => _ = binder.Contains(1));
            Assert.Throws<NotSupportedException>(() => _ = binder.Remove(1));
            Assert.Throws<NotSupportedException>(() => _ = binder.GetEnumerator());
            Assert.Throws<NotSupportedException>(() => _ = ((IEnumerable) binder).GetEnumerator());
            Assert.Throws<NotSupportedException>(() => binder.CopyTo(Array.Empty<int>(), 0));
            Assert.Throws<NotSupportedException>(() => binder.Clear());

            binder.Add(1);
            binder.Add(2);

            Assert.That(l.Count, Is.EqualTo(2));
            Assert.That(l[0], Is.EqualTo(1));
            Assert.That(l[1], Is.EqualTo(2));
        }
    }
}
