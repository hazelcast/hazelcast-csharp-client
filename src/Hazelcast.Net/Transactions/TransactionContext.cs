﻿// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.DistributedObjects.Impl;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Transactions
{
    internal class TransactionContext : ITransactionContext
    {
        private readonly Cluster _cluster;
        private readonly TransactionOptions _options;
        private readonly DistributedObjectFactory _distributedObjectFactory;
        private readonly object _inTransactionMutex = new object();

        private long _threadId; // the "threadId", i.e. async context, which owns the transaction
        //private long _startTime; // the start time of the transaction
        private bool _completed; // whether the transaction has been completed

#pragma warning disable CA2213 // Disposable fields should be disposed - well, no
        private MemberConnection _connection; // the client supporting the transaction
#pragma warning restore CA2213

        // TODO transactions need some TLC
        // how is two-phases commit supposed to work? is it all server-side (and then, why
        // do we have 'prepared' state on client's side?)
        // what about TransactionXxxExceptions, are we ever throwing them, and where?

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionContext"/> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="options">Transaction options.</param>
        /// <param name="serializationService">The serialization service.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public TransactionContext(Cluster cluster, TransactionOptions options, SerializationService serializationService, ILoggerFactory loggerFactory)
        {
            _cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            if (serializationService == null) throw new ArgumentNullException(nameof(serializationService));
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            _distributedObjectFactory = new DistributedObjectFactory(cluster, serializationService, loggerFactory);
        }

        /// <inheritdoc />
        public Guid TransactionId { get; private set; }

        /// <summary>
        /// Gets or sets the state of the transaction.
        /// </summary>
        public TransactionState State { get; private set; }

        /// <summary>
        /// Gets the current context identifier.
        /// </summary>
        /// <remarks>
        /// Hazelcast APIs call this the thread identified and maintain locks "per threads",
        /// so we are keeping the name here internally, but in reality this is not a thread
        /// identifier anymore - it is attached to the async context so it can flow with
        /// async operations.
        /// </remarks>
        private static long ContextId => AsyncContext.Current.Id;

        /// <summary>
        /// Begins the transaction.
        /// </summary>
        public async Task BeginAsync()
        {
            if (State != TransactionState.None)
                throw new InvalidOperationException("The transaction context is already involved in a transaction.");

            var asyncContext = AsyncContext.Current;
            lock (_inTransactionMutex)
            {
                // this is used to prevent an asynchronous context from being in multiple (thus nested) transactions,
                // it was implemented, before async, via a [ThreadStatic] boolean (when everything was bound to
                // threads) which cannot be appropriate anymore. instead we move it to the async context.

                if (asyncContext.InTransaction)
                    throw new InvalidOperationException("Nested transactions are not supported.");
                asyncContext.InTransaction = true;
            }

            _connection = _cluster.Members.GetRandomConnection();
            if (_connection == null) _cluster.State.ThrowClientOfflineException();

            _threadId = ContextId;
            //_startTime = Clock.Milliseconds;

            HConsole.WriteLine(this, $"Begin transaction on context #{ContextId}");

            try
            {
                // codec wants 0 for server config, -1 for infinite (?)
                var timeoutMs = _options.Timeout.RoundedMilliseconds(false);

                var requestMessage = TransactionCreateCodec.EncodeRequest(timeoutMs, _options.Durability, (int) _options.Type, ContextId);
                var responseMessage = await _cluster.Messaging.SendToMemberAsync(requestMessage, _connection).CfAwait();
                TransactionId = TransactionCreateCodec.DecodeResponse(responseMessage).Response;
                State = TransactionState.Active;
            }
            catch
            {
                lock (_inTransactionMutex) asyncContext.InTransaction = false;
                _threadId = 0;
                //_startTime = 0;
                TransactionId = default;
                State = TransactionState.None;
                throw;
            }
        }

        /// <inheritdoc />
        public async Task CommitAsync()
        {
            if (State != TransactionState.Active)
                throw new InvalidOperationException("There is no active transaction to commit.");

            if (_threadId != ContextId)
            {
                HConsole.WriteLine(this, $"Commit transaction on context #{ContextId} that was started on #{_threadId}");
                throw new InvalidOperationException("Transactions cannot span multiple async contexts.");
            }

            HConsole.WriteLine(this, $"Commit transaction on context #{ContextId}");

            try
            {
                var requestMessage = TransactionCommitCodec.EncodeRequest(TransactionId, ContextId);
                var responseMessage = await _cluster.Messaging.SendToMemberAsync(requestMessage, _connection).CfAwait();
                _ = TransactionCommitCodec.DecodeResponse(responseMessage);
                State = TransactionState.Committed;
            }
            catch
            {
                State = TransactionState.RollingBack;
                throw;
            }
            finally
            {
                lock (_inTransactionMutex) AsyncContext.Current.InTransaction = false;
            }
        }

        /// <inheritdoc />
        public async Task RollbackAsync()
        {
            if (State == TransactionState.RollingBack)
            {
                State = TransactionState.RolledBack;
                return;
            }

            if (State != TransactionState.Active)
                throw new InvalidOperationException("There is no active transaction to roll back.");

            if (_threadId != ContextId)
            {
                HConsole.WriteLine(this, $"Rollback transaction on context #{ContextId} that was started on #{_threadId}");
                throw new InvalidOperationException("Transactions cannot span multiple async contexts.");
            }

            HConsole.WriteLine(this, $"Rollback transaction on context #{ContextId}");

            try
            {
                var requestMessage = TransactionRollbackCodec.EncodeRequest(TransactionId, ContextId);
                var responseMessage = await _cluster.Messaging.SendToMemberAsync(requestMessage, _connection).CfAwait();
                _ = TransactionRollbackCodec.DecodeResponse(responseMessage);
                State = TransactionState.RolledBack;
            }
            catch
            {
                // TODO: that is the original code - weird
                State = TransactionState.RolledBack;
            }
            finally
            {
                lock (_inTransactionMutex) AsyncContext.Current.InTransaction = false;
            }
        }

        /// <inheritdoc />
        public void Complete()
        {
            _completed = true;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            try
            {
                await _distributedObjectFactory.DisposeAsync().CfAwait();
            }
            catch
            { /* ignore */ } // TODO: log?

            // if still in a transaction, either commit or rollback
            bool inTransaction;
            lock (_inTransactionMutex) inTransaction = AsyncContext.Current.InTransaction;
            if (inTransaction)
            {
                // may throw
                await (_completed ? CommitAsync() : RollbackAsync()).CfAwait();
            }
        }

        // Objects

        /// <inheritdoc />
        public Task<IHTxList<TItem>> GetListAsync<TItem>(string name)
        {
            return _distributedObjectFactory.GetOrCreateAsync<IHTxList<TItem>, HTxList<TItem>>(ServiceNames.List, name, true,
                (n, factory, cluster, serializationService, loggerFactory)
                    => new HTxList<TItem>(name, factory, cluster, _connection, TransactionId, serializationService, loggerFactory));
        }

        /// <inheritdoc />
        public Task<IHTxSet<TItem>> GetSetAsync<TItem>(string name)
        {
            return _distributedObjectFactory.GetOrCreateAsync<IHTxSet<TItem>, HTxSet<TItem>>(ServiceNames.Set, name, true,
                (n, factory, cluster, serializationService, loggerFactory)
                    => new HTxSet<TItem>(name, factory, cluster, _connection, TransactionId, serializationService, loggerFactory));
        }

        /// <inheritdoc />
        public Task<IHTxQueue<TItem>> GetQueueAsync<TItem>(string name)
        {
            return _distributedObjectFactory.GetOrCreateAsync<IHTxQueue<TItem>, HTxQueue<TItem>>(ServiceNames.Queue, name, true,
                (n, factory, cluster, serializationService, loggerFactory)
                    => new HTxQueue<TItem>(name, factory, cluster, _connection, TransactionId, serializationService, loggerFactory));
        }

        /// <inheritdoc />
        public Task<IHTxMultiMap<TKey, TValue>> GetMultiMapAsync<TKey, TValue>(string name)
        {
            return _distributedObjectFactory.GetOrCreateAsync<IHTxMultiMap<TKey, TValue>, HTxMultiMap<TKey, TValue>>(ServiceNames.MultiMap, name, true,
                (n, factory, cluster, serializationService, loggerFactory)
                    => new HTxMultiMap<TKey, TValue>(name, factory, cluster, _connection, TransactionId, serializationService, loggerFactory));
        }

        /// <inheritdoc />
        public Task<IHTxMap<TKey, TValue>> GetMapAsync<TKey, TValue>(string name)
        {
            return _distributedObjectFactory.GetOrCreateAsync<IHTxMap<TKey, TValue>, HTxMap<TKey, TValue>>(ServiceNames.Map, name, true,
                (n, factory, cluster, serializationService, loggerFactory)
                    => new HTxMap<TKey, TValue>(name, factory, cluster, _connection, TransactionId, serializationService, loggerFactory));
        }
    }
}
