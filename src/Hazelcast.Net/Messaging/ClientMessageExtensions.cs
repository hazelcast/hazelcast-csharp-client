// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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
    /// Provides extension methods to the <see cref="ClientMessage"/> class.
    /// </summary>
    internal static class ClientMessageExtensions
    {
        /// <summary>
        /// Clones the message with a new correlation identifier.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="correlationId">The new correlation identifier.</param>
        /// <returns>A clone of the original message with a new correlation identifier.</returns>
        /// <remarks>
        /// <para>The first frame of the original message is deep-cloned because it carries the correlation
        /// identifier and therefore is modified. Other frames are shallow-cloned because they are not
        /// modified.</para>
        /// </remarks>
        public static ClientMessage CloneWithNewCorrelationId(this ClientMessage message, long correlationId)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var clone = new ClientMessage();
            var first = true;

            foreach (var frame in message)
            {
                if (first)
                {
                    // deep-clone the first frame, we're going to modify it with the new correlation id
                    clone.Append(frame.DeepClone());
                    first = false;
                }
                else
                {
                    // shallow-clone the other frames, we're not modifying them
                    clone.Append(frame.ShallowClone());
                }
            }

            clone.OperationName = message.OperationName;
            clone.IsRetryable = message.IsRetryable;
            clone.CorrelationId = correlationId;

            // everything else (flags...) belong to the first frame = cloned

            return clone;
        }
    }
}
