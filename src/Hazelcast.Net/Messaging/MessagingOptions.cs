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

namespace Hazelcast.Messaging
{
    /// <summary>
    /// Represents messaging options.
    /// </summary>
    public class MessagingOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingOptions"/> class.
        /// </summary>
        public MessagingOptions()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingOptions"/> class.
        /// </summary>
        private MessagingOptions(MessagingOptions other)
        {
            MaxFastInvocationCount = other.MaxFastInvocationCount;
            MinRetryDelayMilliseconds = other.MinRetryDelayMilliseconds;
            RetryTimeoutSeconds = other.RetryTimeoutSeconds;
            RetryOnClientReconnecting = other.RetryOnClientReconnecting;
            RetryOnConnectionLost = other.RetryOnConnectionLost;
        }

        /// <summary>
        /// Gets or sets the max fast invocation count.
        /// </summary>
        internal int MaxFastInvocationCount { get; set; } = 5;

        /// <summary>
        /// Gets or sets the min retry delay.
        /// </summary>
        public int MinRetryDelayMilliseconds { get; set; } = 1_000;

        /// <summary>
        /// Gets or sets the invocation timeout.
        /// </summary>
        /// <remarks>
        /// <para>The <see cref="RetryTimeoutSeconds"/> is a soft timeout that prevents
        /// retrying an invocation for too long in case it fails. It does *not* controls
        /// the duration of a single try, and does *not* abort it. And invocation single
        /// try can run for as long as the connection that supports it remains open.</para>
        /// </remarks>
        public int RetryTimeoutSeconds { get; set; } = 120;

        /// <summary>
        /// Whether to retry an invocation that has failed because the client was offline
        /// but still active and reconnecting.
        /// </summary>
        /// <remarks>
        /// <para>This is <c>true</c> by default, i.e. if the client got disconnected and is reconnecting,
        /// invocations will be retried until they reach their timeout, or the client reconnects. Set this
        /// to <c>false</c> if you want invocations to fail immediately in case the client gets
        /// disconnected, even if it is trying to reconnect.</para>
        /// <para>Note that this options is AND-ed with <see cref="Networking.NetworkingOptions.RedoOperations"/>,
        /// i.e. global operations retry must be enabled for this option to be active.</para>
        /// </remarks>
        public bool RetryOnClientReconnecting { get; set; } = true;

        /// <summary>
        /// Whether to retry an invocation that has failed because the underlying socket connection
        /// was lost.
        /// </summary>
        /// <remarks>
        /// <para>This is <c>true</c> by default, i.e. if the underlying socket connection was lost,
        /// invocations will be retried on another connection until they reach their timeout, or a
        /// connection stays up long enough. Set this to <c>false</c> if you want invocations to fail
        /// immediately if the underlying socket connection was lost.</para>
        /// <para>Note that this options is AND-ed with <see cref="Networking.NetworkingOptions.RedoOperations"/>,
        /// i.e. global operations retry must be enabled for this option to be active.</para>
        /// </remarks>
        public bool RetryOnConnectionLost { get; set; } = true;

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal MessagingOptions Clone() => new MessagingOptions(this);
    }
}
