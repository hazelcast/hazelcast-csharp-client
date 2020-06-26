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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Data;
using Hazelcast.Serialization;

namespace Hazelcast.Clustering
{
    /// <summary>
    /// Defines a service that can authenticate a client.
    /// </summary>
    internal interface IAuthenticator
    {
        /// <summary>
        /// Authenticates the client.
        /// </summary>
        /// <param name="client">The client to authenticate.</param>
        /// <param name="clusterName">The cluster name, as assigned by the client.</param>
        /// <param name="clusterClientId">The cluster unique identifier, as assigned by the client.</param>
        /// <param name="clusterClientName">The cluster client name, as assigned by the client.</param>
        /// <param name="labels">The client labels.</param>
        /// <param name="serializationService">The serialization service.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that will complete when the client is authenticated.</returns>
        ValueTask<AuthenticationResult> AuthenticateAsync(ClientConnection client, string clusterName, Guid clusterClientId, string clusterClientName, ISet<string> labels, ISerializationService serializationService, CancellationToken cancellationToken);
    }
}
