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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.Transactions;

namespace Hazelcast
{
    internal partial class HazelcastClient // Transactions
    {
        private int DefaultOperationTimeoutMilliseconds => _options.Messaging.DefaultOperationTimeoutMilliseconds;

        /// <inheritdoc />
        public Task<ITransactionContext> BeginTransactionAsync(TimeSpan timeout = default)
            => TaskEx.WithTimeout(BeginTransactionAsync, new TransactionOptions(), timeout, DefaultOperationTimeoutMilliseconds);

        /// <inheritdoc />
        public Task<ITransactionContext> BeginTransactionAsync(CancellationToken cancellationToken)
            => BeginTransactionAsync(new TransactionOptions(), cancellationToken);

        /// <inheritdoc />
        public Task<ITransactionContext> BeginTransactionAsync(TransactionOptions options, TimeSpan timeout = default)
            => TaskEx.WithTimeout(BeginTransactionAsync, options, timeout, DefaultOperationTimeoutMilliseconds);

        /// <inheritdoc />
        public async Task<ITransactionContext> BeginTransactionAsync(TransactionOptions options, CancellationToken cancellationToken)
        {
            options ??= new TransactionOptions();

            var transactionContext = new TransactionContext(Cluster, options, DefaultOperationTimeoutMilliseconds, SerializationService, _loggerFactory);
            await transactionContext.BeginAsync(cancellationToken).CAF();
            return transactionContext;
        }
    }
}