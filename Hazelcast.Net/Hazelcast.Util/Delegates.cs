using Hazelcast.Client.Connection;
using Hazelcast.Client.Protocol;
using Hazelcast.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.Util
{
    public delegate TE FactoryMethod<out TE>();

    public delegate TE DestructorMethod<TE>(TE e);

    public delegate V ConstructorMethod<K, V>(K arg);

    public delegate void Runnable();

    public delegate T Callable<T>();


    /// <exception cref="System.IO.IOException"></exception>
    internal delegate ClientConnection NewConnection(Address address);

    internal delegate void Authenticator(ClientConnection connection);

    public delegate void DistributedEventHandler(IClientMessage eventMessage);

    public delegate string DecodeStartListenerResponse(IClientMessage requestMessage);
    public delegate bool DecodeStopListenerResponse(IClientMessage requestMessage);

}