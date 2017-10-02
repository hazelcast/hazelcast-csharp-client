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

using System;
using System.Collections;
using System.Collections.Generic;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Util;

namespace Hazelcast.Core
{
    /// <summary>
    /// This class is a special Predicate which helps to get a page-by-page result of a query.
    /// It can be constructed with a page-size, an inner predicate for filtering, and a comparator for sorting.
    /// </summary>
    /// <remarks>
    /// This class is not thread-safe and stateless. To be able to reuse for another query, one should call
    /// <see cref="PagingPredicate.Reset()"/>
    /// </remarks>
    /// <example>
    /// <code>
    /// Predicate lessEqualThanFour = Predicates.IsLessThanOrEqual("this", 4);
    /// // We are constructing our paging predicate with a predicate and page size. In this case query results fetched two by two.
    /// PagingPredicate predicate = new PagingPredicate(lessEqualThanFour, 2);
    /// // we are initializing our map with integers from 0 to 10 as keys and values.
    /// IMap map = hazelcastInstance.getMap("myMap");
    /// for (int i = 0; i &lt; 10; i++)
    /// {
    ///     map.Put(i, i);
    /// }
    ///
    /// //invoking the query
    /// var values = map.Values(predicate);
    /// Console.WriteLine("values = " + values); // will print 'values = [0, 1]'
    /// predicate.NextPage(); // we are setting up paging predicate to fetch next page in the next call.
    /// values = map.Values(predicate);
    /// Console.WriteLine("values = " + values);// will print 'values = [2, 3]'
    /// var anchor = predicate.GetAnchor();
    /// Console.WriteLine("anchor -> " + anchor); // will print 'anchor -> 1=1',  since the anchor is the last entry of the previous page.
    /// predicate.previousPage(); // we are setting up paging predicate to fetch previous page in the next call
    /// values = map.Values(predicate);
    /// Console.WriteLine("values = " + values) // will print 'values = [0, 1]'
    /// </code>
    /// </example>
    public class PagingPredicate : IPredicate
    {
        private static readonly KeyValuePair<int, KeyValuePair<object, object>> NullAnchor = new KeyValuePair<int, KeyValuePair<object, object>>(-1, new KeyValuePair<object, object>(null, null));

        private IList<KeyValuePair<int, KeyValuePair<object, object>>> anchorList;
        private readonly IPredicate predicate;
        private IComparer<KeyValuePair<object, object>> comparer;
        private int pageSize;
        private int page;

        private IterationType? iterationType;

        public PagingPredicate()
        {
        }
        
        /// <summary>
        /// Creates a Paging predicate with provided page size and optional predicate and comparer.
        /// </summary>
        /// <param name="pageSize">page size of each result set</param>
        /// <param name="predicate">Optional predicate to filter of the results. if null, no filtering applied</param>
        /// <param name="comparer">Optional <see cref="IComparer"/> implementation used to sort the results. see warning at <see cref="Comparer"/></param>
        /// <exception cref="ArgumentOutOfRangeException">if page size is negative</exception>
        /// <exception cref="ArgumentException">Nested PagingPredicate is not supported</exception>
        public PagingPredicate(int pageSize, IPredicate predicate = null, IComparer<KeyValuePair<object, object>> comparer = null)
        {
            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException("pageSize", "pageSize should be greater than 0 !!!");
            }
            this.pageSize = pageSize;
            anchorList = new List<KeyValuePair<int, KeyValuePair<object, object>>>();
            if (predicate is PagingPredicate)
            {
                throw new ArgumentException("Nested PagingPredicate is not supported!!!", "predicate");
            }
            this.predicate = predicate;
            this.comparer = comparer;
        }

        /// <summary>
        /// <c>IComparer&lt;KeyValuePair&lt;object, object&gt;&gt;</c>> implementation used to sort the result on client side.
        /// </summary>
        /// <remarks>
        /// <b>WARNING:</b> This comparer implementation should be hazelcast serializable and must have the same
        /// implementation on server side.
        /// </remarks>
        public IComparer<KeyValuePair<object, object>> Comparer
        {
            get { return comparer; }
            set { comparer = value; }
        }

        /// <summary>
        /// Current page index
        /// </summary>
        public int Page
        {
            get { return page; }
            set { page = value; }
        }

        /// <summary>
        /// Page size of each iteration
        /// </summary>
        public int PageSize
        {
            get { return pageSize; }
            set { pageSize = value; }
        }

        /// <summary>
        /// Iteration type this paging predicate: One of Key, Value or Entry
        /// </summary>
        public IterationType? IterationType
        {
            get { return iterationType; }
            set { iterationType = value; }
        }

        public void ReadData(IObjectDataInput input)
        {
            throw new NotSupportedException("Client should not need to use ReadData method.");
        }

        public void WriteData(IObjectDataOutput output)
        {
            output.WriteObject(predicate);
            output.WriteObject(comparer);
            output.WriteInt(page);
            output.WriteInt(pageSize);
            output.WriteUTF(iterationType.ToString().ToUpper());
            output.WriteInt(anchorList.Count);
            foreach (var anchor in anchorList)
            {
                output.WriteInt(anchor.Key);
                var anchorEntry = anchor.Value;
                output.WriteObject(anchorEntry.Key);
                output.WriteObject(anchorEntry.Value);
            }
        }

        public int GetFactoryId()
        {
            return FactoryIds.PredicateFactoryId;
        }

        public int GetId()
        {
            return PredicateDataSerializerHook.PagingPredicate;
        }

        internal void SetAnchor<TKey, TValue>(int nearestPage, TKey key, TValue value)
        {
            var anchorEntry =
                new KeyValuePair<int, KeyValuePair<object, object>>(page, new KeyValuePair<object, object>(key, value));
            int anchorCount = anchorList.Count;
            if (page < anchorCount)
            {
                anchorList[page] = anchorEntry;
            }
            else if (page == anchorCount)
            {
                anchorList.Add(anchorEntry);
            }
            else
            {
                throw new ArgumentException("Anchor index is not correct, expected: " + page + " found: " +
                                            anchorCount);
            }
        }

        /// <summary>
        /// After each query, an anchor entry is set for that page. <see cref="SetAnchor{TKey,TValue}"/>
        /// For the next query user may set an arbitrary <see cref="Page"/>.
        /// for example: user queried first 5 pages which means first 5 anchor is available
        /// if the next query is for the 10th page then the nearest anchor belongs to page 5
        /// but if the next query is for the 3nd page then the nearest anchor belongs to page 2
        /// </summary>
        /// <returns>nearest anchored entry for current page</returns>
        public KeyValuePair<int, KeyValuePair<object, object>> GetNearestAnchorEntry() {
            int anchorCount = anchorList.Count;
            if (page == 0 || anchorCount == 0) {
                return NullAnchor;
            }

            return page < anchorCount ? anchorList[page - 1] : anchorList[anchorCount - 1];
        }

        /// <summary>
        /// resets for reuse
        /// </summary>
        public void Reset()
        {
            iterationType = null;
            anchorList.Clear();
            page = 0;
        }

        /// <summary>
        /// sets the page value to next page
        /// </summary>
        public void NextPage()
        {
            page++;
        }

        /// <summary>
        /// sets the page value to previous page
        /// </summary>
        public void PreviousPage()
        {
            if (page != 0) {
                page--;
            }
        }
    }
}