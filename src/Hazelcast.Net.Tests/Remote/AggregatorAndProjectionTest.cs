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
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Hazelcast.Aggregating;
using Hazelcast.DistributedObjects;
using Hazelcast.Predicates;
using Hazelcast.Projections;
using Hazelcast.Testing;
using Hazelcast.Tests.TestObjects;
using NUnit.Framework;

namespace Hazelcast.Tests.Remote
{
    [TestFixture]
    public class AggregatorAndProjectionTest : SingleMemberClientRemoteTestBase
    {
        protected override HazelcastOptions CreateHazelcastOptions()
        {
            var options = base.CreateHazelcastOptions();
            options.Serialization.AddPortableFactory(1, new PortableFactory());
            options.Serialization.AddDataSerializableFactory(IdentifiedFactory.FactoryId, new IdentifiedFactory());
            return options;
        }

        private static async Task Fill<T>(IHDictionary<string, T> dictionary, Func<int, T> convert)
        {
            for (var i = 0; i < 10; i++)
            {
                var key = TestUtils.RandomString();
                await dictionary.SetAsync(key, convert(i));
            }
        }

        [Test]
        public async Task Test_Count()
        {
            var dictionary = await Client.GetDictionaryAsync<string, int>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await Fill(dictionary, i => i);

            Assert.AreEqual(10, await dictionary.AggregateAsync(Aggregator.Count()));
            Assert.AreEqual(10, await dictionary.AggregateAsync(Aggregator.Count("this")));
        }

        [Test]
        public async Task Test_DoubleAvg()
        {
            var dictionary = await Client.GetDictionaryAsync<string, double>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await Fill(dictionary, i => i);

            Assert.AreEqual(4.5d, await dictionary.AggregateAsync(Aggregator.DoubleAvg()));
            Assert.AreEqual(4.5d, await dictionary.AggregateAsync(Aggregator.DoubleAvg("this")));
        }

        [Test]
        public async Task Test_IntegerAvg()
        {
            var dictionary = await Client.GetDictionaryAsync<string, int>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await Fill(dictionary, i => i);

            Assert.AreEqual(4.5d, await dictionary.AggregateAsync(Aggregator.IntegerAvg()));
            Assert.AreEqual(4.5d, await dictionary.AggregateAsync(Aggregator.IntegerAvg("this")));
        }

        [Test]
        public async Task Test_LongAvg()
        {
            var dictionary = await Client.GetDictionaryAsync<string, int>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await Fill(dictionary, i => i);

            Assert.AreEqual(4.5d, await dictionary.AggregateAsync(Aggregator.LongAvg()));
            Assert.AreEqual(4.5d, await dictionary.AggregateAsync(Aggregator.LongAvg("this")));
        }

        [Test]
        public async Task Test_NumberAvg()
        {
            // .NET does not have a 'number' type, so we have to use 'object'

            var dictionary = await Client.GetDictionaryAsync<string, object>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await Fill(dictionary, i => (float) i);
            await Fill(dictionary, i => (double) i);
            await Fill(dictionary, i => (int) i);
            await Fill(dictionary, i => (long) i);

            Assert.AreEqual(4.5d, await dictionary.AggregateAsync(Aggregator.NumberAvg()));
            Assert.AreEqual(4.5d, await dictionary.AggregateAsync(Aggregator.NumberAvg("this")));
        }

        [Test]
        public async Task Test_Max()
        {
            var dictionary = await Client.GetDictionaryAsync<string, int>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await Fill(dictionary, i => i);

            Assert.AreEqual(9, await dictionary.AggregateAsync(Aggregator.Max<int>()));
            Assert.AreEqual(9, await dictionary.AggregateAsync(Aggregator.Max<int>("this")));
        }

        [Test]
        public async Task Test_Min()
        {
            var dictionary = await Client.GetDictionaryAsync<string, int>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await Fill(dictionary, i => i);

            Assert.AreEqual(0, await dictionary.AggregateAsync(Aggregator.Min<int>()));
            Assert.AreEqual(0, await dictionary.AggregateAsync(Aggregator.Min<int>("this")));
        }

        [Test]
        public async Task Test_BigIntegerSum()
        {
            var dictionary = await Client.GetDictionaryAsync<string, BigInteger>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await Fill(dictionary, i => new BigInteger(i));

            Assert.AreEqual(new BigInteger(45), await dictionary.AggregateAsync(Aggregator.BigIntegerSum()));
            Assert.AreEqual(new BigInteger(45), await dictionary.AggregateAsync(Aggregator.BigIntegerSum("this")));
        }

        [Test]
        public async Task Test_DoubleSum()
        {
            var dictionary = await Client.GetDictionaryAsync<string, double>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await Fill(dictionary, i => i);

            Assert.AreEqual(45d, await dictionary.AggregateAsync(Aggregator.DoubleSum()));
            Assert.AreEqual(45d, await dictionary.AggregateAsync(Aggregator.DoubleSum("this")));
        }

        [Test]
        public async Task Test_IntegerSum()
        {
            var dictionary = await Client.GetDictionaryAsync<string, int>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await Fill(dictionary, i => i);

            Assert.AreEqual(45, await dictionary.AggregateAsync(Aggregator.IntegerSum()));
            Assert.AreEqual(45, await dictionary.AggregateAsync(Aggregator.IntegerSum("this")));
        }

        [Test]
        public async Task Test_LongSum()
        {
            var dictionary = await Client.GetDictionaryAsync<string, long>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await Fill(dictionary, i => i);

            Assert.AreEqual(45, await dictionary.AggregateAsync(Aggregator.LongSum()));
            Assert.AreEqual(45, await dictionary.AggregateAsync(Aggregator.LongSum("this")));
        }

        [Test]
        public async Task Test_FixedPointSum_MixedGeneric()
        {
            var name = CreateUniqueName();

            await using (var dictionary = await Client.GetDictionaryAsync<string, int>(name))
            {
                await Fill(dictionary, i => i);
            }

            await using (var dictionary = await Client.GetDictionaryAsync<string, long>(name))
            {
                await Fill(dictionary, i => i);
            }

            await using var d = await Client.GetDictionaryAsync<string, int>(name);

            Assert.AreEqual(90, await d.AggregateAsync(Aggregator.FixedPointSum()));
            Assert.AreEqual(90, await d.AggregateAsync(Aggregator.FixedPointSum("this")));
        }

        [Test]
        public async Task Test_FixedPointSum_MixedObject()
        {
            var name = CreateUniqueName();

            await using var dictionary = await Client.GetDictionaryAsync<string, object>(name);

            await Fill(dictionary, i => (int) i);
            await Fill(dictionary, i => (long) i);

            Assert.AreEqual(90, await dictionary.AggregateAsync(Aggregator.FixedPointSum()));
            Assert.AreEqual(90, await dictionary.AggregateAsync(Aggregator.FixedPointSum("this")));
        }

        [Test]
        public async Task Test_FloatingPointSum()
        {
            var dictionary = await Client.GetDictionaryAsync<string, float>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await Fill(dictionary, i => i);

            Assert.AreEqual(45d, await dictionary.AggregateAsync(Aggregator.FloatingPointSum()));
            Assert.AreEqual(45d, await dictionary.AggregateAsync(Aggregator.FloatingPointSum("this")));
        }

        [Test]
        public async Task Test_FloatingPointSum_MixedObject()
        {
            var dictionary = await Client.GetDictionaryAsync<string, object>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await Fill(dictionary, i => (float) i);
            await Fill(dictionary, i => (double) i);

            Assert.AreEqual(90d, await dictionary.AggregateAsync(Aggregator.FloatingPointSum()));
            Assert.AreEqual(90d, await dictionary.AggregateAsync(Aggregator.FloatingPointSum("this")));
        }

        [Test]
        public async Task Test_Count_with_Predicate()
        {
            var dictionary = await Client.GetDictionaryAsync<string, int>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await Fill(dictionary, i => i);

            var predicate = Predicate.IsGreaterThan("this", 5);

            Assert.AreEqual(4, await dictionary.AggregateAsync(Aggregator.Count(), predicate));
            Assert.AreEqual(4, await dictionary.AggregateAsync(Aggregator.Count("this"), predicate));
        }

        [Test]
        public async Task Test_LongSum_field()
        {
            var dictionary = await Client.GetDictionaryAsync<string, Header>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await Fill(dictionary, i => new Header(i, new Handle(false)));

            Assert.AreEqual(45, await dictionary.AggregateAsync(Aggregator.LongSum("id")));
        }

        [Test]
        public async Task Test_LongSum_withPredicate_field()
        {
            var dictionary = await Client.GetDictionaryAsync<string, Header>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await Fill(dictionary, i => new Header(i, new Handle(false)));

            var predicate = Predicate.IsGreaterThan("id", 5);
            Assert.AreEqual(30, await dictionary.AggregateAsync(Aggregator.LongSum("id"), predicate));
        }

        [Test]
        public async Task Test_AggregateAny()
        {
            var dictionary = await Client.GetDictionaryAsync<string, Item>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await Fill(dictionary, i => new Item(new Header(i, new Handle(false)), new int[1], new int[1]));

            Assert.AreEqual(10, await dictionary.AggregateAsync(Aggregator.Count("enabled[any]")));
        }

        [Test]
		public async Task TestAggregate_NullPredicate()
		{
            var dictionary = await Client.GetDictionaryAsync<string, int>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await AssertEx.ThrowsAsync<ArgumentException>(async () =>
            {
                await dictionary.AggregateAsync(Aggregator.LongSum("id"), null);
            });
		}

        //Projection tests
        [Test]
        public async Task Test_SingleAttributeProjection()
        {
            var dictionary = await Client.GetDictionaryAsync<string, Header>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await Fill(dictionary, i => new Header(i, new Handle(false)));

            var result = await dictionary.ProjectAsync<long>(new SingleAttributeProjection("id"));
            var expected = new HashSet<long> {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
            CollectionAssert.AreEquivalent(expected, result);
        }

        [Test]
        public async Task Test_SingleAttributeProjection_withPredicate()
        {
            var dictionary = await Client.GetDictionaryAsync<string, Header>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await Fill(dictionary, i => new Header(i, new Handle(false)));

            var predicate = Predicate.IsGreaterThan("id", 5);
            var result = await dictionary.ProjectAsync<long>(new SingleAttributeProjection("id"), predicate);
            var expected = new HashSet<long> {6, 7, 8, 9};
            CollectionAssert.AreEquivalent(expected, result);
        }

        [Test]
		public async Task Test_NullProjection()
		{
            var dictionary = await Client.GetDictionaryAsync<string, int>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await AssertEx.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await dictionary.ProjectAsync<long>(null);
            });
		}

        [Test]
		public async Task TestProjection_NullPredicate()
		{
            var dictionary = await Client.GetDictionaryAsync<string, int>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await AssertEx.ThrowsAsync<ArgumentException>(async () =>
            {
                await dictionary.ProjectAsync<long>(new SingleAttributeProjection("id"), null);
            });
		}

        [Test]
		public async Task Test_NullPredicateAndProjection()
		{
            var dictionary = await Client.GetDictionaryAsync<string, int>(CreateUniqueName());
            await using var _ = DestroyAndDispose(dictionary);

            await AssertEx.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await dictionary.ProjectAsync<long>(null, null);
            });
		}
    }
}