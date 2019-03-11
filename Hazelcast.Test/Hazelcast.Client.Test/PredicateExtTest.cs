// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Client.Test.Serialization;
using Hazelcast.Config;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class PredicateExtTest : SingleMemberBaseTest
    {
        private IMap<NamedPortable, NamedPortable> map;

        [SetUp]
        public void Init()
        {
            map = Client.GetMap<NamedPortable, NamedPortable>(TestSupport.RandomString());
        }

        [TearDown]
        public void Destroy()
        {
            map.Clear();
        }

        protected override void ConfigureClient(ClientConfig config)
        {
            base.ConfigureClient(config);
            config.GetSerializationConfig().AddPortableFactory(1, new PortableSerializationTest.TestPortableFactory());
        }


        private void FillMap()
        {
            for (var i = 0; i < 100; i++)
            {
                map.Put(new NamedPortable("key-" + i, i), new NamedPortable("value-" + i, i));
            }
        }


        [Test]
        public virtual void TestPredicate_key_property()
        {
            FillMap();
            var predicate = Predicates.Key("myint").GreaterThanOrEqual(50);
            var values = map.Values(predicate);
            Assert.AreEqual(50, values.Count);
        }

        [Test]
        public virtual void TestPredicate_value_property()
        {
            FillMap();
            var predicate = Predicates.Property("name").Like("value-%");
            var values = map.Values(predicate);
            Assert.AreEqual(100, values.Count);
        }

        [Test]
        public virtual void TestPredicate_value_property_keyset()
        {
            FillMap();
            var predicate = Predicates.Property("name").Like("value-%");
            var values = map.KeySet(predicate);
            Assert.AreEqual(100, values.Count);
        }

        [Test]
        public virtual void TestPredicateExt_key_property()
        {
            var predicateProperty = Predicates.Key("id");
            Assert.AreEqual(predicateProperty.Property, "__key#id");
        }

        [Test]
        public virtual void TestPredicateExt_value_property()
        {
            var predicateProperty = Predicates.Property("name");
            Assert.AreEqual(predicateProperty.Property, "name");
        }

        [Test]
        public virtual void TestPredicateExt_key_equal()
        {
            var predicate = Predicates.Key("name").Equal("value-1");
            Assert.AreEqual(predicate, new EqualPredicate("__key#name", "value-1"));
        }

        [Test]
        public virtual void TestPredicateExt_value_equal()
        {
            var predicate = Predicates.Property("name").Equal("value-1");
            Assert.AreEqual(predicate, new EqualPredicate("name", "value-1"));
        }

        [Test]
        public virtual void TestPredicateExt_complex_chained()
        {
            var predicate = Predicates.Key("id").Equal("id-1").And(Predicates.Property("name").ILike("a%"));

            var predicate2 = Predicates.And(Predicates.IsEqual("__key#id", "id-1"),
                Predicates.IsILike("name", "a%")
            );
            Assert.AreEqual(predicate, predicate2);
        }

        [Test]
        public virtual void TestPredicateExt_And()
        {
            var predicate1 = Predicates.Property("name").Equal("val")
                .And(Predicates.Property("id").GreaterThan(10));
            var predicate2 = Predicates.And(Predicates.IsEqual("name", "val"), Predicates.IsGreaterThan("id", 10));
            Assert.AreEqual(predicate1, predicate2);
        }

        [Test]
        public virtual void TestPredicateExt_Between()
        {
            var predicate1 = Predicates.Property("name").Between(10, 20);
            var predicate2 = Predicates.IsBetween("name", 10, 20);
            Assert.AreEqual(predicate1, predicate2);
        }

        [Test]
        public virtual void TestPredicateExt_Equal()
        {
            var predicate1 = Predicates.Property("name").Equal("val");
            var predicate2 = Predicates.IsEqual("name", "val");
            Assert.AreEqual(predicate1, predicate2);
        }

        [Test]
        public virtual void TestPredicateExt_NotEqual()
        {
            var predicate1 = Predicates.Property("name").NotEqual("val");
            var predicate2 = Predicates.IsNotEqual("name", "val");
            Assert.AreEqual(predicate1, predicate2);
        }

        [Test]
        public virtual void TestPredicateExt_GreaterThan()
        {
            var predicate1 = Predicates.Property("name").GreaterThan(10);
            var predicate2 = Predicates.IsGreaterThan("name", 10);
            Assert.AreEqual(predicate1, predicate2);
        }

        [Test]
        public virtual void TestPredicateExt_GreaterThanOrEqual()
        {
            var predicate1 = Predicates.Property("name").GreaterThanOrEqual(10);
            var predicate2 = Predicates.IsGreaterThanOrEqual("name", 10);
            Assert.AreEqual(predicate1, predicate2);
        }

        [Test]
        public virtual void TestPredicateExt_In()
        {
            var predicate1 = Predicates.Property("name").In(10, 20, 30);
            var predicate2 = Predicates.IsIn("name", 10, 20, 30);
            Assert.AreEqual(predicate1, predicate2);
        }

        [Test]
        public virtual void TestPredicateExt_ILike()
        {
            var predicate1 = Predicates.Property("name").ILike("val");
            var predicate2 = Predicates.IsILike("name", "val");
            Assert.AreEqual(predicate1, predicate2);
        }

        [Test]
        public virtual void TestPredicateExt_Like()
        {
            var predicate1 = Predicates.Property("name").Like("val");
            var predicate2 = Predicates.IsLike("name", "val");
            Assert.AreEqual(predicate1, predicate2);
        }

        [Test]
        public virtual void TestPredicateExt_MatchesRegex()
        {
            var predicate1 = Predicates.Property("name").MatchesRegex("[a-z]");
            var predicate2 = Predicates.MatchesRegex("name", "[a-z]");
            Assert.AreEqual(predicate1, predicate2);
        }

        [Test]
        public virtual void TestPredicateExt_LessThan()
        {
            var predicate1 = Predicates.Property("name").LessThan(10);
            var predicate2 = Predicates.IsLessThan("name", 10);
            Assert.AreEqual(predicate1, predicate2);
        }

        [Test]
        public virtual void TestPredicateExt_LessThanOrEqual()
        {
            var predicate1 = Predicates.Property("name").LessThanOrEqual(10);
            var predicate2 = Predicates.IsLessThanOrEqual("name", 10);
            Assert.AreEqual(predicate1, predicate2);
        }

        [Test]
        public virtual void TestPredicateExt_Or()
        {
            var predicate1 = Predicates.Property("name").Equal("val")
                .Or(Predicates.Property("id").GreaterThan(10));
            var predicate2 = Predicates.Or(Predicates.IsEqual("name", "val"), Predicates.IsGreaterThan("id", 10));
            Assert.AreEqual(predicate1, predicate2);
        }

        [Test]
        public virtual void TestPredicateExt_Not()
        {
            var predicate1 = Predicates.Property("name").Equal("val").Not();
            var predicate2 = Predicates.Not(Predicates.IsEqual("name", "val"));
            Assert.AreEqual(predicate1, predicate2);
        }

    }
}