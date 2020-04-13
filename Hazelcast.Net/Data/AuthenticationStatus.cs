namespace Hazelcast.Data
{
    /// <summary>
    /// Represents the result of a client authentication attempt.
    /// </summary>
    internal enum AuthenticationStatus
    {
        /// <summary>
        /// The authentication was successful and the client is now authenticated.
        /// </summary>
        Authenticated = 0,

        /// <summary>
        /// The authentication failed because the credentials were invalid.
        /// </summary>
        CredentialsFailed = 1,

        /// <summary>
        /// The authentication failed because the serialization version did not match what the server expected.
        /// </summary>
        SerializationVersionMismatch = 2,

        /// <summary>
        /// The authentication failed because the client is not allowed in the cluster.
        /// </summary>
        NotAllowedInCluster = 3
    }
}
