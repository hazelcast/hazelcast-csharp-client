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
            _map = Client.GetMap<int, int>(TestSupport.RandomString());
        }

        [TearDown]
        public void Destroy()
        {
            _map.Clear();
        }

        protected override void ConfigureClient(ClientConfig config)
        {
            base.ConfigureClient(config);
            config.GetSerializationConfig()
                .AddDataSerializableFactory(IdentifiedFactory.FactoryId, new IdentifiedFactory());
        }

        private IMap<int, int> _map;

        private void FillMap()
        {
            for (var i = 0; i < 10; i++)
            {
                _map.Put(i, i);
            }
        }

        private static ISet<KeyValuePair<int, int>> GenerateKeyValuePair(int start, int end)
        {
            var kvSet = new HashSet<KeyValuePair<int, int>>();
            for (var i = start; i < end; i++)
            {
                kvSet.Add(new KeyValuePair<int, int>(i, i));
            }
            return kvSet;
        }

        [Test]
        public void KeySetPaging_without_comparator()
        {
            FillMap();
            var predicate = new PagingPredicate(6, Predicates.Key().GreaterThanOrEqual(0));
            var keySetPage1 = _map.KeySet(predicate);
            predicate.NextPage();
            var keySetPage2 = _map.KeySet(predicate);
            predicate.NextPage();
            var keySetPage3 = _map.KeySet(predicate);
            Assert.That(new[] {0, 1, 2, 3, 4, 5}, Is.EquivalentTo(keySetPage1.ToArray()));
            Assert.That(new[] {6, 7, 8, 9}, Is.EquivalentTo(keySetPage2.ToArray()));
            Assert.That(new int[] {}, Is.EquivalentTo(keySetPage3.ToArray()));
        }

        [Test]
        public void ValuesPaging_without_comparator()
        {
            FillMap();
            var predicate = new PagingPredicate(6, Predicates.IsLessThan("this", 10));
            var valuesPage1 = _map.Values(predicate);
            predicate.NextPage();
            var valuesPage2 = _map.Values(predicate);
            predicate.NextPage();
            var valuesPage3 = _map.Values(predicate);
            Assert.That(new[] {0, 1, 2, 3, 4, 5}, Is.EquivalentTo(valuesPage1.ToArray()));
            Assert.That(new[] {6, 7, 8, 9}, Is.EquivalentTo(valuesPage2.ToArray()));
            Assert.That(new int[] {}, Is.EquivalentTo(valuesPage3.ToArray()));
        }

        [Test]
        public void EntrySetPaging_without_comparator()
        {
            FillMap();
            var predicate = new PagingPredicate(5, Predicates.IsLessThan("this", 9));
            var page1 = _map.EntrySet(predicate);
            predicate.NextPage();
            var page2 = _map.EntrySet(predicate);
            predicate.NextPage();
            var page3 = _map.EntrySet(predicate);
            Assert.That(GenerateKeyValuePair(0, 5), Is.EquivalentTo(page1));
            Assert.That(GenerateKeyValuePair(5, 9), Is.EquivalentTo(page2));
            Assert.That(GenerateKeyValuePair(0, 0), Is.EquivalentTo(page3));
        }

        [Test]
        public void ValuesPaging_without_comparator_predicate()
        {
            FillMap();
            var predicate = new PagingPredicate(4);
            var valuesPage1 = _map.Values(predicate);
            Assert.That(new[] {0, 1, 2, 3}, Is.EquivalentTo(valuesPage1.ToArray()));
        }

        [Test]
        public void KeySetPaging_with_comparator()
        {
            FillMap();
            var predicate = new PagingPredicate(6, Predicates.Key().LessThanOrEqual(10), new CustomComparator(1));

            var keySetPage1 = _map.KeySet(predicate);
            predicate.NextPage();
            var keySetPage2 = _map.KeySet(predicate);
            predicate.NextPage();
            var keySetPage3 = _map.KeySet(predicate);

            Assert.That(new[] {9, 8, 7, 6, 5, 4}, Is.EquivalentTo(keySetPage1.ToArray()));
            Assert.That(new[] {3, 2, 1, 0}, Is.EquivalentTo(keySetPage2.ToArray()));
            Assert.That(new int[] {}, Is.EquivalentTo(keySetPage3.ToArray()));
        }
    }
}