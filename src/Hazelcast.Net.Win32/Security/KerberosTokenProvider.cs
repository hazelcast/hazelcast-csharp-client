// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
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
using Hazelcast.Security.Win32;

namespace Hazelcast.Security
{
    /// <summary>
    /// Implements <see cref="IKerberosTokenProvider"/> for Windows.
    /// </summary>
    public class KerberosTokenProvider : IKerberosTokenProvider
    {
        // https://github.com/SteveSyfuhs/Kerberos.NET/issues/89
        //
        // "However, if you're looking to create a KerberosClient and just grab credentials
        // off the currently logged on user in Windows then you should just use SSPI.
        // There's no way to extract the credentials from Windows (by design) to use in the initial
        // authentication of the client, and it's not intended to be a wrapper around SSPI."
        //
        // and that same code works both for .NET Framework and .NET Core

        public byte[] GetToken(string spn, string username, string password, string domain)
        {
            SspiContext context;
            if (username == null)
            {
                context = new SspiContext(spn);
            }
            else
            {
                var credential = new SuppliedCredential(username, password, domain);
                context = new SspiContext(spn, credential);
            }

            using (context)
            {
                return context.RequestToken();
            }

            // kept for reference below, the way to do it that *only* works with .NET Framework

            /*

            using System.IdentityModel.Selectors;
            using System.IdentityModel.Tokens;
            using System.Net;
            using System.Security;
            using System.Security.Principal;

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

            */
        }
    }
}
