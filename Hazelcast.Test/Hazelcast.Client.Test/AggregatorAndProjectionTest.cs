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
using Hazelcast.IO.Serialization;
using Hazelcast.Util;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    [Category("3.8")]
    public class AggregatorAndProjectionTest : SingleMemberBaseTest
    {
        internal IMap<object, object> Map;

        [SetUp]
        public void Init()
        {
            Map = Client.GetMap<object, object>(TestSupport.RandomString());
        }

        [TearDown]
        public void Destroy()
        {
            Map.Clear();
        }

        protected override void ConfigureClient(ClientConfig config)
        {
            base.ConfigureClient(config);
            config.GetSerializationConfig().AddPortableFactory(1, new PortableFactory());
            config.GetSerializationConfig()
                .AddDataSerializableFactory(IdentifiedFactory.FactoryId, new IdentifiedFactory());
        }

        void FillMap<T>()
        {
            for (var i = 0; i < 10; i++)
            {
                var key = TestSupport.RandomString();
                if (typeof(T) == typeof(int))
                {
                    Map.Put(key, i);
                }
                else if (typeof(T) == typeof(BigInteger))
                {
                    Map.Put(key, new BigInteger(i));
                }
                else if (typeof(T) == typeof(double))
                {
                    Map.Put(key, (double)i);
                }
                else if (typeof(T) == typeof(float))
                {
                    Map.Put(key, (float)i);
                }
                else if (typeof(T) == typeof(long))
                {
                    Map.Put(key, (long)i);
                }
                else if (typeof(T) == typeof(int))
                {
                    Map.Put(key, (int)i);
                }
                else if (typeof(T) == typeof(Header))
                {
                    Map.Put(key, new Header(i, new Handle(false)));
                }
                else if (typeof(T) == typeof(Item))
                {
                    var enabled = new int[1];
                    var disabled = new int[1];
                    Map.Put(key, new Item(new Header(i, new Handle(false)), enabled, disabled));
                }
            }
        }

        [Test]
        public void Count()
        {
            FillMap<int>();
            Assert.AreEqual(10, Map.Aggregate(Aggregators.Count()));
            Assert.AreEqual(10, Map.Aggregate(Aggregators.Count("this")));
        }

        [Test]
        public void DoubleAvg()
        {
            FillMap<double>();
            Assert.AreEqual(4.5d, Map.Aggregate(Aggregators.DoubleAvg()));
            Assert.AreEqual(4.5d, Map.Aggregate(Aggregators.DoubleAvg("this")));
        }

        [Test]
        public void IntegerAvg()
        {
            FillMap<int>();
            Assert.AreEqual(4.5d, Map.Aggregate(Aggregators.IntegerAvg()));
            Assert.AreEqual(4.5d, Map.Aggregate(Aggregators.IntegerAvg("this")));
        }

        [Test]
        public void LongAvg()
        {
            FillMap<long>();
            Assert.AreEqual(4.5d, Map.Aggregate(Aggregators.LongAvg()));
            Assert.AreEqual(4.5d, Map.Aggregate(Aggregators.LongAvg("this")));
        }

        [Test]
        public void NumberAvg()
        {
            FillMap<float>();
            FillMap<double>();
            Assert.AreEqual(4.5d, Map.Aggregate(Aggregators.NumberAvg()));
            Assert.AreEqual(4.5d, Map.Aggregate(Aggregators.NumberAvg("this")));
        }

        [Test]
        public void Max()
        {
            FillMap<int>();
            Assert.AreEqual(9, Map.Aggregate(Aggregators.Max<int>()));
            Assert.AreEqual(9, Map.Aggregate(Aggregators.Max<int>("this")));
        }

        [Test]
        public void Min()
        {
            FillMap<int>();
            Assert.AreEqual(0, Map.Aggregate(Aggregators.Min<int>()));
            Assert.AreEqual(0, Map.Aggregate(Aggregators.Min<int>("this")));
        }

        [Test]
        public void BigIntegerSum()
        {
            FillMap<BigInteger>();
            Assert.AreEqual(new BigInteger(45), Map.Aggregate(Aggregators.BigIntegerSum()));
            Assert.AreEqual(new BigInteger(45), Map.Aggregate(Aggregators.BigIntegerSum("this")));
        }

        [Test]
        public void DoubleSum()
        {
            FillMap<double>();
            Assert.AreEqual(45d, Map.Aggregate(Aggregators.DoubleSum()));
            Assert.AreEqual(45d, Map.Aggregate(Aggregators.DoubleSum("this")));
        }

        [Test]
        public void IntegerSum()
        {
            FillMap<int>();
            Assert.AreEqual(45, Map.Aggregate(Aggregators.IntegerSum()));
            Assert.AreEqual(45, Map.Aggregate(Aggregators.IntegerSum("this")));
        }

        [Test]
        public void LongSum()
        {
            FillMap<long>();
            Assert.AreEqual(45, Map.Aggregate(Aggregators.LongSum()));
            Assert.AreEqual(45, Map.Aggregate(Aggregators.LongSum("this")));
        }

        [Test]
        public void FixedPointSum()
        {
            FillMap<float>();
            FillMap<double>();
            Assert.AreEqual(90, Map.Aggregate(Aggregators.FixedPointSum()));
            Assert.AreEqual(90, Map.Aggregate(Aggregators.FixedPointSum("this")));
        }

        [Test]
        public void FloatingPointSum()
        {
            FillMap<float>();
            Assert.AreEqual(45d, Map.Aggregate(Aggregators.FloatingPointSum()));
            Assert.AreEqual(45d, Map.Aggregate(Aggregators.FloatingPointSum("this")));
        }

        [Test]
        public void Count_with_Predicate()
        {
            FillMap<int>();
            var predicate = Predicates.IsGreaterThan("this", 5);
            Assert.AreEqual(4, Map.Aggregate(Aggregators.Count(), predicate));
            Assert.AreEqual(4, Map.Aggregate(Aggregators.Count("this"), predicate));
        }

        [Test]
        public void LongSum_field()
        {
            FillMap<Header>();
            Assert.AreEqual(45, Map.Aggregate(Aggregators.LongSum("id")));
        }

        [Test]
        public void LongSum_withPredicate_field()
        {
            FillMap<Header>();
            var predicate = Predicates.IsGreaterThan("id", 5);
            Assert.AreEqual(30, Map.Aggregate(Aggregators.LongSum("id"), predicate));
        }

        [Test]
        public void AggregateAny()
        {
            FillMap<Item>();
            Assert.AreEqual(10, Map.Aggregate(Aggregators.Count("enabled[any]")));
        }

        [Test]
        public void Aggregate_NullPredicate()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                Map.Aggregate(Aggregators.LongSum("id"), null);
            });
        }

        //Projection tests
        [Test]
        public void SingleAttributeProjection()
        {
            FillMap<Header>();
            var result = (ReadOnlyLazyList<long, IData>)Map.Project<long>(new SingleAttributeProjection("id"));
            var expected = new HashSet<long> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            CollectionAssert.AreEquivalent(expected, result);
        }

        [Test]
        public void SingleAttributeProjection_withPredicate()
        {
            FillMap<Header>();
            var predicate = Predicates.IsGreaterThan("id", 5);
            var result = (ReadOnlyLazyList<long, IData>)Map.Project<long>(new SingleAttributeProjection("id"), predicate);
            var expected = new HashSet<long> { 6, 7, 8, 9 };
            CollectionAssert.AreEquivalent(expected, result);
        }

        [Test]
        public void NullProjection()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                Map.Project<long>(null);
            });
        }

        [Test]
        public void Projection_NullPredicate()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                Map.Project<long>(new SingleAttributeProjection("id"), null);
            });
        }

        [Test]
        public void NullPredicateAndProjection()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                Map.Project<long>(null, null);
            });
        }
    }
}