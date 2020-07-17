using Hazelcast.Security.Win32;

namespace Hazelcast.Security
{
    public static class KerberosTokenProvider
    {
        // https://github.com/SteveSyfuhs/Kerberos.NET/issues/89
        //
        // "However, if you're looking to create a KerberosClient and just grab credentials
        // off the currently logged on user in Windows then you should just use SSPI.
        // There's no way to extract the credentials from Windows (by design) to use in the initial
        // authentication of the client, and it's not intended to be a wrapper around SSPI."
        //
        // and that same code works both for .NET Framework and .NET Core

        public static byte[] GetToken(string spn, string username, string password, string domain)
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
