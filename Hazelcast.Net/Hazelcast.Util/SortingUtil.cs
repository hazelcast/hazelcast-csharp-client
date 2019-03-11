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
// See the License for the specific language governing permissions andasd206

// limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Core;

namespace Hazelcast.Util
{
    internal class SortingUtil
    {
        public static IEnumerable GetSortedQueryResultSet<TKey, TValue>(List<KeyValuePair<object, object>> list,
            PagingPredicate pagingPredicate, IterationType iterationType)
        {
            if (list.Count == 0)
            {
                return new List<KeyValuePair<TKey, TValue>>();
            }

            var comparator = NewComparer(pagingPredicate.Comparer, iterationType);
            list.Sort(comparator);

            var nearestAnchorEntry = pagingPredicate.GetNearestAnchorEntry();
            var nearestPage = nearestAnchorEntry.Key;
            var page = pagingPredicate.Page;
            var pageSize = pagingPredicate.PageSize;
            var begin = pageSize * (page - nearestPage - 1);
            var size = list.Count;

            if (begin > size)
            {
                return new List<KeyValuePair<TKey, TValue>>();
            }

            SetAnchor(list, pagingPredicate, nearestPage);

            var subList = list.GetRange(begin, Math.Min(pageSize, list.Count - begin));
            switch (iterationType)
            {
                case IterationType.Key:
                    return subList.Select(pair => pair.Key);
                case IterationType.Value:
                    return subList.Select(pair => pair.Value);
                case IterationType.Entry:
                    return subList.Select(pair => new KeyValuePair<TKey, TValue>((TKey) pair.Key, (TValue) pair.Value));
                default:
                    throw new ArgumentOutOfRangeException("iterationType", iterationType, null);
            }
        }

        private static IComparer<KeyValuePair<object, object>> NewComparer(IComparer<KeyValuePair<object, object>> 
            pagingPredicateComparer, IterationType iterationType)
        {
            return new ComparerImpl(pagingPredicateComparer, iterationType);
        }

        private static int Compare(IComparer<KeyValuePair<object, object>> comparer, IterationType iterationType,
            KeyValuePair<object, object> x, KeyValuePair<object, object> y)
        {
            int result;
            if (comparer != null)
            {
                result = comparer.Compare(x , y);
                return result != 0 ? result : CompareIntegers(x.Key.GetHashCode(), y.Key.GetHashCode());
            }

            switch (iterationType)
            {
                case IterationType.Key:
                    result = ((IComparable) x.Key).CompareTo(y.Key);
                    break;
                case IterationType.Value:
                    result = ((IComparable) x.Value).CompareTo(y.Value);
                    break;
                default:
                    // Possibly ENTRY
                    // Entries are not comparable, we cannot compare them
                    // So keys can be used instead of map entries.
                    result = ((IComparable) x.Key).CompareTo(y.Key);
                    break;
            }
            return result != 0 ? result : CompareIntegers(x.Key.GetHashCode(), y.Key.GetHashCode());
        }

        private static int CompareIntegers(int i1, int i2)
        {
            // i1 - i2 is not good way for comparison
            if (i1 > i2)
            {
                return +1;
            }
            if (i2 > i1)
            {
                return -1;
            }
            return 0;
        }

        private static void SetAnchor<TKey, TValue>(IList<KeyValuePair<TKey, TValue>> list,
            PagingPredicate pagingPredicate, int nearestPage)
        {
            if (list.Count == 0)
            {
                return;
            }
            var size = list.Count;
            var pageSize = pagingPredicate.PageSize;
            var page = pagingPredicate.Page;
            for (var i = pageSize; i <= size && nearestPage < page; i += pageSize)
            {
                var anchor = list[i - 1];
                nearestPage++;
                pagingPredicate.SetAnchor(nearestPage, anchor.Key, anchor.Value);
            }
        }

        internal class ComparerImpl : IComparer<KeyValuePair<object, object>>
        {
            private readonly IComparer<KeyValuePair<object, object>> _comparer;
            private readonly IterationType _iterationType;

            public ComparerImpl(IComparer<KeyValuePair<object, object>> comparer, IterationType iterationType)
            {
                _comparer = comparer;
                _iterationType = iterationType;
            }

            public int Compare(KeyValuePair<object, object> x, KeyValuePair<object, object> y)
            {
                return SortingUtil.Compare(_comparer, _iterationType, x, y);
            }
        }
    }
}