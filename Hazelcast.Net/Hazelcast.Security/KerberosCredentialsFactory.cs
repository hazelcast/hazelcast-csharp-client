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
using Hazelcast.Net.Win32;

namespace Hazelcast.Security
{
    /// <summary>
    /// Implements a Kerberos <see cref="ICredentialsFactory"/>.
    /// </summary>
    public class KerberosCredentialsFactory : IResettableCredentialsFactory
    {
        private string _spn;
        private ICredentials _credentials;
        private string _username;
        private string _password;
        private string _domain;

        /// <summary>
        /// Initializes a new instance of the <see cref="KerberosCredentialsFactory"/> class.
        /// </summary>
        /// <remarks>The new instance needs to be fully initialized with the <see cref="Init"/> method.</remarks>
        // ReSharper disable once UnusedMember.Global - created by reflection
        public KerberosCredentialsFactory()
        { }

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

        /// <inheritdoc />
        public void Init(IDictionary<string, string> properties)
        {
            if (!properties.TryGetValue("spn", out _spn) || string.IsNullOrWhiteSpace(_spn))
                throw new InvalidOperationException("Missing Service Principal Name.");

            if (!properties.TryGetValue("username", out _username) ||
                !properties.TryGetValue("password", out _password) ||
                !properties.TryGetValue("domain", out _domain))
            {
                _username = _password = _domain = null;
            }
        }

        /// <inheritdoc />
        public ICredentials NewCredentials()
        {
            if (_credentials != null)
                return _credentials;

            // the original way (using KerberosSecurityTokenProvider) did not work with .NET Core,
            // so we use code from the Kerberos.NET library - using SSPI to work with the current
            // Windows user, as per 
            //
            // https://github.com/SteveSyfuhs/Kerberos.NET/issues/89
            //
            // "However, if you're looking to create a KerberosClient and just grab credentials
            // off the currently logged on user in Windows then you should just use SSPI.
            // There's no way to extract the credentials from Windows (by design) to use in the initial
            // authentication of the client, and it's not intended to be a wrapper around SSPI."
            //
            // and that same code works both for .NET Framework and .NET Core

            SspiContext context;
            if (_username == null)
            {
                context = new SspiContext(_spn);
            }
            else
            {
                var credential = new SuppliedCredential(_username, _password, _domain);
                context = new SspiContext(_spn, credential);
            }

            byte[] tokenBytes;
            using (context)
            {
                tokenBytes = context.RequestToken();
            }

            return _credentials = new KerberosCredentials(tokenBytes);
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