// Copyright (c) 2008-2017, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Generic;
using System.Linq;
using Hazelcast.Client.Model;
using Hazelcast.Config;
using Hazelcast.Core;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class PagingPredicateWithPrimitiveTest : SingleMemberBaseTest
    {
        [SetUp]
        public void Init()
        {
            map = Client.GetMap<int, int>(TestSupport.RandomString());
        }

        [TearDown]
        public static void Destroy()
        {
            map.Clear();
        }

        protected override void ConfigureClient(ClientConfig config)
        {
            base.ConfigureClient(config);
            config.GetSerializationConfig()
                .AddDataSerializableFactory(IdentifiedFactory.FactoryId, new IdentifiedFactory());
        }

        internal static IMap<int, int> map;

        private void FillMap()
        {
            for (var i = 0; i < 10; i++)
            {
                map.Put(i, i);
            }
        }

        private ISet<KeyValuePair<int, int>> GenerateKeyValuePair(int start, int end)
        {
            var kvSet = new HashSet<KeyValuePair<int, int>>();
            for (var i = start; i < end; i++)
            {
                kvSet.Add(new KeyValuePair<int, int>(i, i));
            }
            return kvSet;
        }

        [Test]
        public virtual void TestKeySetPaging_without_comparator()
        {
            FillMap();
            var predicate = new PagingPredicate(6, Predicates.Key().GreaterThanOrEqual(0));
            var keySetPage1 = map.KeySet(predicate);
            predicate.NextPage();
            var keySetPage2 = map.KeySet(predicate);
            predicate.NextPage();
            var keySetPage3 = map.KeySet(predicate);
            Assert.That(new[] {0, 1, 2, 3, 4, 5}, Is.EquivalentTo(keySetPage1.ToArray()));
            Assert.That(new[] {6, 7, 8, 9}, Is.EquivalentTo(keySetPage2.ToArray()));
            Assert.That(new int[] {}, Is.EquivalentTo(keySetPage3.ToArray()));
        }

        [Test]
        public virtual void TestValuesPaging_without_comparator()
        {
            FillMap();
            var predicate = new PagingPredicate(6, Predicates.IsLessThan("this", 10));
            var valuesPage1 = map.Values(predicate);
            predicate.NextPage();
            var valuesPage2 = map.Values(predicate);
            predicate.NextPage();
            var valuesPage3 = map.Values(predicate);
            Assert.That(new[] {0, 1, 2, 3, 4, 5}, Is.EquivalentTo(valuesPage1.ToArray()));
            Assert.That(new[] {6, 7, 8, 9}, Is.EquivalentTo(valuesPage2.ToArray()));
            Assert.That(new int[] {}, Is.EquivalentTo(valuesPage3.ToArray()));
        }

        [Test]
        public virtual void TestEntrySetPaging_without_comparator()
        {
            FillMap();
            var predicate = new PagingPredicate(5, Predicates.IsLessThan("this", 9));
            var page1 = map.EntrySet(predicate);
            predicate.NextPage();
            var page2 = map.EntrySet(predicate);
            predicate.NextPage();
            var page3 = map.EntrySet(predicate);
            Assert.That(GenerateKeyValuePair(0, 5), Is.EquivalentTo(page1));
            Assert.That(GenerateKeyValuePair(5, 9), Is.EquivalentTo(page2));
            Assert.That(GenerateKeyValuePair(0, 0), Is.EquivalentTo(page3));
        }

        [Test]
        public virtual void TestValuesPaging_without_comparator_predicate()
        {
            FillMap();
            var predicate = new PagingPredicate(4);
            var valuesPage1 = map.Values(predicate);
            Assert.That(new[] {0, 1, 2, 3}, Is.EquivalentTo(valuesPage1.ToArray()));
        }

        [Test]
        public virtual void TestKeySetPaging_with_comparator()
        {
            FillMap();
            var predicate = new PagingPredicate(6, Predicates.Key().LessThanOrEqual(10), new CustomComparator(1));

            var keySetPage1 = map.KeySet(predicate);
            predicate.NextPage();
            var keySetPage2 = map.KeySet(predicate);
            predicate.NextPage();
            var keySetPage3 = map.KeySet(predicate);

            Assert.That(new[] {9, 8, 7, 6, 5, 4}, Is.EquivalentTo(keySetPage1.ToArray()));
            Assert.That(new[] {3, 2, 1, 0}, Is.EquivalentTo(keySetPage2.ToArray()));
            Assert.That(new int[] {}, Is.EquivalentTo(keySetPage3.ToArray()));

        }
    }
}