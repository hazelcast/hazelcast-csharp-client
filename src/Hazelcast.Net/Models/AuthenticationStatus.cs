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

namespace Hazelcast.Models
{
    /// <summary>
    /// Represents the result of a client authentication attempt.
    /// </summary>
    internal enum AuthenticationStatus
    {
        /// <summary>
        /// The authentication was successful and the client is now authenticated.
        /// </summary>
        Authenticated = 0,

        /// <summary>
        /// The authentication failed because the credentials were invalid.
        /// </summary>
        CredentialsFailed = 1,

        /// <summary>
        /// The authentication failed because the serialization version did not match what the server expected.
        /// </summary>
        SerializationVersionMismatch = 2,

        /// <summary>
        /// The authentication failed because the client is not allowed in the cluster.
        /// </summary>
        NotAllowedInCluster = 3
    }
}
