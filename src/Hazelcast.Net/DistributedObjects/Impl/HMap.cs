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

using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Serialization;
using Hazelcast.Sql;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.Impl
{
    /// <summary>
    /// Implements <see cref="IHMap{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    internal partial class HMap<TKey, TValue> : DistributedObjectBase, IHMap<TKey, TValue>
    {
        private readonly ISequence<long> _lockReferenceIdSequence;
        /// <summary>
        /// Sql Service for Linq Provider.
        /// </summary>
        internal ISqlService SqlService { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HMap{TKey,TValue}"/> class.
        /// </summary>
        /// <param name="name">The unique name of the object.</param>
        /// <param name="factory">The factory owning this object.</param>
        /// <param name="cluster">A cluster.</param>
        /// <param name="serializationService">A serialization service.</param>
        /// <param name="lockReferenceIdSequence">A lock reference identifiers sequence.</param>
        /// <param name="logggerFactory">A logger factory.</param>
        public HMap(string name, DistributedObjectFactory factory, Cluster cluster,
            SerializationService serializationService, ISequence<long> lockReferenceIdSequence,
            ILoggerFactory logggerFactory, ISqlService sqlService)
            : base(ServiceNames.Map, name, factory, cluster, serializationService, logggerFactory)
        {
            _lockReferenceIdSequence = lockReferenceIdSequence;
            SqlService = sqlService;
        }
    }
}
