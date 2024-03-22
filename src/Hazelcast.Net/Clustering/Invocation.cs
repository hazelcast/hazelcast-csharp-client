// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Models;
using Hazelcast.Protocol;
using Hazelcast.Protocol.Models;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents an ongoing server invocation.
    /// </summary>
    internal class Invocation
    {
        private readonly MessagingOptions _messagingOptions;

        private TaskCompletionSource<ClientMessage> _completionSource;
        private int _attemptsCount; // number of times this invocation has been attempted

        /// <summary>
        /// Initializes a new instance of the <see cref="Invocation"/> class.
        /// </summary>
        /// <param name="requestMessage">The request message.</param>
        /// <param name="messagingOptions">Messaging options.</param>
        public Invocation(ClientMessage requestMessage, MessagingOptions messagingOptions)
        {
            RequestMessage = requestMessage ?? throw new ArgumentNullException(nameof(requestMessage));
            _messagingOptions = messagingOptions ?? throw new ArgumentNullException(nameof(messagingOptions));
            CorrelationId = requestMessage.CorrelationId;
            InitializeNewCompletionSource();
            _attemptsCount = 1;
            StartTime = Clock.Milliseconds;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Invocation"/> class.
        /// </summary>
        /// <param name="requestMessage">The request message.</param>
        /// <param name="messagingOptions">Messaging options.</param>
        /// <param name="targetClientConnection">An optional client connection, that the invocation is bound to.</param>
        /// <remarks>
        /// <para>When an invocation is bound to a client, it will only be sent to that client,
        /// and it cannot and will not be retried if the client dies.</para>
        /// </remarks>
        public Invocation(ClientMessage requestMessage, MessagingOptions messagingOptions, MemberConnection targetClientConnection)
            : this(requestMessage, messagingOptions)
        {
            TargetClientConnection = targetClientConnection ?? throw new ArgumentNullException(nameof(targetClientConnection));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Invocation"/> class.
        /// </summary>
        /// <param name="requestMessage">The request message.</param>
        /// <param name="messagingOptions">Messaging options.</param>
        /// <param name="partitionId">The identifier of the target partition.</param>
        /// <remarks>
        /// <para>If the target partition cannot be mapped to an available member, another random member will be used.</para>
        /// </remarks>
        public Invocation(ClientMessage requestMessage, MessagingOptions messagingOptions, int partitionId)
            : this(requestMessage, messagingOptions)
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
        /// <remarks>
        /// <para>If the target member is not available, another random member will be used.</para>
        /// </remarks>
        public Invocation(ClientMessage requestMessage, MessagingOptions messagingOptions, Guid targetMemberId)
            : this(requestMessage, messagingOptions)
        {
            if (targetMemberId == default) throw new ArgumentException("Must be a non-default Guid.", nameof(targetMemberId));
            TargetMemberId = targetMemberId;
        }

        /// <summary>
        /// Gets the request message.
        /// </summary>
        public ClientMessage RequestMessage { get; }

        /// <summary>
        /// Gets the flags.
        /// </summary>
        public InvocationFlags Flags => RequestMessage.InvocationFlags;

        /// <summary>
        /// Gets the target client connection, if any, otherwise <c>null</c>.
        /// </summary>
        public MemberConnection TargetClientConnection { get; }

        /// <summary>
        /// Gets the unique identifier of the target partition, if any, otherwise <c>-1</c>.
        /// </summary>
        public int TargetPartitionId { get; } = -1;

        /// <summary>
        /// Gets the unique identifier of the target member, if any, otherwise <c>default(Guid)</c>.
        /// </summary>
        public Guid TargetMemberId { get; }

        /// <summary>
        /// Gets the correlation identifier.
        /// </summary>
        public long CorrelationId { get; private set; }

        /// <summary>
        /// Gets the completion task.
        /// </summary>
        public Task<ClientMessage> Task => _completionSource.Task;

        /// <summary>
        /// Gets the start time.
        /// </summary>
        public long StartTime { get; }

        /// <summary>
        /// Attempts to transition the task to the TaskStatus.Canceled state.
        /// </summary>
        public void TrySetCanceled()
        {
            _completionSource.TrySetCanceled();
        }

        /// <summary>
        /// Attempts to transition the task to the <see cref="TaskStatus.RanToCompletion"/> state.
        /// </summary>
        /// <param name="result">The response message.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>This operation will return false if the task has already been completed,
        /// faulted or canceled. This method also returns false if the task has been disposed.</para>
        /// </remarks>
        public bool TrySetResult(ClientMessage result)
        {
            return _completionSource.TrySetResult(result);
        }

        /// <summary>
        /// Attempts to transition the task to the <see cref="TaskStatus.Faulted"/> state.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns><c>true</c> if the operation was successful; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>This operation will return false if the task has already been completed,
        /// faulted or canceled. This method also returns false if the task has been disposed.</para>
        /// </remarks>
        public bool TrySetException(Exception exception)
        {
            return _completionSource.TrySetException(exception);
        }

        /// <summary>
        /// Determines whether an invocation should be retried after an exception was thrown.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="retryUnsafeOperations">Whether to retry on <see cref="TargetDisconnectedException"/>.</param>
        /// <param name="retryOnClientReconnecting">Whether to retry on <see cref="ClientOfflineException"/>.</param>
        /// <returns>true if the invocation should be retried; otherwise false.</returns>
        /// <remarks>
        /// <para>If it is determined that the invocation should be retried, it does not necessarily
        /// mean that it can be retried, and that will be determined by <see cref="WaitRetryAsync"/>.</para>
        /// <para>If the underlying socket connection is disconnected while the invocation is running, an
        /// <see cref="TargetDisconnectedException"/> is thrown. If <paramref name="retryUnsafeOperations"/>
        /// is true, the invocation can be retried, on another connection. Otherwise, the invocation fails
        /// immediately.</para>
        /// <para>If the client goes offline while the invocation is running, and <see cref="ClientOfflineException"/>
        /// exception is thrown. If the client has shutdown, there is no chance it can come back online, and the
        /// invocation fails immediately. On the other hand, if the client is still active and trying to reconnect,
        /// and <paramref name="retryOnClientReconnecting"/>, the invocation can be retried. Otherwise, the invocation
        /// fails immediately.</para>
        /// <para>Note that in all cases, the invocation is retried only until its timeout is reached, and then
        /// it fails.</para>
        /// </remarks>
        public bool IsRetryable(Exception exception, bool retryUnsafeOperations, bool retryOnClientReconnecting)
        {
            switch (exception)
            {
                // a remote exception sent by the server, which explicitly indicates that
                // the invocation can be retried, so we always retry it
                case RemoteException { Retryable: true }:
                    return true; // always

                // messaging would not even invoke this method if the client was not active anymore, but
                // better be sure - and then, this exception can only be thrown if no connection was found,
                // so we did *not* even talk to the cluster = safe to retry
                case ClientOfflineException clientOfflineException when clientOfflineException.State.IsActiveState():
                    return TargetClientConnection == null && // not bound to a connection
                           retryOnClientReconnecting; // is retryable

                // these are .NET exceptions and really, anything could have happened, so retry only if ok
                case IOException _:
                case SocketException _:

                // target disconnected protocol error is not automatically retryable, because we need to
                // perform more checks on the client and message - the request need to be retryable (for
                // instance read-only) or unsafe operations need to be explicitly allowed to retry
                case RemoteException { Error: RemoteError.TargetDisconnected }:
                case TargetUnreachableException _:
                    return TargetClientConnection == null && // not bound to a connection
                           (RequestMessage.IsRetryable || retryUnsafeOperations); // is retryable

                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether to retry the invocation with a new correlation identifier.
        /// </summary>
        /// <param name="correlationIdProvider">A correlation identifier provider.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public ValueTask WaitRetryAsync(Func<long> correlationIdProvider, CancellationToken cancellationToken = default)
        {
            if (correlationIdProvider == null) throw new ArgumentNullException(nameof(correlationIdProvider));

            // fast fail on cancel
            cancellationToken.ThrowIfCancellationRequested();

            // fast fail on timeout
            var elapsedMilliseconds = (int) (Clock.Milliseconds - StartTime);
            if (elapsedMilliseconds >= _messagingOptions.RetryTimeoutSeconds * 1000)
                throw new TaskTimeoutException($"Cannot retry the invocation: timeout ({_messagingOptions.RetryTimeoutSeconds}s).");

            _attemptsCount += 1;

            // we are going to return true, either immediately or after a delay, prepare
            RequestMessage.CorrelationId = CorrelationId = correlationIdProvider();

            // fast retry (no delay) the first attempts
            if (_attemptsCount <= _messagingOptions.MaxFastInvocationCount)
            {
                InitializeNewCompletionSource();
                return default;
            }

            return WaitRetryAsync2(elapsedMilliseconds, cancellationToken);
        }

        private async ValueTask WaitRetryAsync2(int elapsedMilliseconds, CancellationToken cancellationToken)
        {
            // otherwise, slow retry (delay)

            // implement some rudimentary increasing delay based on the number of attempts
            // will be 1, 2, 4, 8, 16 etc milliseconds but never less that invocationRetryDelayMilliseconds
            // TODO: this is the original v4 implementation, can we do better (use an IRetryStrategy)?
            var delayMilliseconds = Math.Max(1 << (_attemptsCount - _messagingOptions.MaxFastInvocationCount), _messagingOptions.MinRetryDelayMilliseconds);

            // but no more than the remaining milliseconds before timeout (if any)
            // if we come here, remainingMilliseconds *is* positive (tested in WaitRetryAsync)
            var remainingMilliseconds = _messagingOptions.RetryTimeoutSeconds * 1000 - elapsedMilliseconds;
            delayMilliseconds = Math.Min(delayMilliseconds, remainingMilliseconds);

            await System.Threading.Tasks.Task.Delay(delayMilliseconds, cancellationToken).CfAwait(); // throws if cancelled

            InitializeNewCompletionSource();
        }

        private void InitializeNewCompletionSource()
        {
            // set options to RunContinuationsAsynchronously so that when the response message
            // is received and we set the result of the completion source, the code waiting on
            // the response runs asynchronously on a new task while the networking code proceeds
            // with messages
            _completionSource = new TaskCompletionSource<ClientMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
