using Hazelcast.Security;

namespace Hazelcast.Clustering
{
    public static class AuthenticationOptionsExtensions
    {
        /// <summary>
        /// Configures Kerberos as the authentication mechanism.
        /// </summary>
        /// <param name="spn">The service principal name of the Hazelcast cluster.</param>
        /// <returns>The security options.</returns>
        public static AuthenticationOptions ConfigureKerberosCredentials(this AuthenticationOptions options, string spn)
        {
            options.CredentialsFactory.Creator = () => new KerberosCredentialsFactory(spn);
            return options;
        }
    }
}
