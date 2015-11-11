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

namespace Hazelcast.Security
{
    /// <summary>
    ///     ICredentials is a container object for endpoint (Members and Clients)
    ///     security attributes.
    /// </summary>
    /// <remarks>
    ///     ICredentials is a container object for endpoint (Members and Clients)
    ///     security attributes.
    ///     <p />
    ///     It is used on authentication process by
    /// </remarks>
    public interface ICredentials
    {
        /// <summary>Returns IP address of endpoint.</summary>
        /// <remarks>Returns IP address of endpoint.</remarks>
        /// <returns>endpoint address</returns>
        string GetEndpoint();

        /// <summary>Returns principal of endpoint.</summary>
        /// <remarks>Returns principal of endpoint.</remarks>
        /// <returns>endpoint principal</returns>
        string GetPrincipal();

        /// <summary>Sets IP address of endpoint.</summary>
        /// <remarks>Sets IP address of endpoint.</remarks>
        /// <param name="endpoint">address</param>
        void SetEndpoint(string endpoint);
    }
}