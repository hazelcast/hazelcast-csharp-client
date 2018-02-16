// Copyright (c) 2008-2018, Hazelcast, Inc. All Rights Reserved.
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

namespace Hazelcast.Core
{
    /// <summary>
    ///     IClientListener allows to get notified when a
    ///     <see cref="IClient">IClient</see>
    ///     is connected to
    ///     or disconnected from cluster.
    /// </summary>
    /// <seealso cref="IClient">IClient</seealso>
    /// <seealso cref="IClientService.AddClientListener(IClientListener)">IClientService.AddClientListener(IClientListener)</seealso>
    public interface IClientListener : IEventListener
    {
        /// <summary>Invoked when a new client is connected.</summary>
        /// <remarks>Invoked when a new client is connected.</remarks>
        /// <param name="client">IClient instance</param>
        void ClientConnected(IClient client);

        /// <summary>Invoked when a new client is disconnected.</summary>
        /// <remarks>Invoked when a new client is disconnected.</remarks>
        /// <param name="client">IClient instance</param>
        void ClientDisconnected(IClient client);
    }

    public interface IEventListener
    {
    }
}