using Hazelcast.Client.Connection;
using Hazelcast.IO;

namespace Hazelcast.Util
{
    public delegate TE FactoryMethod<out TE>();

    public delegate TE DestructorMethod<TE>(TE e);

    public delegate V ConstructorMethod<K, V>(K arg);

    public delegate void Runnable();

    public delegate T Callable<T>();


    /// <exception cref="System.IO.IOException"></exception>
    internal delegate Connection NewConnection(Address address);

    public delegate void Authenticator(IConnection connection);
}