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
using System.Linq;
using Hazelcast.Client.Model;
using Hazelcast.Config;
using Hazelcast.Core;
using Hazelcast.Util;
using NUnit.Framework;

namespace Hazelcast.Client.Test
{
    [TestFixture]
    public class PagingPredicateTest : SingleMemberBaseTest
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

        private IMap<object, object> _map;

        protected override void ConfigureClient(ClientConfig config)
        {
            base.ConfigureClient(config);
            config.GetSerializationConfig()
                .AddDataSerializableFactory(IdentifiedFactory.FactoryId, new IdentifiedFactory());
        }

        private void FillMap()
        {
            for (var i = 0; i < 10; i++)
            {
                _map.Put("key-" + i, "value-" + i);
                _map.Put("keyx-" + i, "valuex-" + i);
            }
        }

        private void FillMap2()
        {
            for (var i = 65; i < 75; i++)
            {
                _map.Put("key-" + i,  new string(Convert.ToChar(i), i - 64));
            }
        }

        private static ISet<KeyValuePair<object, object>> GenerateKeyValuePair(int start, int end)
        {
            var kvSet = new HashSet<KeyValuePair<object, object>>();
            for (var i = start; i < end; i++)
            {
                kvSet.Add(new KeyValuePair<object, object>("key-" + i, "value-" + i));
            }
            return kvSet;
        }

        [Test]
        public void KeySetPaging_without_comparator()
        {
            FillMap();
            var predicate = new PagingPredicate(6, Predicates.Key().ILike("key-%"));
            var keySetPage1 = _map.KeySet(predicate);
            predicate.NextPage();
            var keySetPage2 = _map.KeySet(predicate);
            predicate.NextPage();
            var keySetPage3 = _map.KeySet(predicate);
            Assert.That(new[] {"key-0", "key-1", "key-2", "key-3", "key-4", "key-5"}, Is.EquivalentTo(keySetPage1.ToArray()));
            Assert.That(new[] {"key-6", "key-7", "key-8", "key-9"}, Is.EquivalentTo(keySetPage2.ToArray()));
            Assert.That(new string[] {}, Is.EquivalentTo(keySetPage3.ToArray()));
        }

        [Test]
        public void ValuesPaging_without_comparator()
        {
            FillMap();
            var predicate = new PagingPredicate(6, Predicates.IsILike("this", "value-%"));
            var valuesPage1 = _map.Values(predicate);
            predicate.NextPage();
            var valuesPage2 = _map.Values(predicate);
            predicate.NextPage();
            var valuesPage3 = _map.Values(predicate);
            Assert.That(new[] {"value-0", "value-1", "value-2", "value-3", "value-4", "value-5"}, Is.EquivalentTo(valuesPage1.ToArray()));
            Assert.That(new[] {"value-6", "value-7", "value-8", "value-9"}, Is.EquivalentTo(valuesPage2.ToArray()));
            Assert.That(new string[] {}, Is.EquivalentTo(valuesPage3.ToArray()));
        }

        [Test]
        public void EntrySetPaging_without_comparator()
        {
            FillMap();
            var predicate = new PagingPredicate(6, Predicates.IsILike("this", "value-%"));
            var page1 = _map.EntrySet(predicate);
            predicate.NextPage();
            var page2 = _map.EntrySet(predicate);
            predicate.NextPage();
            var page3 = _map.EntrySet(predicate);
            Assert.That(GenerateKeyValuePair(0, 6), Is.EquivalentTo(page1));
            Assert.That(GenerateKeyValuePair(6, 10), Is.EquivalentTo(page2));
            Assert.That(GenerateKeyValuePair(0, 0), Is.EquivalentTo(page3));
        }

        [Test]
        public void ValuesPaging_without_comparator_predicate()
        {
            FillMap();
            var predicate = new PagingPredicate(4);
            var valuesPage1 = _map.Values(predicate);
            Assert.That(new[] {"value-0", "value-1", "value-2", "value-3"}, Is.EquivalentTo(valuesPage1.ToArray()));
        }

        [Test]
        public void KeySetPaging_with_comparator()
        {
            FillMap();
            var predicate = new PagingPredicate(6, Predicates.Key().ILike("key-%"), new CustomComparator(1));

            var keySetPage1 = _map.KeySet(predicate);
            predicate.NextPage();
            var keySetPage2 = _map.KeySet(predicate);
            predicate.NextPage();
            var keySetPage3 = _map.KeySet(predicate);

            Assert.That(new[] {"key-9", "key-8", "key-7", "key-6", "key-5", "key-4"}, Is.EquivalentTo(keySetPage1.ToArray()));
            Assert.That(new[] {"key-3", "key-2", "key-1", "key-0"}, Is.EquivalentTo(keySetPage2.ToArray()));
            Assert.That(new string[] {}, Is.EquivalentTo(keySetPage3.ToArray()));
        }

        [Test]
        public void ValuePaging_with_comparator()
        {
            FillMap2();
            var predicate = new PagingPredicate(6, null, new CustomComparator(2, IterationType.Value));

            var page1 = _map.Values(predicate);
            predicate.NextPage();
            var page2 = _map.Values(predicate);
            predicate.NextPage();
            var page3 = _map.Values(predicate);
            Assert.That(new[] {"A", "BB", "CCC", "DDDD", "EEEEE", "FFFFFF"}, Is.EquivalentTo(page1.ToArray()));
            Assert.That(new[] {"GGGGGGG", "HHHHHHHH", "IIIIIIIII", "JJJJJJJJJJ"}, Is.EquivalentTo(page2.ToArray()));
            Assert.That(new string[] {}, Is.EquivalentTo(page3.ToArray()));
        }


        [Test]
        public void EntrySetPaging_with_comparator()
        {
            FillMap2();
            var predicate = new PagingPredicate(2, null, new CustomComparator(2, IterationType.Entry));

            var page1 = _map.EntrySet(predicate);
            Assert.That(new[] {new KeyValuePair<object, object>("key-65","A"),
                new KeyValuePair<object, object>("key-66","BB")}, Is.EquivalentTo(page1.ToArray()));
        }

        [Test]
        public void EntrySetCount()
        {
            var map = Client.GetMap<int, int>("testMap");
            for (var i = 0; i < 10; i++)
            {
                map.Put(i, 2* i);
            }
            map.EntrySet(new PagingPredicate(3, Predicates.IsGreaterThan("this", 5)));
        }
    }
}