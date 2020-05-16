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
using Hazelcast.Core;
using Hazelcast.Messaging;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents an ongoing server invocation.
    /// </summary>
    internal class Invocation
    {
        private const int MaxFastInvocationCount = 5;
        private const int DefaultInvocationRetryDelayMilliseconds = 1_000;
        private static readonly int InvocationRetryDelayMilliseconds;

        /// <summary>
        /// Initializes the <see cref="Invocation"/> class.
        /// </summary>
        static Invocation()
        {
            InvocationRetryDelayMilliseconds = HazelcastEnvironment.Invocation.DefaultInvocationRetryDelayMilliseconds ??
                                               DefaultInvocationRetryDelayMilliseconds;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Invocation"/> class.
        /// </summary>
        /// <param name="requestMessage">The request message.</param>
        /// <param name="timeoutMilliseconds">The timeout in milliseconds.</param>
        public Invocation(ClientMessage requestMessage, int timeoutMilliseconds)
        {
            RequestMessage = requestMessage;
            CorrelationId = requestMessage.CorrelationId;
            CompletionSource = new TaskCompletionSource<ClientMessage>();
            StartTime = Clock.Milliseconds;
            TimeoutMilliseconds = timeoutMilliseconds;
            AttemptsCount = 1;
        }

        /// <summary>
        /// Gets the request message.
        /// </summary>
        public ClientMessage RequestMessage { get; }

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
        public TaskCompletionSource<ClientMessage> CompletionSource { get; }

        /// <summary>
        /// Gets the completion task.
        /// </summary>
        public Task<ClientMessage> Task => CompletionSource.Task;

        /// <summary>
        /// Gets the start time.
        /// </summary>
        public long StartTime { get; }

        /// <summary>
        /// Gets the timeout in milliseconds.
        /// </summary>
        public int TimeoutMilliseconds { get; }

        /// <summary>
        /// Whether the invocation has a timeout.
        /// </summary>
        public bool HasTimeout => TimeoutMilliseconds > 0;

        /// <summary>
        /// Gets the remaining time in milliseconds.
        /// </summary>
        public int RemainingMilliseconds =>
            HasTimeout
                ? TimeoutMilliseconds - (int) (Clock.Milliseconds - StartTime)
                : int.MaxValue;

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
        /// Tries to retry the invocation with a new correlation identifier.
        /// </summary>
        /// <param name="correlationIdProvider">A correlation identifier provider.</param>
        /// <param name="delayMilliseconds">The delay in milliseconds to wait before retrying.</param>
        /// <returns></returns>
        public bool TryRetry(Func<long> correlationIdProvider, out int delayMilliseconds)
        {
            AttemptsCount += 1;
            delayMilliseconds = 0;

            // cannot retry if running out of time
            if (RemainingMilliseconds == 0)
                return false;

            // fast retry (no delay)
            if (AttemptsCount <= MaxFastInvocationCount)
                return true;

            // otherwise, slow retry (delay)

            // implement some rudimentary increasing delay based on the number of attempts
            // will be 1, 2, 4, 8, 16 etc milliseconds but never less that invocationRetryDelayMilliseconds
            // we *may* want to tweak this?
            delayMilliseconds = Math.Min(1 << (AttemptsCount - MaxFastInvocationCount), InvocationRetryDelayMilliseconds);

            RequestMessage.CorrelationId = CorrelationId = correlationIdProvider();
            return true;
        }
    }
}