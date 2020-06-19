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
        public MessagingOptions(MessagingOptions other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            
            MaxFastInvocationCount = other.MaxFastInvocationCount;
            MinRetryDelayMilliseconds = other.MinRetryDelayMilliseconds;
            DefaultOperationTimeoutMilliseconds = other.DefaultOperationTimeoutMilliseconds;
        }

        /// <summary>
        /// Gets or sets the max fast invocation count.
        /// </summary>
        public int MaxFastInvocationCount { get; set; } = 5;

        /// <summary>
        /// Gets or sets the min retry delay.
        /// </summary>
        public int MinRetryDelayMilliseconds { get; set; } = 1_000;

        /// <summary>
        /// Gets or sets the default operation timeout.
        /// </summary>
        /// <remarks>
        /// <para>The default operation timeout is the timeout for an operation (a complete
        /// roundtrip to the server, for instance adding an item into a map) when no timeout
        /// is specified, and no cancellation token is provided. If a timeout is specified then
        /// it becomes the timeout for the operation. If a cancellation token is provided, then
        /// that token is supposed to handle the life of the operation and no timeout is
        /// involved.</para>
        /// </remarks>
        public int DefaultOperationTimeoutMilliseconds { get; set; } = 120_000;

        /// <summary>
        /// Clones the options.
        /// </summary>
        internal MessagingOptions Clone() => new MessagingOptions(this);
    }
}
