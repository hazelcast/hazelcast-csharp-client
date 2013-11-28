namespace Hazelcast.Core
{
    /// <summary>Container managed context, such as Spring, Guice and etc.</summary>
    /// <remarks>Container managed context, such as Spring, Guice and etc.</remarks>
    public interface IManagedContext
    {
        /// <summary>Initialize the given object instance.</summary>
        /// <remarks>
        ///     Initialize the given object instance.
        ///     This is intended for repopulating select fields and methods for deserialized instances.
        ///     It is also possible to proxy the object, e.g. with AOP proxies.
        /// </remarks>
        /// <param name="obj">Object to initialize</param>
        /// <returns>the initialized object to use</returns>
        object Initialize(object obj);
    }
}