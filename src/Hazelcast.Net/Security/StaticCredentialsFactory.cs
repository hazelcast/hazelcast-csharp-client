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

namespace Hazelcast.Security
{
    /// <summary>
    /// Provides an implementation of <see cref="ICredentialsFactory"/> that returns a static <see cref="ICredentials"/> instance.
    /// </summary>
    public class StaticCredentialsFactory : ICredentialsFactory
    {
        private readonly ICredentials _credentials;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticCredentialsFactory"/> class.
        /// </summary>
        /// <param name="credentials">The credentials.</param>
        public StaticCredentialsFactory(ICredentials credentials)
        {
            _credentials = credentials;
        }

        /// <inheritdoc />
        public ICredentials NewCredentials()
        {
            return _credentials;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes resources.
        /// </summary>
        /// <param name="disposing">true when disposing deterministically.</param>
        protected virtual void Dispose(bool disposing)
        { }
    }
}
