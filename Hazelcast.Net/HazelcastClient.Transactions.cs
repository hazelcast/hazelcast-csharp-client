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

using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.Transactions;

namespace Hazelcast
{
    internal partial class HazelcastClient // Transactions
    {
        // fixme timeout, cancellation token, options, whatnot
        // fixme cannot have default transaction options coming from the options file?!

        public async Task<ITransactionContext> BeginTransactionAsync(TransactionOptions options = null)
        {
            var transactionContext = new TransactionContext(Cluster, options ?? new TransactionOptions(), SerializationService, _loggerFactory);
            await transactionContext.ConnectAsync().CAF();
            await transactionContext.BeginAsync().CAF();
            return transactionContext;
        }
    }
}