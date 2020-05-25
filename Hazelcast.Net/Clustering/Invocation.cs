﻿// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.Messaging;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents an ongoing server invocation.
    /// </summary>
    internal class Invocation
    {
        private static readonly int MinRetryDelayMilliseconds;

        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// Initializes the <see cref="Invocation"/> class.
        /// </summary>
        static Invocation()
        {
            MinRetryDelayMilliseconds = Constants.Invocation.MinRetryDelayMilliseconds;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Invocation"/> class.
        /// </summary>
        /// <param name="requestMessage">The request message.</param>
        /// <param name="boundToClient">Whether the invocation is bound to a client.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <remarks>
        /// <para>When an invocation is bound to a client, it cannot be retried if the client dies.</para>
        /// </remarks>
        public Invocation(ClientMessage requestMessage, bool boundToClient, CancellationToken cancellationToken)
        {
            RequestMessage = requestMessage;
            BoundToClient = boundToClient;
            CorrelationId = requestMessage.CorrelationId;
            CompletionSource = new TaskCompletionSource<ClientMessage>();
            cancellationToken.Register(() => CompletionSource.TrySetCanceled());
            AttemptsCount = 1;
        }

        /// <summary>
        /// Gets the request message.
        /// </summary>
        public ClientMessage RequestMessage { get; }

        /// <summary>
        /// Whether the invocation to bound to a client.
        /// </summary>
        public bool BoundToClient { get; }

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
        public long StartTime => Clock.Milliseconds;

        /// <summary>
        /// Transition the task to <see cref="TaskStatus.RanToCompletion"/>.
        /// </summary>
        /// <param name="result">The response message.</param>
        public void SetResult(ClientMessage result)
            => CompletionSource.SetResult(result);

        /// <summary>
        /// Transition the task to <see cref="TaskStatus.Faulted"/>.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void SetException(Exception exception)
            => CompletionSource.SetException(exception);

        /// <summary>
        /// Determines whether to retry the invocation with a new correlation identifier.
        /// </summary>
        /// <param name="correlationIdProvider">A correlation identifier provider.</param>
        /// <returns>true if the invocation can be retried; otherwise false.</returns>
        public async ValueTask<bool> CanRetry(Func<long> correlationIdProvider)
        {
            AttemptsCount += 1;

            CompletionSource = new TaskCompletionSource<ClientMessage>();
            RequestMessage.CorrelationId = CorrelationId = correlationIdProvider();

            // fast retry (no delay)
            if (AttemptsCount <= Constants.Invocation.MaxFastInvocationCount)
                return true;

            // otherwise, slow retry (delay)

            // implement some rudimentary increasing delay based on the number of attempts
            // will be 1, 2, 4, 8, 16 etc milliseconds but never less that invocationRetryDelayMilliseconds
            // we *may* want to tweak this?
            var delayMilliseconds = Math.Min(1 << (AttemptsCount - Constants.Invocation.MaxFastInvocationCount), MinRetryDelayMilliseconds);
            await System.Threading.Tasks.Task.Delay(delayMilliseconds, _cancellationToken);
            if (_cancellationToken.IsCancellationRequested) return false;
            // FIXME: or, would await throw?!
            return true;
        }
    }
}