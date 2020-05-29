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

using System.Collections.Generic;
using Hazelcast.Predicates;
using Hazelcast.Serialization;

namespace Hazelcast.Protocol.Data
{
    internal class PagingPredicateHolder
    {
        internal AnchorDataListHolder AnchorDataListHolder { get; }

        internal IData PredicateData { get; }

        internal IData ComparatorData { get; }

        internal int PageSize { get; }

        internal int Page { get; }

        internal byte IterationTypeId { get; }

        internal IData PartitionKeyData { get; }

        public PagingPredicateHolder(AnchorDataListHolder anchorDataListHolder, IData predicateData, IData comparatorData,
            int pageSize, int page, byte iterationTypeId, IData partitionKeyData)
        {
            AnchorDataListHolder = anchorDataListHolder;
            PredicateData = predicateData;
            ComparatorData = comparatorData;
            PageSize = pageSize;
            Page = page;
            IterationTypeId = iterationTypeId;
            PartitionKeyData = partitionKeyData;
        }

        public static PagingPredicateHolder Of(IPredicate predicate, ISerializationService serializationService)
        {
            if (predicate is PartitionPredicate partitionPredicate)
            {
                return OfInternal(partitionPredicate, serializationService);
            }
            return OfInternal((PagingPredicate)predicate, serializationService);
        }

        private static PagingPredicateHolder OfInternal(PagingPredicate pagingPredicate,
            ISerializationService serializationService)
        {
            if (pagingPredicate == null)
            {
                return null;
            }
            return BuildHolder(serializationService, pagingPredicate, null);
        }

        private static PagingPredicateHolder OfInternal(PartitionPredicate partitionPredicate,
            ISerializationService serializationService)
        {
            if (partitionPredicate == null)
            {
                return null;
            }

            var pagingPredicate = (PagingPredicate)partitionPredicate.GetTarget();

            var partitionKeyData = serializationService.ToData(partitionPredicate.GetPartitionKey());

            return BuildHolder(serializationService, pagingPredicate, partitionKeyData);
        }

        private static PagingPredicateHolder BuildHolder(ISerializationService serializationService,
            PagingPredicate pagingPredicate, IData partitionKeyData)
        {
            var anchorList = pagingPredicate.AnchorList;
            var anchorDataList = new List<KeyValuePair<IData, IData>>(anchorList.Count);
            var pageList = new List<int>(anchorList.Count);
            foreach (var pair in anchorList)
            {
                pageList.Add(pair.Key);
                var anchorEntry = pair.Value;
                anchorDataList.Add(new KeyValuePair<IData, IData>(serializationService.ToData(anchorEntry.Key),
                    serializationService.ToData(anchorEntry.Value)));
            }
            var anchorDataListHolder = new AnchorDataListHolder(pageList, anchorDataList);
            var predicateData = serializationService.ToData(pagingPredicate.Predicate);
            var comparatorData = serializationService.ToData(pagingPredicate.Comparer);
            return new PagingPredicateHolder(anchorDataListHolder, predicateData, comparatorData, pagingPredicate.PageSize,
                pagingPredicate.Page, (byte)pagingPredicate.IterationType, partitionKeyData);
        }
    }
}
