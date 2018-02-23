// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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

using System.Linq;
using Hazelcast.Core;
using Hazelcast.IO;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    public class CustomPredicate<K, V> : IPredicate<K, V>
    {
        public void ReadData(IObjectDataInput input)
        {
        }

        public void WriteData(IObjectDataOutput output)
        {
        }

        public int GetFactoryId()
        {
            return 0;
        }

        public int GetId()
        {
            return 0;
        }
    }

    [TestFixture]
    public class PredicatesTest : SingleMemberBaseTest
    {
        [SetUp]
        public void Init()
        {
            map = Client.GetMap<object, object>(TestSupport.RandomString());
        }

        [TearDown]
        public static void Destroy()
        {
            map.Clear();
        }

        internal static IMap<object, object> map;

        private void FillMap()
        {
            for (var i = 0; i < 10; i++)
            {
                map.Put("key-" + i, "value-" + i);
            }
        }

        private void FillMapNumericValues()
        {
            for (var i = 0; i < 10; i++)
            {
                map.Put(i, i);
            }
        }

        [Test]
        public virtual void TesCustomPredicate()
        {
            var mapOther = Client.GetMap<int, string>(TestSupport.RandomString());
            var ep  = new EqualPredicate();
            var cp = new CustomPredicate<int, string>();
            var ap = new AndPredicate(ep, cp);
            try
            {
                var keySet = mapOther.KeySet(ap);
            }
            finally
            {
                Assert.Pass("Predicate generics related compile test, if compiles it passes");
            }
        }


        [Test]
        public virtual void TestAnd()
        {
            FillMap();
            var predicate = CreateAndPredicate();
            var keySet = map.KeySet(predicate);
            Assert.That(new[] {"key-1"}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public virtual void TestBetween()
        {
            FillMapNumericValues();
            var predicate = CreateBetween();
            var keySet = map.KeySet(predicate);
            Assert.That(new[] {4, 5, 6}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public virtual void TestEqual()
        {
            FillMap();
            var predicate = CreateEqual();
            var keySet = map.KeySet(predicate);
            Assert.That(new[] {"key-0"}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public virtual void TestFalse()
        {
            FillMap();
            var predicate = CreateFalse();
            var keySet = map.KeySet(predicate);
            Assert.AreEqual(0, keySet.Count);
        }

        [Test]
        public virtual void TestGreaterThan()
        {
            FillMap();
            var predicate = CreateGreaterThan();
            var keySet = map.KeySet(predicate);
            Assert.That(new[] {"key-9"}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public virtual void TestGreaterThanOrEqual()
        {
            FillMap();
            var predicate = CreateGreaterThanOrEqual();
            var keySet = map.KeySet(predicate);
            Assert.That(new[] {"key-8", "key-9"}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public virtual void TestILike()
        {
            map.Put("key-1", "a_value");
            map.Put("key-2", "b_value");
            map.Put("key-3", "aa_value");
            map.Put("key-4", "AA_value");
            var predicate = CreateILike();
            var keySet = map.KeySet(predicate);
            Assert.That(new[] {"key-1", "key-3", "key-4"}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public virtual void TestIn()
        {
            FillMapNumericValues();
            var predicate = CreateIn();
            var keySet = map.KeySet(predicate);
            Assert.That(new[] {1, 5, 7}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public virtual void TestLessThan()
        {
            FillMap();
            var predicate = CreateLessThan();
            var keySet = map.KeySet(predicate);
            Assert.That(new[] {"key-0"}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public virtual void TestLessThanOrEqual()
        {
            FillMap();
            var predicate = CreateLessThanOrEqual();
            var keySet = map.KeySet(predicate);
            Assert.That(new[] {"key-0", "key-1"}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public virtual void TestLike()
        {
            map.Put("key-1", "a_value");
            map.Put("key-2", "b_value");
            map.Put("key-3", "aa_value");
            map.Put("key-4", "AA_value");
            var predicate = CreateLike();
            var keySet = map.KeySet(predicate);
            Assert.That(new[] {"key-1", "key-3"}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public virtual void TestNot()
        {
            FillMap();
            var predicate = CreateNot();
            var keySet = map.KeySet(predicate);
            Assert.That(new[] {"key-0"}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public virtual void TestNotEqual()
        {
            FillMap();
            var predicate = CreateNotEqual();
            var keySet = map.KeySet(predicate);
            Assert.That(new[] {"key-1", "key-2", "key-3", "key-4", "key-5", "key-6", "key-7", "key-8", "key-9"},
                Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public virtual void TestOr()
        {
            FillMap();
            var predicate = CreateOr();
            var keySet = map.KeySet(predicate);
            Assert.That(new[] {"key-1", "key-2"}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public virtual void TestRegex()
        {
            map.Put("key-1", "car");
            map.Put("key-2", "cry");
            map.Put("key-3", "giraffe");
            var predicate = CreateRegex();
            var keySet = map.KeySet(predicate);
            Assert.That(new[] {"key-1", "key-2"}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public virtual void TestSql()
        {
            FillMap();
            var predicate = CreateSql();
            var keySet = map.KeySet(predicate);
            Assert.That(new[] {"key-1"}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public virtual void TestTrue()
        {
            FillMap();
            var predicate = CreateTrue();
            var keySet = map.KeySet(predicate);
            Assert.That(map.KeySet(), Is.EquivalentTo(keySet.ToArray()));
        }        
        
        [Test]
        public virtual void TestAndSerialize()
        {
            var predicate = CreateAndPredicate();
            map.Put("key", predicate);
            map.Put("key", predicate);
            Assert.AreEqual(predicate, map.Get("key"));
        }

        [Test]
        public virtual void TestBetweenSerialize()
        {
            var predicate = CreateBetween();
            map.Put("key", predicate);
            Assert.AreEqual(predicate, map.Get("key"));
        }

        [Test]
        public virtual void TestEqualSerialize()
        {
            var predicate = CreateEqual();
            map.Put("key", predicate);
            Assert.AreEqual(predicate, map.Get("key"));
        }

        [Test]
        public virtual void TestFalseSerialize()
        {
            var predicate = CreateFalse();
            map.Put("key", predicate);
            Assert.AreEqual(predicate, map.Get("key"));
        }

        [Test]
        public virtual void TestGreaterThanSerialize()
        {
            var predicate = CreateGreaterThan();
            map.Put("key", predicate);
            Assert.AreEqual(predicate, map.Get("key"));
        }

        [Test]
        public virtual void TestGreaterThanOrEqualSerialize()
        {
            var predicate = CreateGreaterThanOrEqual();
            map.Put("key", predicate);
            Assert.AreEqual(predicate, map.Get("key"));
        }

        [Test]
        public virtual void TestILikeSerialize()
        {
            var predicate = CreateILike();
            map.Put("key", predicate);
            Assert.AreEqual(predicate, map.Get("key"));
        }

        [Test]
        public virtual void TestInSerialize()
        {
            var predicate = CreateIn();
            map.Put("key", predicate);
            Assert.AreEqual(predicate, map.Get("key"));
        }

        [Test]
        public virtual void TestLessThanSerialize()
        {
            var predicate = CreateLessThan();
            map.Put("key", predicate);
            Assert.AreEqual(predicate, map.Get("key"));
        }

        [Test]
        public virtual void TestLessThanOrEqualSerialize()
        {
            var predicate = CreateLessThanOrEqual();
            map.Put("key", predicate);
            Assert.AreEqual(predicate, map.Get("key"));
        }

        [Test]
        public virtual void TestLikeSerialize()
        {
            var predicate = CreateLike();
            map.Put("key", predicate);
            Assert.AreEqual(predicate, map.Get("key"));
        }

        [Test]
        public virtual void TestNotSerialize()
        {
            var predicate = CreateNot();
            map.Put("key", predicate);
            Assert.AreEqual(predicate, map.Get("key"));
        }

        [Test]
        public virtual void TestNotEqualSerialize()
        {
            var predicate = CreateNotEqual();
            map.Put("key", predicate);
            Assert.AreEqual(predicate, map.Get("key"));
        }

        [Test]
        public virtual void TestOrSerialize()
        {
            var predicate = CreateOr();
            map.Put("key", predicate);
            Assert.AreEqual(predicate, map.Get("key"));
        }

        [Test]
        public virtual void TestRegexSerialize()
        {
            var predicate = CreateRegex();
            map.Put("key", predicate);
            Assert.AreEqual(predicate, map.Get("key"));
        }

        [Test]
        public virtual void TestSqlSerialize()
        {
            var predicate = CreateSql();
            map.Put("key", predicate);
            Assert.AreEqual(predicate, map.Get("key"));
        }

        [Test]
        public virtual void TestTrueSerialize()
        {
            var predicate = CreateTrue();
            map.Put("key", predicate);
            Assert.AreEqual(predicate, map.Get("key"));
        }        
        
        [Test]
        public virtual void TestInstanceOfSerialize()
        {
            var predicate = CreateInstanceOf();
            map.Put("key", predicate);
            Assert.AreEqual(predicate, map.Get("key"));
        }

        private IPredicate CreateAndPredicate()
        {
            var predicate0 = Predicates.IsLessThan("this", "value-2");
            var predicate1 = Predicates.IsGreaterThan("this", "value-0");
            return new AndPredicate(predicate0, predicate1);
        }

        private IPredicate CreateBetween()
        {
            return Predicates.IsBetween("this", 4, 6);
        }

        private IPredicate CreateEqual()
        {
            return Predicates.IsEqual("this", "value-0");
        }

        private IPredicate CreateFalse()
        {
            return Predicates.False();
        }

        private IPredicate CreateGreaterThan()
        {
            return Predicates.IsGreaterThan("this", "value-8");
        }

        private IPredicate CreateGreaterThanOrEqual()
        {
            return Predicates.IsGreaterThanOrEqual("this", "value-8");
        }

        private IPredicate CreateILike()
        {
            return Predicates.IsILike("this", "a%");
        }

        private IPredicate CreateIn()
        {
            return Predicates.IsIn("this", 1, 5, 7);
        }

        private IPredicate CreateLessThan()
        {
            return Predicates.IsLessThan("this", "value-1");
        }

        private IPredicate CreateLessThanOrEqual()
        {
            return Predicates.IsLessThanOrEqual("this", "value-1");
        }

        private IPredicate CreateLike()
        {
            return Predicates.IsLike("this", "a%");
        }

        private IPredicate CreateNot()
        {
            var predicate0 = Predicates.IsNotEqual("this", "value-0");
            return Predicates.Not(predicate0);
        }

        private IPredicate CreateNotEqual()
        {
            return Predicates.IsNotEqual("this", "value-0");
        }

        private IPredicate CreateOr()
        {
            var predicate0 = Predicates.IsEqual("this", "value-1");
            var predicate1 = Predicates.IsEqual("this", "value-2");
            return Predicates.Or(predicate0, predicate1);
        }

        private IPredicate CreateRegex()
        {
            return Predicates.MatchesRegex("this", "c[ar].*");
        }

        private IPredicate CreateSql()
        {
            return Predicates.Sql("this == 'value-1'");
        }

        private IPredicate CreateTrue()
        {
            return Predicates.True();
        }

        private IPredicate CreateInstanceOf()
        {
            return Predicates.InstanceOf("java.lang.Integer");
        }
    }
}