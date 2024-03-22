// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
    public class MaybeTests
    {
        [Test]
        public void MaybeIsIEquatable()
        {
            Assert.That(Maybe.Some(12), Is.Not.SameAs(Maybe.Some(12)));
            Assert.That(Maybe.Some(12), Is.EqualTo(Maybe.Some(12)));
        }

        [Test]
        public void MaybeSomeNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Maybe.Some<string>(null));
        }

        [Test]
        public void ImplicitCastOfT()
        {
            // implicit cast of T to Maybe<T>
            static Maybe<int> GetMaybe() => 3;

            Assert.That(GetMaybe().ValueOrDefault(), Is.EqualTo(3));
            Assert.That(GetMaybe().ToString(), Is.EqualTo("HasValue = true, Value = 3"));
        }

        [Test]
        public void ImplicitCastOfNone()
        {
            // implicit cast of T to Maybe<T>
            static Maybe<int> GetMaybe() => Maybe.None;

            Assert.That(GetMaybe().ToString(), Is.EqualTo("HasValue = false"));
        }

        [Test]
        public void ValueOr()
        {
            Assert.That(Maybe.Some(3).ValueOr(42), Is.EqualTo(3));
            Assert.That(Maybe<int>.None.ValueOr(42), Is.EqualTo(42));
        }

        [Test]
        public void TryGetValue()
        {
            Assert.That(Maybe.Some(3).TryGetValue(out var value));
            Assert.That(value, Is.EqualTo(3));

            Assert.That(Maybe<int>.None.TryGetValue(out _), Is.False);
        }

        [Test]
        public void Map()
        {
            Assert.Throws<ArgumentNullException>(() => Maybe.Some(3).Map<int>(null));

            Assert.That(Maybe.Some(3).Map(x => x + 2).ValueOr(0), Is.EqualTo(5));
            Assert.That(Maybe<int>.None.Map(x => x + 2).ValueOr(0), Is.EqualTo(0));
        }

        [Test]
        public void Bind()
        {
            Assert.Throws<ArgumentNullException>(() => Maybe.Some(3).Bind<int>(null));

            Assert.That(Maybe.Some(3).Bind(x => Maybe.Some(x.ToString())).ValueOr("bah"), Is.EqualTo("3"));
            Assert.That(Maybe<int>.None.Bind(x => Maybe.Some(x.ToString())).ValueOr("bah"), Is.EqualTo("bah"));
        }

        [Test]
        public void Equality()
        {
            // Maybe.None is equal to...
            Assert.That(Maybe.None.Equals(Maybe.None));
            Assert.That(Maybe.None.Equals(Maybe<int>.None));

            // Maybe.None<int> is equal to...
            Assert.That(Maybe<int>.None.Equals(Maybe<int>.None));
            Assert.That(Maybe<int>.None.Equals(Maybe.None));

            // but not that one
            Assert.That(Maybe<int>.None.Equals(Maybe<float>.None), Is.False);

            Assert.That(Maybe.None == Maybe.None);

            Assert.That(Maybe<int>.None == Maybe<int>.None);
            Assert.That(Maybe.None == Maybe<int>.None);
            Assert.That(Maybe<int>.None == Maybe.None);

            Assert.That(Maybe.Some(3).Equals(Maybe.Some(3)));
            Assert.That(!Maybe.Some(3).Equals(Maybe.Some(4)));
            Assert.That(!Maybe.Some(3).Equals(Maybe.None));
            Assert.That(!Maybe.Some(3).Equals("meh"));

            Assert.That(Maybe.Some(3) == Maybe.Some(3));
            Assert.That(Maybe.Some(3) != Maybe.Some(4));
            Assert.That(Maybe.Some(3) != Maybe.None);

            //
            Assert.That(3 == 3.0); // implicit int -> double
            Assert.That(3.Equals(3.0), Is.False);

            Assert.That(Maybe.Some(3) == (int) 3.0); // explicit
            Assert.That(Maybe.Some(3.0) == (double) 3); // implicit

            // does not build - not supposed to be possible
            //Assert.That(Maybe.Some(3) == Maybe.Some(3.0));

            Assert.That(Maybe.Some(3) == 3);
            Assert.That(Maybe.Some(3) != 4);

            Assert.That(Maybe.Some(3).Equals(3));
            Assert.That(!Maybe.Some(3).Equals(4));


            Assert.That(Maybe.Some(3).Equals(Maybe.Some(3.0)), Is.False);

            Assert.That(Maybe.Some(3), Is.EqualTo(3));

            Assert.That(Maybe.Some(3).GetHashCode(), Is.EqualTo(3));
        }
    }
}
