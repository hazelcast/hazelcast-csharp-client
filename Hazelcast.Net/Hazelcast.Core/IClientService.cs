// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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

using System.Collections.Generic;

namespace Hazelcast.Core
{
    /// <summary>
    ///     IClientService allows to query connected
    ///     <see cref="IClient">IClient</see>
    ///     s and
    ///     attach/detach
    ///     <see cref="IClientListener">IClientListener</see>
    ///     s to listen connection events.
    /// </summary>
    /// <seealso cref="IClient">IClient</seealso>
    /// <seealso cref="IClientListener">IClientListener</seealso>
    public interface IClientService
    {
        /// <param name="clientListener">IClientListener</param>
        /// <returns>returns registration id.</returns>
        string AddClientListener(IClientListener clientListener);

        /// <summary>Returns all connected clients to this member.</summary>
        /// <remarks>Returns all connected clients to this member.</remarks>
        /// <returns>all connected clients to this member.</returns>
        ICollection<IClient> GetConnectedClients();

        /// <param name="registrationId">Id of listener registration.</param>
        /// <returns>true if registration is removed, false otherwise</returns>
        bool RemoveClientListener(string registrationId);
    }
}