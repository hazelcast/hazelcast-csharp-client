﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.Implementation.Map
{
    /// <summary>
    /// Implements <see cref="IMap{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    internal partial class Map<TKey, TValue> : DistributedObjectBase, IMap<TKey, TValue>
    {
        private readonly ISequence<long> _lockReferenceIdSequence;

        /// <summary>
        /// Initializes a new instance of the <see cref="Map{TKey,TValue}"/> class.
        /// </summary>
        /// <param name="name">The unique name of the object.</param>
        /// <param name="cluster">A cluster.</param>
        /// <param name="serializationService">A serialization service.</param>
        /// <param name="lockReferenceIdSequence">A lock reference identifiers sequence.</param>
        /// <param name="logggerFactory">A logger factory.</param>
        public Map(string name, Cluster cluster, ISerializationService serializationService, ISequence<long> lockReferenceIdSequence, ILoggerFactory logggerFactory)
            : base(Map.ServiceName, name, cluster, serializationService, logggerFactory)
        {
            _lockReferenceIdSequence = lockReferenceIdSequence;
        }
    }
}