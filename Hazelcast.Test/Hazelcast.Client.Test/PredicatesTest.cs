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

using System.Linq;
using Hazelcast.Core;
using Hazelcast.IO;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    class CustomPredicate<K, V> : IPredicate<K, V>
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
            _map = Client.GetMap<object, object>(TestSupport.RandomString());
        }

        [TearDown]
        public void Destroy()
        {
            _map.Clear();
        }

        IMap<object, object> _map;

        void FillMap()
        {
            for (var i = 0; i < 10; i++)
            {
                _map.Put("key-" + i, "value-" + i);
            }
        }

        void FillMapNumericValues()
        {
            for (var i = 0; i < 10; i++)
            {
                _map.Put(i, i);
            }
        }

        [Test]
        public void CustomPredicate()
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
        public void And()
        {
            FillMap();
            var predicate = CreateAndPredicate();
            var keySet = _map.KeySet(predicate);
            Assert.That(new[] {"key-1"}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public void Between()
        {
            FillMapNumericValues();
            var predicate = CreateBetween();
            var keySet = _map.KeySet(predicate);
            Assert.That(new[] {4, 5, 6}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public void Equal()
        {
            FillMap();
            var predicate = CreateEqual();
            var keySet = _map.KeySet(predicate);
            Assert.That(new[] {"key-0"}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public void False()
        {
            FillMap();
            var predicate = CreateFalse();
            var keySet = _map.KeySet(predicate);
            Assert.AreEqual(0, keySet.Count);
        }

        [Test]
        public void GreaterThan()
        {
            FillMap();
            var predicate = CreateGreaterThan();
            var keySet = _map.KeySet(predicate);
            Assert.That(new[] {"key-9"}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public void GreaterThanOrEqual()
        {
            FillMap();
            var predicate = CreateGreaterThanOrEqual();
            var keySet = _map.KeySet(predicate);
            Assert.That(new[] {"key-8", "key-9"}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public void ILike()
        {
            _map.Put("key-1", "a_value");
            _map.Put("key-2", "b_value");
            _map.Put("key-3", "aa_value");
            _map.Put("key-4", "AA_value");
            var predicate = CreateILike();
            var keySet = _map.KeySet(predicate);
            Assert.That(new[] {"key-1", "key-3", "key-4"}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public void In()
        {
            FillMapNumericValues();
            var predicate = CreateIn();
            var keySet = _map.KeySet(predicate);
            Assert.That(new[] {1, 5, 7}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public void LessThan()
        {
            FillMap();
            var predicate = CreateLessThan();
            var keySet = _map.KeySet(predicate);
            Assert.That(new[] {"key-0"}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public void LessThanOrEqual()
        {
            FillMap();
            var predicate = CreateLessThanOrEqual();
            var keySet = _map.KeySet(predicate);
            Assert.That(new[] {"key-0", "key-1"}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public void Like()
        {
            _map.Put("key-1", "a_value");
            _map.Put("key-2", "b_value");
            _map.Put("key-3", "aa_value");
            _map.Put("key-4", "AA_value");
            var predicate = CreateLike();
            var keySet = _map.KeySet(predicate);
            Assert.That(new[] {"key-1", "key-3"}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public void Not()
        {
            FillMap();
            var predicate = CreateNot();
            var keySet = _map.KeySet(predicate);
            Assert.That(new[] {"key-0"}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public void NotEqual()
        {
            FillMap();
            var predicate = CreateNotEqual();
            var keySet = _map.KeySet(predicate);
            Assert.That(new[] {"key-1", "key-2", "key-3", "key-4", "key-5", "key-6", "key-7", "key-8", "key-9"},
                Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public void Or()
        {
            FillMap();
            var predicate = CreateOr();
            var keySet = _map.KeySet(predicate);
            Assert.That(new[] {"key-1", "key-2"}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public void Regex()
        {
            _map.Put("key-1", "car");
            _map.Put("key-2", "cry");
            _map.Put("key-3", "giraffe");
            var predicate = CreateRegex();
            var keySet = _map.KeySet(predicate);
            Assert.That(new[] {"key-1", "key-2"}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public void Sql()
        {
            FillMap();
            var predicate = CreateSql();
            var keySet = _map.KeySet(predicate);
            Assert.That(new[] {"key-1"}, Is.EquivalentTo(keySet.ToArray()));
        }

        [Test]
        public void True()
        {
            FillMap();
            var predicate = CreateTrue();
            var keySet = _map.KeySet(predicate);
            Assert.That(_map.KeySet(), Is.EquivalentTo(keySet.ToArray()));
        }        
        
        [Test]
        public void AndSerialize()
        {
            var predicate = CreateAndPredicate();
            _map.Put("key", predicate);
            _map.Put("key", predicate);
            Assert.AreEqual(predicate, _map.Get("key"));
        }

        [Test]
        public void BetweenSerialize()
        {
            var predicate = CreateBetween();
            _map.Put("key", predicate);
            Assert.AreEqual(predicate, _map.Get("key"));
        }

        [Test]
        public void EqualSerialize()
        {
            var predicate = CreateEqual();
            _map.Put("key", predicate);
            Assert.AreEqual(predicate, _map.Get("key"));
        }

        [Test]
        public void FalseSerialize()
        {
            var predicate = CreateFalse();
            _map.Put("key", predicate);
            Assert.AreEqual(predicate, _map.Get("key"));
        }

        [Test]
        public void GreaterThanSerialize()
        {
            var predicate = CreateGreaterThan();
            _map.Put("key", predicate);
            Assert.AreEqual(predicate, _map.Get("key"));
        }

        [Test]
        public void GreaterThanOrEqualSerialize()
        {
            var predicate = CreateGreaterThanOrEqual();
            _map.Put("key", predicate);
            Assert.AreEqual(predicate, _map.Get("key"));
        }

        [Test]
        public void ILikeSerialize()
        {
            var predicate = CreateILike();
            _map.Put("key", predicate);
            Assert.AreEqual(predicate, _map.Get("key"));
        }

        [Test]
        public void InSerialize()
        {
            var predicate = CreateIn();
            _map.Put("key", predicate);
            Assert.AreEqual(predicate, _map.Get("key"));
        }

        [Test]
        public void LessThanSerialize()
        {
            var predicate = CreateLessThan();
            _map.Put("key", predicate);
            Assert.AreEqual(predicate, _map.Get("key"));
        }

        [Test]
        public void LessThanOrEqualSerialize()
        {
            var predicate = CreateLessThanOrEqual();
            _map.Put("key", predicate);
            Assert.AreEqual(predicate, _map.Get("key"));
        }

        [Test]
        public void LikeSerialize()
        {
            var predicate = CreateLike();
            _map.Put("key", predicate);
            Assert.AreEqual(predicate, _map.Get("key"));
        }

        [Test]
        public void NotSerialize()
        {
            var predicate = CreateNot();
            _map.Put("key", predicate);
            Assert.AreEqual(predicate, _map.Get("key"));
        }

        [Test]
        public void NotEqualSerialize()
        {
            var predicate = CreateNotEqual();
            _map.Put("key", predicate);
            Assert.AreEqual(predicate, _map.Get("key"));
        }

        [Test]
        public void OrSerialize()
        {
            var predicate = CreateOr();
            _map.Put("key", predicate);
            Assert.AreEqual(predicate, _map.Get("key"));
        }

        [Test]
        public void RegexSerialize()
        {
            var predicate = CreateRegex();
            _map.Put("key", predicate);
            Assert.AreEqual(predicate, _map.Get("key"));
        }

        [Test]
        public void SqlSerialize()
        {
            var predicate = CreateSql();
            _map.Put("key", predicate);
            Assert.AreEqual(predicate, _map.Get("key"));
        }

        [Test]
        public void TrueSerialize()
        {
            var predicate = CreateTrue();
            _map.Put("key", predicate);
            Assert.AreEqual(predicate, _map.Get("key"));
        }        
        
        [Test]
        public void InstanceOfSerialize()
        {
            var predicate = CreateInstanceOf();
            _map.Put("key", predicate);
            Assert.AreEqual(predicate, _map.Get("key"));
        }

        static IPredicate CreateAndPredicate()
        {
            var predicate0 = Predicates.IsLessThan("this", "value-2");
            var predicate1 = Predicates.IsGreaterThan("this", "value-0");
            return new AndPredicate(predicate0, predicate1);
        }

        static IPredicate CreateBetween()
        {
            return Predicates.IsBetween("this", 4, 6);
        }

        static IPredicate CreateEqual()
        {
            return Predicates.IsEqual("this", "value-0");
        }

        static IPredicate CreateFalse()
        {
            return Predicates.False();
        }

        static IPredicate CreateGreaterThan()
        {
            return Predicates.IsGreaterThan("this", "value-8");
        }

        static IPredicate CreateGreaterThanOrEqual()
        {
            return Predicates.IsGreaterThanOrEqual("this", "value-8");
        }

        static IPredicate CreateILike()
        {
            return Predicates.IsILike("this", "a%");
        }

        static IPredicate CreateIn()
        {
            return Predicates.IsIn("this", 1, 5, 7);
        }

        static IPredicate CreateLessThan()
        {
            return Predicates.IsLessThan("this", "value-1");
        }

        static IPredicate CreateLessThanOrEqual()
        {
            return Predicates.IsLessThanOrEqual("this", "value-1");
        }

        static IPredicate CreateLike()
        {
            return Predicates.IsLike("this", "a%");
        }

        static IPredicate CreateNot()
        {
            var predicate0 = Predicates.IsNotEqual("this", "value-0");
            return Predicates.Not(predicate0);
        }

        static IPredicate CreateNotEqual()
        {
            return Predicates.IsNotEqual("this", "value-0");
        }

        IPredicate CreateOr()
        {
            var predicate0 = Predicates.IsEqual("this", "value-1");
            var predicate1 = Predicates.IsEqual("this", "value-2");
            return Predicates.Or(predicate0, predicate1);
        }

        static IPredicate CreateRegex()
        {
            return Predicates.MatchesRegex("this", "c[ar].*");
        }

        static IPredicate CreateSql()
        {
            return Predicates.Sql("this == 'value-1'");
        }

        static IPredicate CreateTrue()
        {
            return Predicates.True();
        }

        static IPredicate CreateInstanceOf()
        {
            return Predicates.InstanceOf("java.lang.Integer");
        }
    }
}