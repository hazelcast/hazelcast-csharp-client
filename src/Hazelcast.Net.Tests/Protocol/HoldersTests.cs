// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Protocol.Models;
using Hazelcast.Query;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Hazelcast.Tests.Protocol
{
    [TestFixture]
    public class HoldersTests
    {
        [Test]
        public void AnchorDataListHolderTest()
        {
            var serializationService = new SerializationServiceBuilder(new NullLoggerFactory()).Build();

            var holder = GetAnchorDataListHolder(serializationService, out var pageList, out var dataList);

            Assert.That(holder.AnchorPageList, Is.SameAs(pageList));
            Assert.That(holder.AnchorDataList, Is.SameAs(dataList));

            var anchors = holder.AsAnchorIterator(serializationService).ToArray();

            Assert.That(anchors.Length, Is.EqualTo(4));
            for (var i = 0; i < 4; i++)
            {
                Assert.That(anchors[i].Key, Is.EqualTo(i));
                Assert.That(anchors[i].Value.Key, Is.EqualTo(i + 1));
                Assert.That(anchors[i].Value.Value, Is.EqualTo(i + 2));
            }
        }

        private AnchorDataListHolder GetAnchorDataListHolder(SerializationService serializationService, out List<int> pageList, out List<KeyValuePair<IData, IData>> dataList)
        {
            pageList = new List<int> { 0, 1, 2, 3 };
            dataList = new List<KeyValuePair<IData, IData>>
            {
                new KeyValuePair<IData, IData>(serializationService.ToData(1), serializationService.ToData(2)),
                new KeyValuePair<IData, IData>(serializationService.ToData(2), serializationService.ToData(3)),
                new KeyValuePair<IData, IData>(serializationService.ToData(3), serializationService.ToData(4)),
                new KeyValuePair<IData, IData>(serializationService.ToData(4), serializationService.ToData(5)),
            };

            return new AnchorDataListHolder(pageList, dataList);
        }

        [Test]
        public void PagingPredicateHolderTest()
        {
            var serializationService = new SerializationServiceBuilder(new NullLoggerFactory())
                .AddHook<PredicateDataSerializerHook>()
                .Build();

            var anchorDataListHolder = GetAnchorDataListHolder(serializationService, out var pageList, out var dataList);

            var predicateData = new HeapData();
            var comparatorData = new HeapData();
            var partitionKeyData = new HeapData();

            var holder = new PagingPredicateHolder(anchorDataListHolder, predicateData, comparatorData, 5, 12, 3, partitionKeyData);

            Assert.That(holder.AnchorDataListHolder, Is.SameAs(anchorDataListHolder));
            Assert.That(holder.PredicateData, Is.SameAs(predicateData));
            Assert.That(holder.ComparatorData, Is.SameAs(comparatorData));
            Assert.That(holder.PageSize, Is.EqualTo(5));
            Assert.That(holder.Page, Is.EqualTo(12));
            Assert.That(holder.IterationTypeId, Is.EqualTo(3));
            Assert.That(holder.PartitionKeyData, Is.SameAs(partitionKeyData));

            var predicate = new PagingPredicate(5)
            {
                IterationType = IterationType.Key
            };
            predicate.UpdateAnchors(new List<KeyValuePair<int, KeyValuePair<object, object>>>
            {
                new KeyValuePair<int, KeyValuePair<object, object>>(1, new KeyValuePair<object, object>("key", "value"))
            });

            holder = PagingPredicateHolder.Of(predicate, serializationService);
            Assert.That(holder.PageSize, Is.EqualTo(5));

            holder = PagingPredicateHolder.Of(new PartitionPredicate("key", new PagingPredicate(5) { IterationType = IterationType.Key }), serializationService);
            Assert.That(holder.PageSize, Is.EqualTo(5));

            Assert.That(PagingPredicateHolder.Of(null, serializationService), Is.Null);

            Assert.Throws<InvalidOperationException>(() => _ = PagingPredicateHolder.Of(new PagingPredicate(5), serializationService));

            Assert.Throws<InvalidOperationException>(() => _ = PagingPredicateHolder.Of(new AndPredicate(), serializationService));
            Assert.Throws<InvalidOperationException>(() => _ = PagingPredicateHolder.Of(new PartitionPredicate(), serializationService));
        }
    }
}
