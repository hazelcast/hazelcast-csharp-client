// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.DistributedObjects;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.CP
{
    /// <summary>
    /// Provides the <see cref="IAtomicLong"/> implementation.
    /// </summary>
    internal class AtomicLong : DistributedObjectBase, IAtomicLong
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AtomicLong"/> class.
        /// </summary>
        /// <param name="name">The unique name.</param>
        /// <param name="factory">The distributed objects factory.</param>
        /// <param name="cluster">The cluster.</param>
        /// <param name="serializationService">The serialization service.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public AtomicLong(string name, DistributedObjectFactory factory, Cluster cluster, SerializationService serializationService, ILoggerFactory loggerFactory)
            : base(ServiceNames.AtomicLong, name, factory, cluster, serializationService, loggerFactory)
        { }

        // java:
        //public AtomicLongProxy(NodeEngine nodeEngine, RaftGroupId groupId, String proxyName, String objectName)
        //{
        //    RaftService service = nodeEngine.getService(RaftService.SERVICE_NAME);
        //    this.invocationManager = service.getInvocationManager();
        //    this.groupId = groupId;
        //    this.proxyName = proxyName;
        //    this.objectName = objectName;
        //}

        // need:
        // RaftGroupId
        //  CPGroupId
        // RaftService
        //  NodeEngine
        // AddAndGetOp, CompareAndSetOp, GetAndAddOp, GetAndSetOp
        //  AbstractAtomicLongOp
        //   RaftOp
        //  AtomicLongService... tons of things - which are server, which are client?!
        // NodeEngine
        //
        // => FIXME: determine the dependencies, evaluate the implementation cost
        // also: revisit NDepend for cycles?

        /// <inheritdoc />
        public Task<long> AddAndGetAsync(long value)
        {
            // java:
            //RaftOp op = new AddAndGetOp(objectName, delta);
            //return delta == 0 ? invocationManager.query(groupId, op, LINEARIZABLE) : invocationManager.invoke(groupId, op);
            throw new NotImplementedException(); // FIXME not implemented
        }

        /// <inheritdoc />
        public Task<bool> CompareAndSetAsync(long comparand, long value)
        {
            // java:
            //return invocationManager.invoke(groupId, new CompareAndSetOp(objectName, expect, update));
            throw new NotImplementedException(); // FIXME not implemented
        }

        /// <inheritdoc />
        public Task<long> GetAndAddAsync(long value)
        {
            // java:
            //RaftOp op = new GetAndAddOp(objectName, delta);
            //return delta == 0 ? invocationManager.query(groupId, op, LINEARIZABLE) : invocationManager.invoke(groupId, op);
            throw new NotImplementedException(); // FIXME not implemented
        }

        /// <inheritdoc />
        public Task<long> GetAndSetAsync(long value)
        {
            // java:
            //return invocationManager.invoke(groupId, new GetAndSetOp(objectName, newValue));
            throw new NotImplementedException(); // FIXME not implemented
        }

        /// <inheritdoc />
        public Task<long> DecrementAndGetAsync() => AddAndGetAsync(-1);

        /// <inheritdoc />
        public Task<long> GetAndDecrementAsync() => GetAndAddAsync(-1);

        /// <inheritdoc />
        public Task<long> GetAsync() => GetAndAddAsync(0);

        /// <inheritdoc />
        public Task<long> IncrementAndGetAsync() => AddAndGetAsync(+1);

        /// <inheritdoc />
        public Task<long> GetAndIncrementAsync() => GetAndAddAsync(+1);

        /// <inheritdoc />
        public Task SetAsync(long value) => GetAndSetAsync(value);
    }
}