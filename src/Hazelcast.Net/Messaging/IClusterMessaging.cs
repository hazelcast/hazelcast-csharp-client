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

using System.Threading;
using System.Threading.Tasks;

namespace Hazelcast.Messaging
{
    /// <summary>
    /// Defines the cluster messaging service.
    /// </summary>
    internal interface IClusterMessaging
    {
        /// <summary>
        /// Triggers before a message is sent.
        /// </summary>
        Func<ValueTask> SendingMessage { get; set; }

        /// <summary>
        /// Sends a message to a random member.
        /// </summary>
        /// <param name="requestMessage">The message to send.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The response message.</returns>
        Task<ClientMessage> SendAsync(ClientMessage requestMessage, CancellationToken cancellationToken);

        /// <summary>
        /// Sends a message to a random member.
        /// </summary>
        /// <param name="requestMessage">The message to send.</param>
        /// <param name="raiseEvents">Whether to raise events.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The response message.</returns>
        Task<ClientMessage> SendAsync(ClientMessage requestMessage, bool raiseEvents, CancellationToken cancellationToken);
    }
}
