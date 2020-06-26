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
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.DistributedObjects;
using Hazelcast.DistributedObjects.HTxListImpl;
using Hazelcast.DistributedObjects.HTxMapImpl;
using Hazelcast.DistributedObjects.HTxMultiMapImpl;
using Hazelcast.DistributedObjects.HTxQueueImpl;
using Hazelcast.DistributedObjects.HTxSetImpl;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Transactions
{
    internal class TransactionContext : ITransactionContext
    {
        private readonly Cluster _cluster;
        private readonly TransactionOptions _options;
        private readonly int _defaultOperationTimeoutMilliseconds;
        private readonly DistributedObjectFactory _distributedObjectFactory;

        private long _threadId; // the "threadId", i.e. async context, which owns the transaction
        private long _startTime; // the start time of the transaction
        private ClientConnection _client; // the client supporting the transaction

        // FIXME transactions need some TLC
        // how is two-phases commit supposed to work? is it all server-side (and then, why
        // do we have 'prepared' state on client's side?)
        // what about TransactionXxxExceptions, are we ever throwing them, and where?

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionContext"/> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="options">Transaction options.</param>
        /// <param name="defaultOperationTimeoutMilliseconds">The default operation timeout.</param>
        /// <param name="serializationService">The serialization service.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public TransactionContext(Cluster cluster, TransactionOptions options, int defaultOperationTimeoutMilliseconds, ISerializationService serializationService, ILoggerFactory loggerFactory)
        {
            _cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _defaultOperationTimeoutMilliseconds = defaultOperationTimeoutMilliseconds;

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
        /// Begins a the transaction.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        public async Task BeginAsync(CancellationToken cancellationToken)
        {
            if (State != TransactionState.None)
                throw new InvalidOperationException("The transaction context is already involved in a transaction.");

            if (InTransaction)
                throw new InvalidOperationException("Nested transactions are not supported.");

            _client = await _cluster.GetRandomClient(cancellationToken).CAF();
            InTransaction = true;
            _threadId = ContextId;
            _startTime = Clock.Milliseconds;

            try
            {
                var timeoutMilliseconds = _options.Timeout.TimeoutMilliseconds(0, int.MaxValue);
                var requestMessage = TransactionCreateCodec.EncodeRequest(timeoutMilliseconds, _options.Durability, (int) _options.Type, ContextId);
                var responseMessage = await _cluster.SendToClientAsync(requestMessage, _client, cancellationToken).CAF();
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
        public Task CommitAsync(TimeSpan timeout = default)
            => TaskEx.WithTimeout(CommitAsync, timeout, _defaultOperationTimeoutMilliseconds);

        /// <inheritdoc />
        public async Task CommitAsync(CancellationToken cancellationToken)
        {
            if (State != TransactionState.Active)
                throw new InvalidOperationException("There is no active transaction to commit.");

            if (_threadId != ContextId)
                throw new InvalidOperationException("Transactions cannot span multiple threads.");

            var timeoutMilliseconds = _options.Timeout.TimeoutMilliseconds(0, int.MaxValue);
            if (_startTime + timeoutMilliseconds < Clock.Milliseconds)
                throw new TransactionException("Transaction has timed out.");

            try
            {
                var requestMessage = TransactionCommitCodec.EncodeRequest(TransactionId, ContextId);
                var responseMessage = await _cluster.SendToClientAsync(requestMessage, _client, cancellationToken).CAF();
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
        public Task RollbackAsync(TimeSpan timeout = default)
            => TaskEx.WithTimeout(RollbackAsync, timeout, _defaultOperationTimeoutMilliseconds);

        /// <inheritdoc />
        public async Task RollbackAsync(CancellationToken cancellationToken)
        {
            if (State == TransactionState.RollingBack)
            {
                State = TransactionState.RolledBack;
                return;
            }

            if (State != TransactionState.Active)
                throw new InvalidOperationException("There is no active transaction to roll back.");

            if (_threadId != ContextId)
                throw new InvalidOperationException("Transactions cannot span multiple threads.");

            try
            {
                var requestMessage = TransactionRollbackCodec.EncodeRequest(TransactionId, ContextId);
                var responseMessage = await _cluster.SendToClientAsync(requestMessage, _client, cancellationToken).CAF();
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

            if (State != TransactionState.None ||
                State != TransactionState.Committed ||
                State != TransactionState.RolledBack)
            {
                // FIXME what shall we do of abandoned transaction?
            }
        }

        // Objects

        /// <inheritdoc />
        public Task<IHTxList<TItem>> GetListAsync<TItem>(IHList<TItem> source, TimeSpan timeout = default)
            => TaskEx.WithTimeout(GetListAsync<TItem>, source.Name, timeout, _defaultOperationTimeoutMilliseconds);

        /// <inheritdoc />
        public Task<IHTxList<TItem>> GetListAsync<TItem>(string name, TimeSpan timeout = default)
            => TaskEx.WithTimeout(GetListAsync<TItem>, name, timeout, _defaultOperationTimeoutMilliseconds);

        /// <inheritdoc />
        public Task<IHTxList<TItem>> GetListAsync<TItem>(IHList<TItem> source, CancellationToken cancellationToken)
            => GetListAsync<TItem>(source.Name, cancellationToken);

        /// <inheritdoc />
        public Task<IHTxList<TItem>> GetListAsync<TItem>(string name, CancellationToken cancellationToken)
        {
            return _distributedObjectFactory.GetOrCreateAsync<IHTxList<TItem>, HTxList<TItem>>(HList.ServiceName, name, true,
                (n, cluster, serializationService, loggerFactory) => new HTxList<TItem>(name, cluster, _client, TransactionId, serializationService, loggerFactory),
                cancellationToken);
        }

        /// <inheritdoc />
        public Task<IHTxSet<TItem>> GetSetAsync<TItem>(IHSet<TItem> source, TimeSpan timeout = default)
            => TaskEx.WithTimeout(GetSetAsync<TItem>, source.Name, timeout, _defaultOperationTimeoutMilliseconds);

        /// <inheritdoc />
        public Task<IHTxSet<TItem>> GetSetAsync<TItem>(string name, TimeSpan timeout = default)
            => TaskEx.WithTimeout(GetSetAsync<TItem>, name, timeout, _defaultOperationTimeoutMilliseconds);

        /// <inheritdoc />
        public Task<IHTxSet<TItem>> GetSetAsync<TItem>(IHSet<TItem> source, CancellationToken cancellationToken)
            => GetSetAsync<TItem>(source.Name, cancellationToken);

        /// <inheritdoc />
        public Task<IHTxSet<TItem>> GetSetAsync<TItem>(string name, CancellationToken cancellationToken)
        {
            return _distributedObjectFactory.GetOrCreateAsync<IHTxSet<TItem>, HTxSet<TItem>>(HSet.ServiceName, name, true,
                (n, cluster, serializationService, loggerFactory) => new HTxSet<TItem>(name, cluster, _client, TransactionId, serializationService, loggerFactory),
                cancellationToken);
        }

        public Task<IHTxQueue<TItem>> GetQueueAsync<TItem>(IHQueue<TItem> source, TimeSpan timeout = default)
            => TaskEx.WithTimeout(GetQueueAsync<TItem>, source.Name, timeout, _defaultOperationTimeoutMilliseconds);

        public Task<IHTxQueue<TItem>> GetQueueAsync<TItem>(string name, TimeSpan timeout = default)
            => TaskEx.WithTimeout(GetQueueAsync<TItem>, name, timeout, _defaultOperationTimeoutMilliseconds);

        public Task<IHTxQueue<TItem>> GetQueueAsync<TItem>(IHQueue<TItem> source, CancellationToken cancellationToken)
            => GetQueueAsync<TItem>(source.Name, cancellationToken);

        public Task<IHTxQueue<TItem>> GetQueueAsync<TItem>(string name, CancellationToken cancellationToken)
        {
            return _distributedObjectFactory.GetOrCreateAsync<IHTxQueue<TItem>, HTxQueue<TItem>>(HQueue.ServiceName, name, true,
                (n, cluster, serializationService, loggerFactory) => new HTxQueue<TItem>(name, cluster, _client, TransactionId, serializationService, loggerFactory),
                cancellationToken);
        }

        public Task<IHTxMultiMap<TKey, TValue>> GetMultiMapAsync<TKey, TValue>(IHMultiMap<TKey, TValue> source, TimeSpan timeout = default)
            => TaskEx.WithTimeout(GetMultiMapAsync<TKey, TValue>, source.Name, timeout, _defaultOperationTimeoutMilliseconds);

        public Task<IHTxMultiMap<TKey, TValue>> GetMultiMapAsync<TKey, TValue>(string name, TimeSpan timeout = default)
            => TaskEx.WithTimeout(GetMultiMapAsync<TKey, TValue>, name, timeout, _defaultOperationTimeoutMilliseconds);

        public Task<IHTxMultiMap<TKey, TValue>> GetMultiMapAsync<TKey, TValue>(IHMultiMap<TKey, TValue> source, CancellationToken cancellationToken)
            => GetMultiMapAsync<TKey, TValue>(source.Name, cancellationToken);

        public Task<IHTxMultiMap<TKey, TValue>> GetMultiMapAsync<TKey, TValue>(string name, CancellationToken cancellationToken)
        {
            return _distributedObjectFactory.GetOrCreateAsync<IHTxMultiMap<TKey, TValue>, HTxMultiMap<TKey, TValue>>(HMultiMap.ServiceName, name, true,
                (n, cluster, serializationService, loggerFactory) => new HTxMultiMap<TKey, TValue>(name, cluster, _client, TransactionId, serializationService, loggerFactory),
                cancellationToken);
        }

        public Task<IHTxMap<TKey, TValue>> GetMapAsync<TKey, TValue>(IHMap<TKey, TValue> source, TimeSpan timeout = default)
            => TaskEx.WithTimeout(GetMapAsync<TKey, TValue>, source.Name, timeout, _defaultOperationTimeoutMilliseconds);

        public Task<IHTxMap<TKey, TValue>> GetMapAsync<TKey, TValue>(string name, TimeSpan timeout = default)
            => TaskEx.WithTimeout(GetMapAsync<TKey, TValue>, name, timeout, _defaultOperationTimeoutMilliseconds);

        public Task<IHTxMap<TKey, TValue>> GetMapAsync<TKey, TValue>(IHMap<TKey, TValue> source, CancellationToken cancellationToken)
            => GetMapAsync<TKey, TValue>(source.Name, cancellationToken);

        public Task<IHTxMap<TKey, TValue>> GetMapAsync<TKey, TValue>(string name, CancellationToken cancellationToken)
        {
            return _distributedObjectFactory.GetOrCreateAsync<IHTxMap<TKey, TValue>, HTxMap<TKey, TValue>>(HMap.ServiceName, name, true,
                (n, cluster, serializationService, loggerFactory) => new HTxMap<TKey, TValue>(name, cluster, _client, TransactionId, serializationService, loggerFactory),
                cancellationToken);
        }
    }
}
