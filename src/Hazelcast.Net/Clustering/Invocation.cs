// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Protocol.Models;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents an ongoing server invocation.
    /// </summary>
    internal class Invocation : IDisposable
    {
        private readonly MessagingOptions _messagingOptions;
        private readonly CancellationToken _cancellationToken;

        // do NOT make that field readonly - we don't want to call Dispose()
        // on a copy of the readonly variable - if may be that it would work
        // as expected with the current implementation of CancellationTokenRegistration
        // but how can we be sure it will never change?
        //
        // reference: https://stackoverflow.com/questions/9927434/impure-method-is-called-for-readonly-field
#pragma warning disable IDE0044 // Add readonly modifier
        private CancellationTokenRegistration _registration;
#pragma warning restore IDE0044 // Add readonly modifier

        private TaskCompletionSource<ClientMessage> _completionSource;
        private int _attemptsCount; // number of times this invocation has been attempted

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
            _cancellationToken = cancellationToken;
            InitializeNewCompletionSource();
            _registration = _cancellationToken.Register(TrySetCanceled); // must dispose to de-register!
            _attemptsCount = 1;
            StartTime = Clock.Milliseconds;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Invocation"/> class.
        /// </summary>
        /// <param name="requestMessage">The request message.</param>
        /// <param name="messagingOptions">Messaging options.</param>
        /// <param name="targetClientConnection">An optional client connection, that the invocation is bound to.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <remarks>
        /// <para>When an invocation is bound to a client, it will only be sent to that client,
        /// and it cannot and will not be retried if the client dies.</para>
        /// </remarks>
        public Invocation(ClientMessage requestMessage, MessagingOptions messagingOptions, MemberConnection targetClientConnection, CancellationToken cancellationToken)
            : this(requestMessage, messagingOptions, cancellationToken)
        {
            TargetClientConnection = targetClientConnection ?? throw new ArgumentNullException(nameof(targetClientConnection));
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
        /// Gets the invocation cancellation token.
        /// </summary>
        public CancellationToken CancellationToken => _cancellationToken;

        /// <summary>
        /// Gets the request message.
        /// </summary>
        public ClientMessage RequestMessage { get; }

        /// <summary>
        /// Gets the target client connection, if any, otherwise <c>null</c>.
        /// </summary>
        public MemberConnection TargetClientConnection { get; }

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
        /// <param name="retryOnTargetDisconnected">Whether to retry on <see cref="TargetDisconnectedException"/>.</param>
        /// <returns>true if the invocation should be retried; otherwise false.</returns>
        /// <remarks>
        /// <para>If it is determined that the invocation should be retried, it does not necessarily
        /// mean that it can be retried, and that will be determined by <see cref="CanRetryAsync"/>.</para>
        /// </remarks>
        public bool IsRetryable(Exception exception, bool retryOnTargetDisconnected)
        {
            switch (exception)
            {
                case IOException _:
                    return TargetClientConnection == null; // not bound to a client

                case SocketException _:
                case RemoteException cpe when cpe.Retryable:
                    return true;

                // target disconnected protocol error is not automatically retryable,
                // because we need to perform more checks on the client and message
                case RemoteException cpe when cpe.Error == RemoteError.TargetDisconnected:
                case TargetDisconnectedException _:
                    return TargetClientConnection == null && // not bound to a client
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

            _attemptsCount += 1;

            // we are going to return true, either immediately or after a delay, prepare
            RequestMessage.CorrelationId = CorrelationId = correlationIdProvider();

            // fast retry (no delay)
            if (_attemptsCount <= _messagingOptions.MaxFastInvocationCount)
            {
                InitializeNewCompletionSource();
                return true;
            }

            // otherwise, slow retry (delay)

            // implement some rudimentary increasing delay based on the number of attempts
            // will be 1, 2, 4, 8, 16 etc milliseconds but never less that invocationRetryDelayMilliseconds
            // we *may* want to tweak this?
            var delayMilliseconds = Math.Min(1 << (_attemptsCount - _messagingOptions.MaxFastInvocationCount), _messagingOptions.MinRetryDelayMilliseconds);
            await System.Threading.Tasks.Task.Delay(delayMilliseconds, _cancellationToken).CfAwait(); // throws if cancelled

            InitializeNewCompletionSource();
            return true;
        }

        private void InitializeNewCompletionSource()
        {
            _completionSource = new TaskCompletionSource<ClientMessage>();
            if (_cancellationToken.IsCancellationRequested) TrySetCanceled();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _registration.Dispose(); // de-register!
        }
    }
}
