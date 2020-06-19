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

using System;
using Hazelcast.Clustering;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Defines the base contract for all transactional Hazelcast distributed objects.
    /// </summary>
    public interface ITransactionalDistributedObject : IDistributedObject
    {
        /// <summary>
        /// Gets the unique identifier of the transaction.
        /// </summary>
        Guid TransactionId { get; }

        /// <summary>
        /// Gets the client supporting the transaction.
        /// </summary>
        Client TransactionClient { get; }
    }
}
