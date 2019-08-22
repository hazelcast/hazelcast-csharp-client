// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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

using Hazelcast.Core;

namespace Hazelcast.Transaction
{
    /// <summary>
    ///     Provides a context to do transactional operations; so beginning/committing transactions, but also retrieving
    ///     transactional data-structures like the
    ///     <see cref="ITransactionalMap{TKey,TValue}">Hazelcast.Core.ITransactionalMap&lt;K, V&gt;</see>
    ///     .
    /// </summary>
    public interface ITransactionContext : ITransactionalTaskContext
    {
        /// <summary>Begins a transaction.</summary>
        /// <remarks>Begins a transaction.</remarks>
        /// <exception cref="System.InvalidOperationException">if a transaction already is active.</exception>
        ITransaction BeginTransaction();
    }
}