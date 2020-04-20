using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Hazelcast.Aggregators;
using Hazelcast.Predicates;
using Hazelcast.Projections;
using Hazelcast.Serialization;

namespace Hazelcast
{
    /// <summary>
    /// Provides a lightweight service container.
    /// </summary>
    /// <remarks>
    /// <para>This class provides a lightweight service container, which is actually more a service
    /// locator, which is an anti-pattern. It is going to be used while we refactor the solution.</para>
    /// <para>The purpose here is to reduce the coupling between namespaces, as well as keeping the usage
    /// of non-CLS-compliant code internal. For instance, Microsoft's logging abstractions are not CLS
    /// compliant. The only way we can provide for users to declare which logger to use, is via this
    /// kind of service provider abstraction, as we cannot expose anything through configuration.</para>
    /// </remarks>
    public static class Services
    {
        private static readonly ConcurrentDictionary<Type, Func<object>> Factories = new ConcurrentDictionary<Type, Func<object>>();

        private static readonly ConcurrentDictionary<Type, object> Instances = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// Initializes the <see cref="Services"/> class.
        /// </summary>
        static Services()
        {
            // register the serializer hooks collection
            // the purpose of this is to break cross-dependencies between namespaces
            var serializerHooks = new SerializerHooks();
            serializerHooks.Add<PredicateDataSerializerHook>();
            serializerHooks.Add<AggregatorDataSerializerHook>();
            serializerHooks.Add<ProjectionDataSerializerHook>();
            Instances[typeof(SerializerHooks)] = serializerHooks;
        }

        /// <summary>
        /// Gets the service getter.
        /// </summary>
        public static ServiceGetter Get { get; } = new ServiceGetter();

        /// <summary>
        /// Represents the service getter.
        /// </summary>
        public sealed class ServiceGetter
        { }

        /// <summary>
        /// Resets services. This method is provided for tests.
        /// </summary>
        internal static void Reset()
        {
            Factories.Clear();
            Instances.Clear();
        }

        /// <summary>
        /// Registers a service with an implementation instance.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="instance">The implementation instance.</param>
        public static void Register<TService>(TService instance)
            where TService : class
        {
            Instances[typeof(TService)] = instance;
        }

        /// <summary>
        /// Registers a service with an implementation factory.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="factory">The implementation factory.</param>
        public static void Register<TService>(Func<TService> factory)
            where TService : class
        {
            Factories[typeof(TService)] = factory;
        }

        /// <summary>
        /// Gets an instance of a service.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <returns>The implementation instance.</returns>
        /// <exception cref="InvalidOperationException">Occurs when no instance could be found.</exception>
        internal static TService GetInstance<TService>()
        {
            if (TryGetInstance(typeof(TService), out var instance))
                return (TService)instance;

            throw new InvalidOperationException($"Cannot get an instance of type {typeof(TService)}.");
        }

        /// <summary>
        /// Tries to get an instance of a service.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <returns>The implementation instance, or default(TService) if no instance could be found.</returns>
        internal static TService TryGetInstance<TService>()
        {
            return TryGetInstance(typeof(TService), out var instance) ? (TService)instance : default;
        }

        /// <summary>
        /// Tries to get an instance of a service.
        /// </summary>
        /// <param name="type">The type of the service.</param>
        /// <param name="instance">The implementation instance.</param>
        /// <returns>Whether an instance could be found.</returns>
        private static bool TryGetInstance(Type type, out object instance)
        {
            instance = Instances.GetOrAdd(type, t =>
            {
                if (!Factories.TryGetValue(type, out var factory))
                    return default;

                var o = factory();
                return type.IsInstanceOfType(o) ? o : default;
            });

            return instance != null;
        }

        /// <summary>
        /// Represents a collection of <see cref="ISerializerHook{T}"/> types.
        /// </summary>
        public sealed class SerializerHooks
        {
            private readonly List<Type> _types = new List<Type>();

            /// <summary>
            /// Adds a type.
            /// </summary>
            /// <param name="type">The type.</param>
            public void Add(Type type) => _types.Add(type);

            /// <summary>
            /// Adds a type.
            /// </summary>
            /// <typeparam name="T">The type.</typeparam>
            public void Add<T>() => Add(typeof(T));

            /// <summary>
            /// Gets the types.
            /// </summary>
            public IEnumerable<Type> Types => _types;

            /// <summary>
            /// Gets the hooks.
            /// </summary>
            public IEnumerable<IDataSerializerHook> Hooks => _types.Select(Activator.CreateInstance).Cast<IDataSerializerHook>();
        }
    }
}
