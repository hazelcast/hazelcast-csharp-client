// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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
            RetryUnsafeOperations = other.RetryUnsafeOperations;
            RetryOnClientReconnecting = other.RetryOnClientReconnecting;
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
        /// Whether to retry all operations including unsafe operations.
        /// </summary>
        /// <remarks>
        /// <para>Operations can fail due to various reasons. Read-only operations are retried by
        /// default. If you want to enable retry for all operations, set this property to <c>true</c>.</para>
        /// <para>However, note that a failed operation leaves the cluster in an undecided state. The
        /// cluster may have received the request and executed the operation, but failed to respond
        /// to the client. For idempotent operations this is harmless, but for non idempotent ones retrying
        /// can cause undesirable effects. Also note that the redo can perform on any member.</para>
        /// <para>For these reasons, this is <c>false</c> by default.</para>
        /// </remarks>
        public bool RetryUnsafeOperations { get; set; }

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
        /// Whether to retry an invocation that has failed to start because the client was offline
        /// but still active and reconnecting.
        /// </summary>
        /// <remarks>
        /// <para>This is <c>true</c> by default, i.e. if the client got disconnected and is reconnecting,
        /// invocations will be retried until they reach their timeout, or the client reconnects. Set this
        /// to <c>false</c> if you want invocations to fail immediately in case the client gets
        /// disconnected, even if it is trying to reconnect.</para>
        /// <para>Note that this only applies to invocation that failed to start, and therefore this is
        /// safe for all invocations. See <see cref="RetryUnsafeOperations"/> for what happens once the
        /// invocation has started.</para>
        /// </remarks>
        public bool RetryOnClientReconnecting { get; set; } = true;

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal MessagingOptions Clone() => new(this);
    }
}
