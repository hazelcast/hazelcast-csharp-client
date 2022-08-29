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

namespace Hazelcast.Query
{
    /// <summary>
    /// Defines a paging predicate.
    /// </summary>
    public interface IPagingPredicate : IPredicate
    {
        /// <summary>
        /// Resets the predicate for re-use.
        /// </summary>
        void Reset();

        /// <summary>
        /// Moves to the next page.
        /// </summary>
        /// <remarks>
        /// <para>This is equivalent to incrementing <see cref="Page"/>.</para>
        /// </remarks>
        void NextPage();

        /// <summary>
        /// Moves to the previous page.
        /// </summary>
        /// <remarks>
        /// <para>This is equivalent to decrementing <see cref="Page"/>.</para>
        /// </remarks>
        void PreviousPage();

        /// <summary>
        /// Gets or sets the current page index
        /// </summary>
        int Page { get; set; }

        /// <summary>
        /// Gets the page size of each iteration.
        /// </summary>
        int PageSize { get; }

        // NOTE: comparer and anchor - keep internal for now (usage?)
        // in Java they are typed ie KeyValuePair<TKey, TValue>, here it is <object, object>
        // is this working as expected? is it OK not to be typed?

        /// <summary>
        /// Gets the comparer that is used to sort the results on the client.
        /// </summary>
        /// <remarks>
        /// <para>The comparer implementation should be serializable by Hazelcast,
        /// and should have a corresponding identical implementation on the server.</para>
        /// </remarks>
        //IComparer<KeyValuePair<object, object>> Comparer { get; }

        /// <summary>
        /// Gets the anchor entry, i.e. the last entry on the previous page.
        /// </summary>
        //KeyValuePair<object, object> Anchor { get; }
    }
}
