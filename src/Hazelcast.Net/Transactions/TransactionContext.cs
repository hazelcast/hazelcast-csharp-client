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

        private long _threadId; // the "threadId", i.e. async context, which owns the transaction
        private long _startTime; // the start time of the transaction
        private MemberConnection _connection; // the client supporting the transaction

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
        public TransactionContext(Cluster cluster, TransactionOptions options, ISerializationService serializationService, ILoggerFactory loggerFactory)
        {
            _cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            if (serializationService == null) throw new ArgumentNullException(nameof(serializationService));
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            _distributedObjectFactory = new DistributedObjectFactory(cluster, serializationService, loggerFactory);
        }

        /// <inheritdoc />
        public Guid TransactionId { get; private set; }

        /// <inheritdoc />
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
        private static long ContextId => AsyncContext.CurrentContext.Id;

        /// <summary>
        /// Gets or sets a value indicating whether the current asynchronous context is in a transaction.
        /// </summary>
        private static bool InTransaction
        {
            // this is used to prevent an asynchronous context from being in multiple (thus nested) transactions,
            // it was implemented, before async, via a [ThreadStatic] boolean (when everything was bound to
            // threads) which cannot be appropriate anymore.
            // therefore an InTransaction property has been added to the async context - this may be considered
            // a confusion of concerns, and maybe a separate AsyncLocal would be more 'pure', but this is
            // simple enough.

            get => AsyncContext.CurrentContext.InTransaction;
            set => AsyncContext.CurrentContext.InTransaction = value;
        }

        /// <summary>
        /// Begins the transaction.
        /// </summary>
        public async Task BeginAsync()
        {
            if (State != TransactionState.None)
                throw new InvalidOperationException("The transaction context is already involved in a transaction.");

            if (InTransaction)
                throw new InvalidOperationException("Nested transactions are not supported.");

            // TODO: think about race conditions?
            _connection = await _cluster.Members.WaitRandomConnection().CAF();
            InTransaction = true;

            _threadId = ContextId;
            _startTime = Clock.Milliseconds;

            HConsole.WriteLine(this, $"Begin transaction on context #{ContextId}");

            try
            {
                var timeoutMilliseconds = _options.Timeout.TimeoutMilliseconds(0, int.MaxValue);
                var requestMessage = TransactionCreateCodec.EncodeRequest(timeoutMilliseconds, _options.Durability, (int) _options.Type, ContextId);
                var responseMessage = await _cluster.Messaging.SendToMemberAsync(requestMessage, _connection).CAF();
                TransactionId = TransactionCreateCodec.DecodeResponse(responseMessage).Response;
                State = TransactionState.Active;
            }
            catch
            {
                InTransaction = false;
                _threadId = 0;
                _startTime = 0;
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

            var timeoutMilliseconds = _options.Timeout.TimeoutMilliseconds(0, int.MaxValue);
            if (_startTime + timeoutMilliseconds < Clock.Milliseconds)
                throw new TransactionException("Transaction has timed out.");

            HConsole.WriteLine(this, $"Commit transaction on context #{ContextId}");

            try
            {
                var requestMessage = TransactionCommitCodec.EncodeRequest(TransactionId, ContextId);
                var responseMessage = await _cluster.Messaging.SendToMemberAsync(requestMessage, _connection).CAF();
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
                InTransaction = false;
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
                var responseMessage = await _cluster.Messaging.SendToMemberAsync(requestMessage, _connection).CAF();
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
                InTransaction = false;
            }
        }

        public async ValueTask DisposeAsync()
        {
            // be sure to always clear this!
            InTransaction = false;

            try
            {
                await _distributedObjectFactory.DisposeAsync().CAF();
            }
            catch
            { /* ignore */ } // TODO: log?

            if (State == TransactionState.Active)
            {
                // abandoned transaction, roll it back

                try
                {
                    await RollbackAsync().CAF();
                }
                catch
                { /* ignore */ } // TODO: log?
            }
        }

        // Objects

        /// <inheritdoc />
        public Task<IHTxList<TItem>> GetTransactionalAsync<TItem>(IHList<TItem> source)
            => GetListAsync<TItem>(source.Name);

        /// <inheritdoc />
        public Task<IHTxList<TItem>> GetListAsync<TItem>(string name)
        {
            return _distributedObjectFactory.GetOrCreateAsync<IHTxList<TItem>, HTxList<TItem>>(HList.ServiceName, name, true,
                (n, factory, cluster, serializationService, loggerFactory)
                    => new HTxList<TItem>(name, factory, cluster, _connection, TransactionId, serializationService, loggerFactory));
        }

        /// <inheritdoc />
        public Task<IHTxSet<TItem>> GetTransactionalAsync<TItem>(IHSet<TItem> source)
            => GetSetAsync<TItem>(source.Name);

        /// <inheritdoc />
        public Task<IHTxSet<TItem>> GetSetAsync<TItem>(string name)
        {
            return _distributedObjectFactory.GetOrCreateAsync<IHTxSet<TItem>, HTxSet<TItem>>(HSet.ServiceName, name, true,
                (n, factory, cluster, serializationService, loggerFactory)
                    => new HTxSet<TItem>(name, factory, cluster, _connection, TransactionId, serializationService, loggerFactory));
        }

        public Task<IHTxQueue<TItem>> GetTransactionalAsync<TItem>(IHQueue<TItem> source)
            => GetQueueAsync<TItem>(source.Name);

        public Task<IHTxQueue<TItem>> GetQueueAsync<TItem>(string name)
        {
            return _distributedObjectFactory.GetOrCreateAsync<IHTxQueue<TItem>, HTxQueue<TItem>>(HQueue.ServiceName, name, true,
                (n, factory, cluster, serializationService, loggerFactory)
                    => new HTxQueue<TItem>(name, factory, cluster, _connection, TransactionId, serializationService, loggerFactory));
        }

        public Task<IHTxMultiDictionary<TKey, TValue>> GetTransactionalAsync<TKey, TValue>(IHMultiDictionary<TKey, TValue> source)
            => GetMultiDictionaryAsync<TKey, TValue>(source.Name);

        public Task<IHTxMultiDictionary<TKey, TValue>> GetMultiDictionaryAsync<TKey, TValue>(string name)
        {
            return _distributedObjectFactory.GetOrCreateAsync<IHTxMultiDictionary<TKey, TValue>, HTxMultiDictionary<TKey, TValue>>(HMultiDictionary.ServiceName, name, true,
                (n, factory, cluster, serializationService, loggerFactory)
                    => new HTxMultiDictionary<TKey, TValue>(name, factory, cluster, _connection, TransactionId, serializationService, loggerFactory));
        }

        public Task<IHTxDictionary<TKey, TValue>> GetTransactionalAsync<TKey, TValue>(IHDictionary<TKey, TValue> source)
            => GetDictionaryAsync<TKey, TValue>(source.Name);

        public Task<IHTxDictionary<TKey, TValue>> GetDictionaryAsync<TKey, TValue>(string name)
        {
            return _distributedObjectFactory.GetOrCreateAsync<IHTxDictionary<TKey, TValue>, HTxDictionary<TKey, TValue>>(HDictionary.ServiceName, name, true,
                (n, factory, cluster, serializationService, loggerFactory)
                    => new HTxDictionary<TKey, TValue>(name, factory, cluster, _connection, TransactionId, serializationService, loggerFactory));
        }
    }
}
