using System;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.DistributedObjects.HTxListImpl;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Hazelcast.Transactions;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects
{
    internal class TransactionContext : ITransactionContext // FIXME IAsyncDisposable? with .Complete();?
    {
        private readonly int _defaultTimeout = 120_000; // FIXME operation timeout (see dist. object)
        private readonly int _waitForClientsMilliseconds = 1_000; // FIXME from Networking configuration

        private readonly Cluster _cluster;
        private readonly TransactionOptions _options;
        private readonly DistributedObjectFactory _distributedObjectFactory;

        private bool? _threadFlag; // FIXME?! should be [ThreadStatic] static = ??

        private long _threadId; // the "threadId", i.e. async context, which owns the transaction
        private long _startTime; // the start time of the transaction
        private Client _client; // the client supporting the transaction

        // FIXME
        // could we rename to transaction scope, have it auto-begin when created, commit or rollback when disposed?

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
        /// Gets the current "thread identifier".
        /// </summary>
        /// <remarks>
        /// Hazelcast APIs call this the thread identified and maintain locks "per threads",
        /// so we are keeping the name here internally, but in reality this is not a thread
        /// identifier anymore - it is attached to the async context so it can flow with
        /// async operations.
        /// </remarks>
        private static long ThreadId => AsyncContext.CurrentContext.Id;

        /// <inheritdoc />
        public Task ConnectAsync(TimeSpan timeout = default)
            => TaskEx.WithTimeout(ConnectAsync, timeout, 60_000); // FIXME default?!

        /// <inheritdoc />
        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // this is just basically retrieving a random client
                var client = _cluster.GetRandomClient();
                if (client == null)
                {
                    // no clients => wait for clients
                    await Task.Delay(_waitForClientsMilliseconds, cancellationToken).CAF();
                    continue;
                }

                _client = client;
                return;
            }

            // this *will* throw
            cancellationToken.ThrowIfCancellationRequested();
        }

        /// <inheritdoc />
        public Task BeginAsync(TimeSpan timeout = default)
            => TaskEx.WithTimeout(BeginAsync, timeout, _defaultTimeout);

        /// <inheritdoc />
        public async Task BeginAsync(CancellationToken cancellationToken)
        {
            // FIXME lock? or document that this is not thread-safe! (well, trx cannot be, really)

            if (_client == null)
                throw new InvalidOperationException("The transaction context is not connected.");

            if (State != TransactionState.None)
                throw new InvalidOperationException("The transaction context is already involved in a transaction.");

            // FIXME that does not make sense here?!
            //if (_threadId != ThreadId)
            //    throw new InvalidOperationException("Transactions cannot span multiple threads.");

            if (_threadFlag != null)
                throw new InvalidOperationException("Nested transactions are not supported.");

            _threadFlag = true;
            _threadId = ThreadId;
            _startTime = Clock.Milliseconds;

            try
            {
                var timeoutMilliseconds = _options.Timeout.TimeoutMilliseconds(0, int.MaxValue);
                var requestMessage = TransactionCreateCodec.EncodeRequest(timeoutMilliseconds, _options.Durability, (int) _options.Type, ThreadId);
                var responseMessage = await _cluster.SendToClientAsync(requestMessage, _client, cancellationToken).CAF();
                TransactionId = TransactionCreateCodec.DecodeResponse(responseMessage).Response;
                State = TransactionState.Active;
            }
            catch
            {
                _threadFlag = null;
                _threadId = 0;
                _startTime = 0;
                TransactionId = default;
                State = TransactionState.None;
                throw;
            }
        }

        /// <inheritdoc />
        public Task CommitAsync(TimeSpan timeout = default)
            => TaskEx.WithTimeout(CommitAsync, timeout, _defaultTimeout);

        /// <inheritdoc />
        public async Task CommitAsync(CancellationToken cancellationToken)
        {
            if (State != TransactionState.Active)
                throw new InvalidOperationException("There is no active transaction to commit.");

            if (_threadId != ThreadId)
                throw new InvalidOperationException("Transactions cannot span multiple threads.");

            var timeoutMilliseconds = _options.Timeout.TimeoutMilliseconds(0, int.MaxValue);
            if (_startTime + timeoutMilliseconds < Clock.Milliseconds)
                throw new TransactionException("Transaction has timed out.");

            try
            {
                var requestMessage = TransactionCommitCodec.EncodeRequest(TransactionId, ThreadId);
                var responseMessage = await _cluster.SendToClientAsync(requestMessage, _client, cancellationToken).CAF();
                _ = TransactionCommitCodec.DecodeResponse(responseMessage);
                TransactionId = default; // FIXME reset more things?
                State = TransactionState.Committed; // FIXME cannot reuse for another transaction, then?
            }
            catch
            {
                State = TransactionState.RollingBack;
                throw;
            }
            finally
            {
                _threadFlag = null;
            }
        }

        /// <inheritdoc />
        public Task RollbackAsync(TimeSpan timeout = default)
            => TaskEx.WithTimeout(RollbackAsync, timeout, _defaultTimeout);

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

            if (_threadId != ThreadId)
                throw new InvalidOperationException("Transactions cannot span multiple threads.");

            try
            {
                var requestMessage = TransactionRollbackCodec.EncodeRequest(TransactionId, ThreadId);
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
                _threadFlag = null;
            }
        }

        // FIXME are we ever throwing TransactionXxxException?

        // Objects

        /// <inheritdoc />
        public Task<IHTxList<TItem>> GetListAsync<TItem>(IHList<TItem> list, TimeSpan timeout = default)
            => TaskEx.WithTimeout(GetListAsync<TItem>, list.Name, timeout, _defaultTimeout);

        /// <inheritdoc />
        public Task<IHTxList<TItem>> GetListAsync<TItem>(string name, TimeSpan timeout = default)
            => TaskEx.WithTimeout(GetListAsync<TItem>, name, timeout, _defaultTimeout);

        /// <inheritdoc />
        public Task<IHTxList<TItem>> GetListAsync<TItem>(IHList<TItem> list, CancellationToken cancellationToken)
            => GetListAsync<TItem>(list.Name, cancellationToken);

        /// <inheritdoc />
        public Task<IHTxList<TItem>> GetListAsync<TItem>(string name, CancellationToken cancellationToken)
        {
            return _distributedObjectFactory.GetOrCreateAsync<IHTxList<TItem>, HTxList<TItem>>(HList.ServiceName, name, true,
                (n, cluster, serializationService, loggerFactory) => new HTxList<TItem>(name, cluster, _client, TransactionId, serializationService, loggerFactory),
                cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            await _distributedObjectFactory.DisposeAsync().CAF();
        }
    }
}
