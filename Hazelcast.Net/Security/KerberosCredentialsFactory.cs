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
#if !NETSTANDARD
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Net;
using System.Security;
using System.Security.Principal;
#endif

namespace Hazelcast.Security
{
    /// <summary>
    /// Implements a Kerberos <see cref="ICredentialsFactory"/>.
    /// </summary>
    public class KerberosCredentialsFactory : IResettableCredentialsFactory
    {
        private string _spn;
        private ICredentials _credentials;

#if NETSTANDARD
// disable 'can be removed as value is never read' message
#pragma warning disable IDE0052
        private string _username;
        private string _password;
        private string _domain;
#pragma warning restore IDE0052
#else
        private string _username;
        private string _password;
        private string _domain;
#endif

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

            // this is the default way to do Kerberos for the .NET Framework, which is
            // CLS-compliant and safe, but is not supported by .NET Core - however,
            // .NET Core methods require Win32 P/Invoke and/or a reference to an external
            // library - so for the time being, we don't support Kerberos with .NET Core.
            //
            // (see the project git history for how to do it, though)

#if NETSTANDARD
            throw new NotSupportedException("Kerberos is not supported on .NET Core.");
#else
            var tokenProvider = _username == null
                ? new KerberosSecurityTokenProvider(_spn, TokenImpersonationLevel.Identification)
                : new KerberosSecurityTokenProvider(_spn, TokenImpersonationLevel.Identification, new NetworkCredential(_username, _password, _domain));

            var timeout = TimeSpan.FromSeconds(30);

            byte[] tokenBytes;
            try
            {
                if (!(tokenProvider.GetToken(timeout) is KerberosRequestorSecurityToken token))
                    throw new InvalidOperationException("Token is not KerberosRequestorSecurityToken.");
                tokenBytes = token.GetRequest();
            }
            catch (Exception e)
            {
                throw new SecurityException("Failed to get a Kerberos token.", e);
            }

            return _credentials = new KerberosCredentials(tokenBytes);
#endif
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