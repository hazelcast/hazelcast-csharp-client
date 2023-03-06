// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

using System.Linq;
using Hazelcast.DistributedObjects;
using Hazelcast.DistributedObjects.Impl;

namespace Hazelcast.Linq
{
    public static class HMapExtension
    {
        /// <summary>
        /// Extension of <see cref="IHMap"/> for LINQ functionalities. It provides IAsyncQueryable part of the <see cref="IHMap"/>.
        /// </summary>
        /// <param name="hMap"><see cref="IHMap"/> to be queried.</param>
        /// <typeparam name="TKey">Type of key of the <see cref="IHMap"/></typeparam>
        /// <typeparam name="TValue">Type of value of the <see cref="IHMap"/></typeparam>
        /// <returns></returns>
        // Async LINQ is provided as an extension package - so IHMap cannot directly expose async LINQ otherwise it should implement
        // the interface - so we have to go with an extension method to provide async-queryable.
        public static IAsyncQueryable<MapEntry<TKey, TValue>> AsAsyncQueryable<TKey, TValue>(this IHMap<TKey, TValue> hMap)
        {
            var mapInternal = (HMap<TKey, TValue>) hMap;
            return new QueryableMap<MapEntry<TKey, TValue>>(new QueryProvider(mapInternal.SqlService, typeof(MapEntry<TKey, TValue>), mapInternal.LoggerFactory), mapInternal.Name);
        }
    }
}