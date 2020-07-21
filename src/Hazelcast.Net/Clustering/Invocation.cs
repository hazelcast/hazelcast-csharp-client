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
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Exceptions;
using Hazelcast.Messaging;
using Hazelcast.Protocol;
using Hazelcast.Protocol.Data;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents an ongoing server invocation.
    /// </summary>
    internal class Invocation
    {
        private readonly MessagingOptions _messagingOptions;
        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="Invocation"/> class.
        /// </summary>
        /// <param name="requestMessage">The request message.</param>
        /// <param name="messagingOptions">Messaging options.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public Invocation(ClientMessage requestMessage, MessagingOptions messagingOptions, CancellationToken cancellationToken)
        {
            RequestMessage = requestMessage ?? throw new ArgumentNullException(nameof(requestMessage));
            _messagingOptions = messagingOptions ?? throw new ArgumentNullException(nameof(messagingOptions));
            CorrelationId = requestMessage.CorrelationId;
            CompletionSource = new TaskCompletionSource<ClientMessage>();
            _cancellationToken = cancellationToken;
            _cancellationToken.Register(() => CompletionSource.TrySetCanceled());
            AttemptsCount = 1;
            StartTime = Clock.Milliseconds;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Invocation"/> class.
        /// </summary>
        /// <param name="requestMessage">The request message.</param>
        /// <param name="messagingOptions">Messaging options.</param>
        /// <param name="targetClient">An optional client, that the invocation is bound to.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <remarks>
        /// <para>When an invocation is bound to a client, it will only be sent to that client,
        /// and it cannot and will not be retried if the client dies.</para>
        /// </remarks>
        public Invocation(ClientMessage requestMessage, MessagingOptions messagingOptions, ClientConnection targetClient, CancellationToken cancellationToken)
            : this(requestMessage, messagingOptions, cancellationToken)
        {
            TargetClient = targetClient ?? throw new ArgumentNullException(nameof(targetClient));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Invocation"/> class.
        /// </summary>
        /// <param name="requestMessage">The request message.</param>
        /// <param name="messagingOptions">Messaging options.</param>
        /// <param name="partitionId">The identifier of the target partition.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <remarks>
        /// <para>If the target partition cannot be mapped to an available member, another random member will be used.</para>
        /// </remarks>
        public Invocation(ClientMessage requestMessage, MessagingOptions messagingOptions, int partitionId, CancellationToken cancellationToken)
            : this(requestMessage, messagingOptions, cancellationToken)
        {
            if (partitionId < 0) throw new ArgumentException("Must be a positive integer.", nameof(partitionId));
            TargetPartitionId = partitionId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Invocation"/> class.
        /// </summary>
        /// <param name="requestMessage">The request message.</param>
        /// <param name="messagingOptions">Messaging options.</param>
        /// <param name="targetMemberId">The identifier of the target member.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <remarks>
        /// <para>If the target member is not available, another random member will be used.</para>
        /// </remarks>
        public Invocation(ClientMessage requestMessage, MessagingOptions messagingOptions, Guid targetMemberId, CancellationToken cancellationToken)
            : this(requestMessage, messagingOptions, cancellationToken)
        {
            if (targetMemberId == default) throw new ArgumentException("Must be a non-default Guid.", nameof(targetMemberId));
            TargetMemberId = targetMemberId;
        }

        /// <summary>
        /// Gets the request message.
        /// </summary>
        public ClientMessage RequestMessage { get; }

        /// <summary>
        /// Gets the target client, if any, otherwise <c>null</c>.
        /// </summary>
        public ClientConnection TargetClient { get; }

        /// <summary>
        /// Gets the unique identifier of the target partition, if any, otherwise <c>-1</c>.
        /// </summary>
        public int TargetPartitionId { get; }

        /// <summary>
        /// Gets the unique identifier of the target member, if any, otherwise <c>default(Guid)</c>.
        /// </summary>
        public Guid TargetMemberId { get; }

        /// <summary>
        /// Gets the correlation identifier.
        /// </summary>
        public long CorrelationId { get; private set; }

        /// <summary>
        /// Gets the number of time this invocation has been attempted.
        /// </summary>
        public int AttemptsCount { get; private set; }

        /// <summary>
        /// Gets the completion source.
        /// </summary>
        public TaskCompletionSource<ClientMessage> CompletionSource { get; private set; }

        /// <summary>
        /// Gets the completion task.
        /// </summary>
        public Task<ClientMessage> Task => CompletionSource.Task;

        /// <summary>
        /// Gets the start time.
        /// </summary>
        public long StartTime { get; }

        /// <summary>
        /// Transition the task to <see cref="TaskStatus.RanToCompletion"/>.
        /// </summary>
        /// <param name="result">The response message.</param>
        public void SetResult(ClientMessage result)
            => CompletionSource.SetResult(result);

        /// <summary>
        /// Transitions the task to <see cref="TaskStatus.Faulted"/>.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void SetException(Exception exception)
            => CompletionSource.SetException(exception);

        /// <summary>
        /// Attempts to transition the task to <see cref="TaskStatus.Faulted"/>.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void TrySetException(Exception exception)
            => CompletionSource.TrySetException(exception);

        /// <summary>
        /// Determines whether an invocation should be retried after an exception was thrown.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="retryOnTargetDisconnected">Whether to retry on <see cref="TargetDisconnectedException"/>.</param>
        /// <returns>true if the invocation should be retried; otherwise false.</returns>
        /// <remarks>
        /// <para>If it is determined that the invocation should be retried, it does not necessarily
        /// mean that it can be retried, and that will be determined by <see cref="CanRetryAsync"/>.</para>
        /// </remarks>
        public bool ShouldRetry(Exception exception, bool retryOnTargetDisconnected)
        {
            switch (exception)
            {
                case IOException _:
                    return TargetClient == null; // not bound to a client

                case SocketException _:
                case ClientProtocolException cpe when cpe.Retryable:
                    return true;

                // target disconnected protocol error is not automatically retryable,
                // because we need to perform more checks on the client and message
                case ClientProtocolException cpe when cpe.Error == ClientProtocolError.TargetDisconnected:
                case TargetDisconnectedException _:
                    return TargetClient == null && // not bound to a client
                           (RequestMessage.IsRetryable || retryOnTargetDisconnected);

                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether to retry the invocation with a new correlation identifier.
        /// </summary>
        /// <param name="correlationIdProvider">A correlation identifier provider.</param>
        /// <returns>true if the invocation can be retried; otherwise false.</returns>
        public async ValueTask<bool> CanRetryAsync(Func<long> correlationIdProvider)
        {
            if (correlationIdProvider == null) throw new ArgumentNullException(nameof(correlationIdProvider));

            if (_cancellationToken.IsCancellationRequested) return false;

            AttemptsCount += 1;

            CompletionSource = new TaskCompletionSource<ClientMessage>();
            RequestMessage.CorrelationId = CorrelationId = correlationIdProvider();

            // fast retry (no delay)
            if (AttemptsCount <= _messagingOptions.MaxFastInvocationCount)
                return true;

            // otherwise, slow retry (delay)

            // implement some rudimentary increasing delay based on the number of attempts
            // will be 1, 2, 4, 8, 16 etc milliseconds but never less that invocationRetryDelayMilliseconds
            // we *may* want to tweak this?
            var delayMilliseconds = Math.Min(1 << (AttemptsCount - _messagingOptions.MaxFastInvocationCount), _messagingOptions.MinRetryDelayMilliseconds);
            await System.Threading.Tasks.Task.Delay(delayMilliseconds, _cancellationToken).CAF(); // throws if cancelled
            return true;
        }
    }
}
