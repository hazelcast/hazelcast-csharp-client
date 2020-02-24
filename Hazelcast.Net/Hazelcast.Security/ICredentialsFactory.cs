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
using Hazelcast.Config;

namespace Hazelcast.Security
{
    /// <summary>
    /// Defines a factory of <see cref="ICredentials"/>.
    /// </summary>
    public interface ICredentialsFactory : IDisposable
    {
        /// <summary>
        /// Initializes the factory.
        /// </summary>
        /// <remarks>
        /// This method is only invoked when the factory instance is created by Hazelcast from a class
        /// type provided in <see cref="CredentialsFactoryConfig"/>. It is *not* invoked when an
        /// <see cref="ICredentialsFactory"/> instance is provided.
        /// </remarks>
        /// <param name="properties">Factory properties defined in configuration.</param>
        void Init(IDictionary<string, string> properties);

        /// <summary>
        /// Creates and returns a new <see cref="ICredentials"/> object.
        /// </summary>
        /// <remarks>
        /// This method is invoked any time a new connection is authenticated.
        /// </remarks>
        /// <returns>The new credentials object.</returns>
        ICredentials NewCredentials();
    }
}