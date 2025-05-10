﻿// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
    /// Provides Kerberos tokens.
    /// </summary>
    public interface IKerberosTokenProvider
    {
        /// <summary>
        /// Gets the token corresponding to a SPN.
        /// </summary>
        /// <param name="spn">The SPN.</param>
        /// <param name="username">An optional username.</param>
        /// <param name="password">An optional password.</param>
        /// <param name="domain">An optional domain.</param>
        /// <returns>The token bytes.</returns>
        byte[] GetToken(string spn, string username, string password, string domain);
    }
}
