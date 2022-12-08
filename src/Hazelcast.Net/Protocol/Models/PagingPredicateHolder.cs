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
using Hazelcast.Query;
using Hazelcast.Serialization;

namespace Hazelcast.Protocol.Models
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

        internal ICollection<IData> PartitionKeysData { get; }

        public PagingPredicateHolder(AnchorDataListHolder anchorDataListHolder, IData predicateData, IData comparatorData,
            int pageSize, int page, byte iterationTypeId, IData partitionKeyData,
            bool partitionKeysDataExists, ICollection<IData> partitionKeysData)
        {
            AnchorDataListHolder = anchorDataListHolder;
            PredicateData = predicateData;
            ComparatorData = comparatorData;
            PageSize = pageSize;
            Page = page;
            IterationTypeId = iterationTypeId;
            PartitionKeyData = partitionKeyData;
            if (partitionKeysDataExists) PartitionKeysData = partitionKeysData;
        }

        public static PagingPredicateHolder Of(IPredicate predicate, SerializationService serializationService)
        {
            if (predicate is null)
                return null;

            if (predicate is PartitionPredicate partitionPredicate)
            {
                if (partitionPredicate.Target is PagingPredicate partitionPagingPredicate)
                {
                    var partitionKeyData = serializationService.ToData(partitionPredicate.PartitionKey);
                    return BuildHolder(serializationService, partitionPagingPredicate, partitionKeyData);
                }

                throw new InvalidOperationException("PartitionPredicate Target is not a PagingPredicate.");
            }

            if (predicate is PagingPredicate pagingPredicate)
            {
                return BuildHolder(serializationService, pagingPredicate, null);
            }

            throw new InvalidOperationException("Predicate is neither a PartitionPredicate nor a PagingPredicate.");
        }

        private static PagingPredicateHolder BuildHolder(SerializationService serializationService,
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

            if (!pagingPredicate.IterationType.HasValue)
                throw new InvalidOperationException("The paging predicate does not specify an iteration type.");

            return new PagingPredicateHolder(anchorDataListHolder, predicateData, comparatorData, pagingPredicate.PageSize,
                pagingPredicate.Page, (byte) pagingPredicate.IterationType, partitionKeyData, false, null);
        }
    }
}
