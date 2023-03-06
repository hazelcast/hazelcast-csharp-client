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

using System;
using Hazelcast.Messaging;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Represents the event data corresponding to an exception thrown while handling a distributed event.
    /// </summary>
    internal class DistributedEventExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedEventExceptionEventArgs"/> class.
        /// </summary>
        /// <param name="exception">The exception that was thrown by the event handler.</param>
        /// <param name="message">The client event message that was being processed when the exception was thrown.</param>
        internal DistributedEventExceptionEventArgs(Exception exception, ClientMessage message)
        {
            Exception = exception;
            Message = message;
        }

        /// <summary>
        /// Gets the exception that was thrown by the event handler.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Whether the exception has been handled by user code.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Gets the client event message that was being processed when the exception was thrown.
        /// </summary>
        internal ClientMessage Message { get; }
    }
}
