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
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects
{
    /// <summary>
    /// Provides a base class to transactional distributed objects.
    /// </summary>
    internal abstract class TransactionalDistributedObjectBase : DistributedObjectBase, ITransactionalDistributedObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionalDistributedObjectBase"/> class.
        /// </summary>
        /// <param name="serviceName">the name of the service managing this object.</param>
        /// <param name="name">The unique name of the object.</param>
        /// <param name="cluster">A cluster.</param>
        /// <param name="transactionClientConnection">The client connection supporting the transaction.</param>
        /// <param name="transactionId">The unique identifier of the transaction.</param>
        /// <param name="serializationService">A serialization service.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        protected TransactionalDistributedObjectBase(string serviceName, string name, Cluster cluster, ClientConnection transactionClientConnection, Guid transactionId, ISerializationService serializationService, ILoggerFactory loggerFactory)
            : base(serviceName, name, cluster, serializationService, loggerFactory)
        {
            TransactionId = transactionId;
            TransactionClientConnection = transactionClientConnection;
        }

        /// <inheritdoc />
        public Guid TransactionId { get; }

        /// <summary>
        /// Gets the client connection supporting the transaction.
        /// </summary>
        public ClientConnection TransactionClientConnection { get; }
    }
}
