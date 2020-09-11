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
    public class DictionaryBinderTests
    {
        [Test]
        public void Test()
        {
            var d = new Dictionary<string, string>();
            var binder = new DictionaryBinder((key, value) => d[key] = value);

            Assert.Throws<NotSupportedException>(() => _ = binder.Count);
            Assert.Throws<NotSupportedException>(() => _ = binder.IsReadOnly);
            Assert.Throws<NotSupportedException>(() => _ = binder["x"]);
            Assert.Throws<NotSupportedException>(() => _ = binder.ContainsKey("x"));
            Assert.Throws<NotSupportedException>(() => _ = binder.Contains(new KeyValuePair<string, string>("x", "y")));
            Assert.Throws<NotSupportedException>(() => _ = binder.Remove("x"));
#if !NETFRAMEWORK
            Assert.Throws<NotSupportedException>(() => _ = binder.Remove("x", out _));
#endif
            Assert.Throws<NotSupportedException>(() => _ = binder.Remove(new KeyValuePair<string, string>("x", "y")));
            Assert.Throws<NotSupportedException>(() => _ = binder.TryGetValue("x", out _));
            Assert.Throws<NotSupportedException>(() => _ = binder.GetEnumerator());
            Assert.Throws<NotSupportedException>(() => _ = ((IEnumerable) binder).GetEnumerator());
            Assert.Throws<NotSupportedException>(() => _ = binder.Values);
            Assert.Throws<NotSupportedException>(() => _ = binder.Keys);
            Assert.Throws<NotSupportedException>(() => binder.CopyTo(new KeyValuePair<string, string>[0], 0));
            Assert.Throws<NotSupportedException>(() => binder.Clear());

            binder["key_1"] = "value_1";
            binder.Add("key_2", "value_2");
            binder.Add(new KeyValuePair<string, string>("key_3", "value_3"));

            Assert.That(d.Count, Is.EqualTo(3));
            Assert.That(d["key_1"], Is.EqualTo("value_1"));
            Assert.That(d["key_2"], Is.EqualTo("value_2"));
            Assert.That(d["key_3"], Is.EqualTo("value_3"));
        }
    }
}
