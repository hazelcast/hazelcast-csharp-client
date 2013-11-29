namespace Hazelcast.Client.Spi
{
    public delegate void ResponseHandler(IResponseStream stream);

    public interface ResponseHandler2
    {
        /// <exception cref="System.Exception"></exception>
        void Handle(IResponseStream stream);
    }
}