using Hazelcast.IO;

namespace Hazelcast.Client.Spi
{
    public interface IClientInvocationService
    {
        /// <exception cref="System.Exception"></exception>
        T InvokeOnRandomTarget<T>(object request);

        /// <exception cref="System.Exception"></exception>
        T InvokeOnTarget<T>(object request, Address target);

        /// <exception cref="System.Exception"></exception>
        T InvokeOnKeyOwner<T>(object request, object key);

        /// <exception cref="System.Exception"></exception>
        void InvokeOnRandomTarget(object request, ResponseHandler handler);

        /// <exception cref="System.Exception"></exception>
        void InvokeOnTarget(object request, Address target, ResponseHandler handler);

        /// <exception cref="System.Exception"></exception>
        void InvokeOnKeyOwner(object request, object key, ResponseHandler handler);
    }
}