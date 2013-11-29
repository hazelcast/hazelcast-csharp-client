namespace Hazelcast.Client.Connection
{
    public interface IAuthenticator
    {
        /// <exception cref="Hazelcast.Client.AuthenticationException"></exception>
        /// <exception cref="System.IO.IOException"></exception>
        void Auth(IConnection connection);
    }
}