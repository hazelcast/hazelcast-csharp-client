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

using System;
using System.Collections.Generic;
using System.Numerics;
using Hazelcast.Client.Model;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Util;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    [Category("3.8")]
    public class AggregatorAndProjectionTest : SingleMemberBaseTest
    {
        internal static IMap<object, object> map;

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

        protected override void ConfigureClient(ClientConfig config)
        {
            base.ConfigureClient(config);
            config.GetSerializationConfig().AddPortableFactory(1, new PortableFactory());
            config.GetSerializationConfig()
                .AddDataSerializableFactory(IdentifiedFactory.FactoryId, new IdentifiedFactory());
        }

        private void FillMap<T>()
        {
            for (var i = 0; i < 10; i++)
            {
                var key = TestSupport.RandomString();
                if (typeof(T) == typeof(int))
                {
                    map.Put(key, i);
                }
                else if (typeof(T) == typeof(BigInteger))
                {
                    map.Put(key, new BigInteger(i));
                }
                else if (typeof(T) == typeof(double))
                {
                    map.Put(key, (double) i);
                }
                else if (typeof(T) == typeof(float))
                {
                    map.Put(key, (float) i);
                }
                else if (typeof(T) == typeof(long))
                {
                    map.Put(key, (long) i);
                }
                else if (typeof(T) == typeof(int))
                {
                    map.Put(key, (int) i);
                }
                else if (typeof(T) == typeof(Header))
                {
                    map.Put(key, new Header(i, new Handle(false)));
                }
                else if (typeof(T) == typeof(Item))
                {
                    var enabled = new int[1];
                    var disabled = new int[1];
                    map.Put(key, new Item(new Header(i, new Handle(false)), enabled, disabled));
                }
            }
        }

        [Test]
        public void test_Count()
        {
            FillMap<int>();
            Assert.AreEqual(10, map.Aggregate(Aggregators.Count()));
            Assert.AreEqual(10, map.Aggregate(Aggregators.Count("this")));
        }

        [Test]
        public void test_DoubleAvg()
        {
            FillMap<double>();
            Assert.AreEqual(4.5d, map.Aggregate(Aggregators.DoubleAvg()));
            Assert.AreEqual(4.5d, map.Aggregate(Aggregators.DoubleAvg("this")));
        }

        [Test]
        public void test_IntegerAvg()
        {
            FillMap<int>();
            Assert.AreEqual(4.5d, map.Aggregate(Aggregators.IntegerAvg()));
            Assert.AreEqual(4.5d, map.Aggregate(Aggregators.IntegerAvg("this")));
        }

        [Test]
        public void test_LongAvg()
        {
            FillMap<long>();
            Assert.AreEqual(4.5d, map.Aggregate(Aggregators.LongAvg()));
            Assert.AreEqual(4.5d, map.Aggregate(Aggregators.LongAvg("this")));
        }

        [Test]
        public void test_NumberAvg()
        {
            FillMap<float>();
            FillMap<double>();
            Assert.AreEqual(4.5d, map.Aggregate(Aggregators.NumberAvg()));
            Assert.AreEqual(4.5d, map.Aggregate(Aggregators.NumberAvg("this")));
        }

        [Test]
        public void test_Max()
        {
            FillMap<int>();
            Assert.AreEqual(9, map.Aggregate(Aggregators.Max<int>()));
            Assert.AreEqual(9, map.Aggregate(Aggregators.Max<int>("this")));
        }

        [Test]
        public void test_Min()
        {
            FillMap<int>();
            Assert.AreEqual(0, map.Aggregate(Aggregators.Min<int>()));
            Assert.AreEqual(0, map.Aggregate(Aggregators.Min<int>("this")));
        }

        [Test]
        public void test_BigIntegerSum()
        {
            FillMap<BigInteger>();
            Assert.AreEqual(new BigInteger(45), map.Aggregate(Aggregators.BigIntegerSum()));
            Assert.AreEqual(new BigInteger(45), map.Aggregate(Aggregators.BigIntegerSum("this")));
        }

        [Test]
        public void test_DoubleSum()
        {
            FillMap<double>();
            Assert.AreEqual(45d, map.Aggregate(Aggregators.DoubleSum()));
            Assert.AreEqual(45d, map.Aggregate(Aggregators.DoubleSum("this")));
        }

        [Test]
        public void test_IntegerSum()
        {
            FillMap<int>();
            Assert.AreEqual(45, map.Aggregate(Aggregators.IntegerSum()));
            Assert.AreEqual(45, map.Aggregate(Aggregators.IntegerSum("this")));
        }

        [Test]
        public void test_LongSum()
        {
            FillMap<long>();
            Assert.AreEqual(45, map.Aggregate(Aggregators.LongSum()));
            Assert.AreEqual(45, map.Aggregate(Aggregators.LongSum("this")));
        }

        [Test]
        public void test_FixedPointSum()
        {
            FillMap<float>();
            FillMap<double>();
            Assert.AreEqual(90, map.Aggregate(Aggregators.FixedPointSum()));
            Assert.AreEqual(90, map.Aggregate(Aggregators.FixedPointSum("this")));
        }

        [Test]
        public void test_FloatingPointSum()
        {
            FillMap<float>();
            Assert.AreEqual(45d, map.Aggregate(Aggregators.FloatingPointSum()));
            Assert.AreEqual(45d, map.Aggregate(Aggregators.FloatingPointSum("this")));
        }

        [Test]
        public void test_Count_with_Predicate()
        {
            FillMap<int>();
            var predicate = Predicates.IsGreaterThan("this", 5);
            Assert.AreEqual(4, map.Aggregate(Aggregators.Count(), predicate));
            Assert.AreEqual(4, map.Aggregate(Aggregators.Count("this"), predicate));
        }

        [Test]
        public void test_LongSum_field()
        {
            FillMap<Header>();
            Assert.AreEqual(45, map.Aggregate(Aggregators.LongSum("id")));
        }

        [Test]
        public void test_LongSum_withPredicate_field()
        {
            FillMap<Header>();
            var predicate = Predicates.IsGreaterThan("id", 5);
            Assert.AreEqual(30, map.Aggregate(Aggregators.LongSum("id"), predicate));
        }

        [Test]
        public void test_AggregateAny()
        {
            FillMap<Item>();
            Assert.AreEqual(10, map.Aggregate(Aggregators.Count("enabled[any]")));
        }

        [Test]
		public void testAggregate_NullPredicate()
		{
			Assert.Throws<ArgumentException>(() =>
        {
            map.Aggregate(Aggregators.LongSum("id"), null);
        });
		}

        //Projection tests
        [Test]
        public void test_SingleAttributeProjection()
        {
            FillMap<Header>();
            var result = (ReadOnlyLazyList<long>) map.Project<long>(new SingleAttributeProjection("id"));
            var expected = new HashSet<long> {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
            CollectionAssert.AreEquivalent(expected, result);
        }

        [Test]
        public void test_SingleAttributeProjection_withPredicate()
        {
            FillMap<Header>();
            var predicate = Predicates.IsGreaterThan("id", 5);
            var result = (ReadOnlyLazyList<long>) map.Project<long>(new SingleAttributeProjection("id"), predicate);
            var expected = new HashSet<long> {6, 7, 8, 9};
            CollectionAssert.AreEquivalent(expected, result);
        }

        [Test]
		public void test_NullProjection()
		{
			Assert.Throws<ArgumentException>(() =>
        {
            map.Project<long>(null);
        });
		}

        [Test]
		public void testProjection_NullPredicate()
		{
			Assert.Throws<ArgumentException>(() =>
        {
            map.Project<long>(new SingleAttributeProjection("id"), null);
        });
		}

        [Test]
		public void test_NullPredicateAndProjection()
		{
			Assert.Throws<ArgumentException>(() =>
        {
            map.Project<long>(null, null);
        });
		}
    }
}