// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Core;
using Hazelcast.Exceptions;

namespace Hazelcast.Security
{
    /// <summary>
    /// Implements a Kerberos <see cref="ICredentialsFactory"/>.
    /// </summary>
    internal sealed class KerberosCredentialsFactory : IResettableCredentialsFactory
    {
        private static IKerberosTokenProvider _tokenProvider;
        private readonly string _spn;
        private ICredentials _credentials;

// disable 'can be removed as value is never read' message
#pragma warning disable IDE0052
        private readonly string _username;
        private readonly string _password;
        private readonly string _domain;
#pragma warning restore IDE0052

        /// <summary>
        /// Initializes a new instance of the <see cref="KerberosCredentialsFactory"/> class.
        /// </summary>
        /// <returns>The application will authenticate to the KDC as the current Windows user.</returns>
        /// <param name="spn">The service principal name.</param>
        public KerberosCredentialsFactory(string spn)
        {
            if (string.IsNullOrWhiteSpace(spn))
                throw new ArgumentException("Value cannot be null nor empty.", nameof(spn));
            _spn = spn;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KerberosCredentialsFactory"/> class.
        /// </summary>
        /// <returns>The application will authenticate to the KDC as the specified domain user.</returns>
        /// <param name="spn">The service principal name.</param>
        /// <param name="username">A username.</param>
        /// <param name="password">A password.</param>
        /// <param name="domain">A domain.</param>
        public KerberosCredentialsFactory(string spn, string username, string password, string domain)
            : this(spn)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Value cannot be null nor empty.", nameof(username));
            _username = username;

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Value cannot be null nor empty.", nameof(password));
            _password = password;

            if (string.IsNullOrWhiteSpace(domain))
                throw new ArgumentException("Value cannot be null nor empty.", nameof(domain));
            _domain = domain;
        }

        private static IKerberosTokenProvider TokenProvider
        {
            get
            {
                if (_tokenProvider != null) return _tokenProvider;

                // locate the token provider - so far we only support our Win32 provider

                var type = Type.GetType("Hazelcast.Security.KerberosTokenProvider, Hazelcast.Net.Win32");
                if (type == null) throw new HazelcastException("Failed to load a Kerberos token provider." +
                                                               " Have you installed the Hazelcast.Net.Win32 NuGet package?");
                _tokenProvider = Activator.CreateInstance(type) as IKerberosTokenProvider;
                if (_tokenProvider == null) throw new HazelcastException("Failed to create a Kerberos token provider.");

                return _tokenProvider;
            }
        }

        /// <inheritdoc />
        public ICredentials NewCredentials()
        {
            if (_credentials != null)
                return _credentials;

            var token = TokenProvider.GetToken(_spn, _username, _password, _domain);
            return new TokenCredentials(token);
        }

        /// <inheritdoc />
        public void Reset()
        {
            _credentials = null;
        }

        /// <inheritdoc />
        public void Dispose()
        { }
    }
}
