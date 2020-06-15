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

using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.NearCaching;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.HMapImpl
{
    /// <summary>
    /// Implements a caching version of <see cref="IHMap{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    internal partial class HMapWithCache<TKey, TValue> : HMap<TKey, TValue>
    {
        private readonly NearCacheBase _cache;

        /// <summary>
        /// Initializes a new version of the <see cref="HMapWithCache{TKey,TValue}"/> class.
        /// </summary>
        /// <param name="name">The unique name of the object.</param>
        /// <param name="cluster">A cluster.</param>
        /// <param name="serializationService">A serialization service.</param>
        /// <param name="lockReferenceIdSequence">A lock reference identifiers sequence.</param>
        /// <param name="cache">A cache.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public HMapWithCache(string name, Cluster cluster, ISerializationService serializationService, ISequence<long> lockReferenceIdSequence, NearCacheBase cache, ILoggerFactory loggerFactory)
            : base(name, cluster, serializationService, lockReferenceIdSequence, loggerFactory)
        {
            _cache = cache;
        }

        // TODO: consider invalidating in a continuation?
        // TODO: not every methods are overriden, and then what?
        // TODO: OnInitialize, OnShutdown, PostDestroy and IDisposable?
        // TODO: refactor Map and CachedMap, so we don't need to serialize key to keyData all the time!
        //       generally, Map+NearCache can be greatly optimized, but we'll do that later

        // TODO: split regions
    }
}
