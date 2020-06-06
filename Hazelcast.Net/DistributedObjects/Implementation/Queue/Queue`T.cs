using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.DistributedObjects.Implementation.Collection;
using Hazelcast.Messaging;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Collections;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.Implementation.Queue
{
    internal partial class Queue<T> // Setting (Enqueue)
    {
        // <inheritdoc />
        // need that one because we are an HCollection - weird?
        // tries to enqueue immediately, does not wait & does not throw
        public override
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<bool> AddAsync(T item, CancellationToken cancellationToken)
        {
            var task = EnqueueAsync(item, TimeToWait.Zero, false, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        // <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<bool> TryEnqueueAsync(T item)
        {
            var task = EnqueueAsync(item, TimeToWait.Zero, false, CancellationToken.None);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        // <inheritdoc />
        // was: Offer with no timeout - tries to enqueue immediately, does not wait & does not throw
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<bool> TryEnqueueAsync(T item, CancellationToken cancellationToken)
        {
            var task = EnqueueAsync(item, TimeToWait.Zero, false, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        // <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<bool> TryEnqueueAsync(T item, TimeSpan timeToWait)
        {
            var task = EnqueueAsync(item, timeToWait, false, CancellationToken.None);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        // <inheritdoc />
        // was: Offer with timeout - tries to enqueue within timeToWait, does not throw
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<bool> TryEnqueueAsync(T item, TimeSpan timeToWait, CancellationToken cancellationToken)
        {
            var task = EnqueueAsync(item, timeToWait, false, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        // <inheritdoc />
        // was: Put - enqueue, wait indefinitely, may throw
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task EnqueueAsync(T item, TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(DefaultOperationTimeoutMilliseconds);
            var task = EnqueueAsync(item, TimeToWait.InfiniteTimeSpan, true, cancellation.Token).OrTimeout(cancellation);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        // <inheritdoc />
        // was: Put - enqueue, wait indefinitely, may throw
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task EnqueueAsync(T item, CancellationToken cancellationToken)
        {
            var task = EnqueueAsync(item, TimeToWait.InfiniteTimeSpan, true, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            await task.CAF();
#endif
        }

        private async Task<bool> EnqueueAsync(T item, TimeSpan timeToWait, bool doThrow, CancellationToken cancellationToken)
        {
            var itemData = ToSafeData(item);

            var timeToWaitMilliseconds = timeToWait.TimeoutMilliseconds(0);
            var requestMessage = timeToWaitMilliseconds < 0
                ? QueuePutCodec.EncodeRequest(Name, itemData)
                : QueueOfferCodec.EncodeRequest(Name, itemData, timeToWaitMilliseconds);

            ClientMessage responseMessage;
            try
            {
                responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken);
            }
            catch
            {
                if (doThrow) throw;
                return false;
            }

            bool queued;
            if (timeToWaitMilliseconds < 0)
            {
                _ = QueuePutCodec.DecodeResponse(responseMessage);
                queued = true;
            }
            else
            {
                queued = QueueOfferCodec.DecodeResponse(responseMessage).Response;
            }

            if (queued) return true;
            if (doThrow) throw new Exception(); // FIXME
            return false;
        }

        // <inheritdoc />
        // need to have it because HCollection but feels weird
        // maybe we need to have something above HCollection?
        public override async Task<bool> AddRangeAsync<TItem>(ICollection<TItem> items, CancellationToken cancellationToken)
        {
            var itemsData = ToSafeData(items);
            var requestMessage = QueueAddAllCodec.EncodeRequest(Name, itemsData);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken);
            return QueueAddAllCodec.DecodeResponse(responseMessage).Response;
        }
    }

    internal partial class Queue<T> // Removing
    {
        // <inheritdoc />
        public override async Task<bool> RemoveAsync(T item, CancellationToken cancellationToken)
        {
            var itemData = ToSafeData(item);
            var requestMessage = QueueRemoveCodec.EncodeRequest(Name, itemData);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken);
            return QueueRemoveCodec.DecodeResponse(responseMessage).Response;
        }

        // <inheritdoc />
        public override async Task<bool> RemoveAllAsync<TItem>(ICollection<TItem> items, CancellationToken cancellationToken)
        {
            var itemsData = ToSafeData(items);
            var requestMessage = QueueCompareAndRemoveAllCodec.EncodeRequest(Name, itemsData);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken);
            return QueueCompareAndRemoveAllCodec.DecodeResponse(responseMessage).Response;
        }

        // <inheritdoc />
        public override async Task<bool> RetainAllAsync<TItem>(ICollection<TItem> items, CancellationToken cancellationToken)
        {
            var itemsData = ToSafeData(items);
            var requestMessage = QueueCompareAndRetainAllCodec.EncodeRequest(Name, itemsData);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken);
            return QueueCompareAndRetainAllCodec.DecodeResponse(responseMessage).Response;
        }

        // <inheritdoc />
        public override async Task ClearAsync(CancellationToken cancellationToken)
        {
            var requestMessage = QueueClearCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken);
            _ = QueueCompareAndRetainAllCodec.DecodeResponse(responseMessage).Response;
        }
    }

    internal partial class Queue<T> // Getting
    {
        // <inheritdoc />
        public override async Task<int> CountAsync(CancellationToken cancellationToken)
        {
            var requestMessage = QueueSizeCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken);
            return QueueSizeCodec.DecodeResponse(responseMessage).Response;
        }

        // <inheritdoc />
        public override async Task<bool> ContainsAsync(T item, CancellationToken cancellationToken)
        {
            var itemData = ToSafeData(item);
            var requestMessage = QueueContainsCodec.EncodeRequest(Name, itemData);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken);
            return QueueContainsCodec.DecodeResponse(responseMessage).Response;
        }

        // <inheritdoc />
        public override async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken)
        {
            var requestMessage = QueueIteratorCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken);
            var response = QueueIteratorCodec.DecodeResponse(responseMessage).Response;
            return new ReadOnlyLazyList<T>(response, SerializationService);
        }

        // <inheritdoc />
        public override async Task<bool> ContainsAllAsync<TItem>(ICollection<TItem> items, CancellationToken cancellationToken)
        {
            var itemsData = ToSafeData(items);
            var requestMessage = QueueContainsAllCodec.EncodeRequest(Name, itemsData);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken);
            return QueueContainsAllCodec.DecodeResponse(responseMessage).Response;
        }

        // <inheritdoc />
        public override async Task<bool> IsEmptyAsync(CancellationToken cancellationToken)
        {
            var requestMessage = QueueIsEmptyCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken);
            return QueueIsEmptyCodec.DecodeResponse(responseMessage).Response;
        }

        // <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<int> RemainingCapacityAsync(TimeSpan timeout = default)
        {
            var cancellation = timeout.AsCancellationTokenSource(DefaultOperationTimeoutMilliseconds);
            var task = RemainingCapacityAsync(cancellation.Token).OrTimeout(cancellation);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        // <inheritdoc />
        public async Task<int> RemainingCapacityAsync(CancellationToken cancellationToken)
        {
            var requestMessage = QueueRemainingCapacityCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken);
            return QueueRemainingCapacityCodec.DecodeResponse(responseMessage).Response;
        }
    }

    internal partial class Queue<T> // Events
    {
        /// <inheritdoc />
        protected override ClientMessage CreateSubscribeRequest(string name, bool includeValue, bool isSmartRouting)
            => QueueAddListenerCodec.EncodeRequest(name, includeValue, isSmartRouting);

        /// <inheritdoc />
        protected override ClientMessage CreateUnsubscribeRequest(Guid subscriptionId, SubscriptionState<CollectionItemEventHandlers<T>> state)
            => QueueRemoveListenerCodec.EncodeRequest(Name, subscriptionId);

        /// <inheritdoc />
        protected override Guid HandleSubscribeResponse(ClientMessage responseMessage, SubscriptionState<CollectionItemEventHandlers<T>> state)
            => QueueAddListenerCodec.DecodeResponse(responseMessage).Response;

        /// <inheritdoc />
        protected override bool DecodeUnsubscribeResponse(ClientMessage unsubscribeResponseMessage, SubscriptionState<CollectionItemEventHandlers<T>> state)
            => QueueRemoveListenerCodec.DecodeResponse(unsubscribeResponseMessage).Response;
    }

    internal partial class Queue<T> // Dequeue
    {
        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<T> PeekAsync(TimeSpan timeout = default) // peek but throw - was Element
        {
            var cancellation = timeout.AsCancellationTokenSource(DefaultOperationTimeoutMilliseconds);
            var task = PeekAsync(cancellation.Token).OrTimeout(cancellation);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task<T> PeekAsync(CancellationToken cancellationToken) // peek but throw - was Element
            => await TryPeekAsync(cancellationToken) ??
               throw new InvalidOperationException("The queue is empty.");

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<T> TryPeekAsync(TimeSpan timeout = default) // peek, or null
        {
            var cancellation = timeout.AsCancellationTokenSource(DefaultOperationTimeoutMilliseconds);
            var task = TryPeekAsync(cancellation.Token).OrTimeout(cancellation);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task<T> TryPeekAsync(CancellationToken cancellationToken) // peek, or null
        {
            var requestMessage = QueuePeekCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken);
            var response = QueuePeekCodec.DecodeResponse(responseMessage).Response;
            return ToObject<T>(response);
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<T> TryDequeueAsync() // was poll = take immediately with zero timeout = infinite? default?
        {
            var task = TryDequeueAsync(TimeToWait.Zero, CancellationToken.None);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<T> TryDequeueAsync(CancellationToken cancellationToken) // was poll = take immediately with zero timeout = infinite? default?
        {
            var task = TryDequeueAsync(TimeToWait.Zero, cancellationToken);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }


        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<T> TryDequeueAsync(TimeSpan timeToWait) // was poll, take with timeout
        {
            var task = TryDequeueAsync(timeToWait, CancellationToken.None);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task<T> TryDequeueAsync(TimeSpan timeToWait, CancellationToken cancellationToken) // was poll, take with timeout
        {
            var timeToWaitMilliseconds = timeToWait.TimeoutMilliseconds(0);
            var requestMessage = QueuePollCodec.EncodeRequest(Name, timeToWaitMilliseconds);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken);
            var response = QueuePollCodec.DecodeResponse(responseMessage).Response;
            return ToObject<T>(response);
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<T> DequeueAsync(bool waitForItem, TimeSpan timeout = default) // was take, wail until an element is avail
        {
            var cancellation = timeout.AsCancellationTokenSource(DefaultOperationTimeoutMilliseconds);
            var task = DequeueAsync(waitForItem, cancellation.Token).OrTimeout(cancellation);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task<T> DequeueAsync(bool waitForItem, CancellationToken cancellationToken) // was take, wail until an element is avail
        {
            if (!waitForItem)
                return await TryDequeueAsync(TimeToWait.Zero, cancellationToken) ??
                       throw new InvalidOperationException("The queue is empty.");

            var requestMessage = QueueTakeCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken);
            var response = QueueTakeCodec.DecodeResponse(responseMessage).Response;
            return ToObject<T>(response);
        }

        // FIXME: Queue.Drain has issues
        // may throw if the object is T but not TItem, need to review all these weird overloads
        // bit silly, deserializing immediately instead of returning a lazy thing?

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<int> DrainToAsync<TItem>(ICollection<TItem> items, TimeSpan timeout = default)
            where TItem : T
        {
            var cancellation = timeout.AsCancellationTokenSource(DefaultOperationTimeoutMilliseconds);
            var task = DrainToAsync(items, cancellation.Token).OrTimeout(cancellation);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task<int> DrainToAsync<TItem>(ICollection<TItem> items, CancellationToken cancellationToken)
            where TItem : T
        {
            var requestMessage = QueueDrainToCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken);
            var response = QueueDrainToCodec.DecodeResponse(responseMessage).Response;

            foreach (var itemData in response) items.Add((TItem)ToObject<T>(itemData));
            return response.Count;
        }

        /// <inheritdoc />
        public
#if !HZ_OPTIMIZE_ASYNC
            async
#endif
        Task<int> DrainToAsync<TItem>(ICollection<TItem> items, int count, TimeSpan timeout = default)
            where TItem : T
        {
            var cancellation = timeout.AsCancellationTokenSource(DefaultOperationTimeoutMilliseconds);
            var task = DrainToAsync(items, count, cancellation.Token).OrTimeout(cancellation);

#if HZ_OPTIMIZE_ASYNC
            return task;
#else
            return await task.CAF();
#endif
        }

        /// <inheritdoc />
        public async Task<int> DrainToAsync<TItem>(ICollection<TItem> items, int count, CancellationToken cancellationToken)
            where TItem : T
        {
            var requestMessage = QueueDrainToMaxSizeCodec.EncodeRequest(Name, count);
            var responseMessage = await Cluster.SendAsync(requestMessage, cancellationToken);
            var response = QueueDrainToMaxSizeCodec.DecodeResponse(responseMessage).Response;

            foreach (var itemData in response) items.Add((TItem)ToObject<T>(itemData));
            return response.Count;
        }
    }

    // FIXME all CAF are missing -> must implement the ConfigureAwait analyzer

    internal partial class Queue<T> : HCollectionBase<T>, IQueue<T>
    {
        public Queue(string name, Cluster cluster, ISerializationService serializationService, ILoggerFactory loggerFactory)
            : base(DistributedObjects.Queue.ServiceName, name, cluster, serializationService, loggerFactory)
        { }
    }
}
