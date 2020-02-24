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
using System.Security;
#if !NETSTANDARD
using System.Net;
using System.Security.Principal;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
#else
using Hazelcast.Net.Win32;
#endif

namespace Hazelcast.Security
{
    /// <summary>
    /// Implements a Kerberos <see cref="ICredentialsFactory"/>.
    /// </summary>
    public class KerberosCredentialsFactory : IResettableCredentialsFactory
    {
        // default timeout for getting the token, in seconds. Retrieving the security token involves
        // distributed messaging and we want to avoid exceptions caused by network failures, message
        // loss and other error conditions - we don't want to wait forever either.
        private const int DefaultTimeoutSeconds = 30;

        private string _spn;
        private TimeSpan _timeout;
        private ICredentials _credentials;
        private string _username;
        private string _password;
        private string _domain;

        /// <summary>
        /// Initializes a new instance of the <see cref="KerberosCredentialsFactory"/> class.
        /// </summary>
        /// <remarks>The new instance needs to be fully initialized with the <see cref="Init"/> method.</remarks>
        public KerberosCredentialsFactory()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="KerberosCredentialsFactory"/> class.
        /// </summary>
        /// <returns>The application will authenticate to the KDC as the current Windows user.</returns>
        /// <param name="spn">The service principal name.</param>
        /// <param name="timeoutSeconds">An optional timeout for communicating with the KDC.</param>
        public KerberosCredentialsFactory(string spn, int timeoutSeconds = 0)
        {
            if (string.IsNullOrWhiteSpace(spn))
                throw new ArgumentException("Value cannot be null nor empty.", nameof(spn));
            _spn = spn;

            if (timeoutSeconds < 0)
                throw new ArgumentException("Invalid timeout value.", nameof(timeoutSeconds));
            _timeout = TimeSpan.FromSeconds(timeoutSeconds > 0 ? timeoutSeconds : DefaultTimeoutSeconds);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KerberosCredentialsFactory"/> class.
        /// </summary>
        /// <returns>The application will authenticate to the KDC as the specified domain user.</returns>
        /// <param name="spn">The service principal name.</param>
        /// <param name="username">A username.</param>
        /// <param name="password">A password.</param>
        /// <param name="domain">A domain.</param>
        /// <param name="timeoutSeconds">An optional timeout for communicating with the KDC.</param>
        public KerberosCredentialsFactory(string spn, string username, string password, string domain, int timeoutSeconds = 0)
            : this(spn, timeoutSeconds)
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

            if (properties.TryGetValue("timeout", out var timeoutString))
            {
                if (int.TryParse(timeoutString, out var timeoutSeconds) && timeoutSeconds >= 0)
                    _timeout = TimeSpan.FromSeconds(timeoutSeconds > 0 ? timeoutSeconds : DefaultTimeoutSeconds);
                else
                    throw new InvalidOperationException("Invalid timeout value.");
            }
            else
            {
                _timeout = TimeSpan.FromSeconds(DefaultTimeoutSeconds);
            }

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

            // ReSharper disable once JoinDeclarationAndInitializer
            byte[] tokenBytes;

#if !NETSTANDARD

            // this is the default way to do Kerberos for the .NET Framework
            // and... the Kerberos.NET library is netstandard-2.0, which requires
            // net-461 and we are still aiming at net-40 so we cannot use it here,
            // hence we have to have two different implementations

            // FIXME but now that we have embedded Kerberos.NET maybe we can use it here too?

            var tokenProvider = _username == null
                ? new KerberosSecurityTokenProvider(_spn, TokenImpersonationLevel.Identification)
                : new KerberosSecurityTokenProvider(_spn, TokenImpersonationLevel.Identification, new NetworkCredential(_username, _password, _domain));

            try
            {
                if (!(tokenProvider.GetToken(_timeout) is KerberosRequestorSecurityToken token)) 
                    throw new InvalidOperationException("Token is not KerberosRequestorSecurityToken.");
                tokenBytes = token.GetRequest();
            }
            catch (Exception e)
            {
                throw new SecurityException("Failed to get a Kerberos token.", e);
            }
#else

            // the default way above does not work with .NET Core, so, we are
            // code from the Kerberos.NET library. we have to use SSPI to work with
            // the current Windows user, as per
            //
            // https://github.com/SteveSyfuhs/Kerberos.NET/issues/89
            //
            // "However, if you're looking to create a KerberosClient and just grab credentials
            // off the currently logged on user in Windows then you should just use SSPI.
            // There's no way to extract the credentials from Windows (by design) to use in the initial
            // authentication of the client, and it's not intended to be a wrapper around SSPI."

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

            using (context)
            {
                tokenBytes = context.RequestToken();
            }
#endif

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