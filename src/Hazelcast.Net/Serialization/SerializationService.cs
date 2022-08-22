﻿// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hazelcast.Core;
using Hazelcast.Partitioning.Strategies;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Serialization
{
    internal sealed partial class SerializationService : IDisposable
    {
        public const byte SerializerVersion = 1;
        private const int ConstantSerializersCount = SerializationConstants.ConstantSerializersArraySize;

        private static readonly IPartitioningStrategy NullPartitioningStrategy = new NullPartitioningStrategy();
        private static MethodInfo _createSerializerAdapter;

        private readonly SerializationOptions _options;
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private readonly ILogger _logger;
        private readonly IPartitioningStrategy _globalPartitioningStrategy;
        private readonly int _initialOutputBufferSize;
        private readonly bool _enableClrSerialization;

        // We have two groups of serializers:
        //   'constant' are built-in immutable serializers
        //   'custom' are user-level serializers
        //
        // Serializers are registered in maps:
        //   _constantById   : constant type-id -> ISerializerAdapter  (with positive type-id because it's an array)
        //   _constantByType : constant type    -> ISerializerAdapter
        //   _customById     : custom type-id   -> ISerializerAdapter
        //   _customByType   : custom type      -> ISerializerAdapter
        //
        private readonly ISerializerAdapter[] _constantById = new ISerializerAdapter[ConstantSerializersCount];
        private readonly ConcurrentDictionary<Type, ISerializerAdapter> _constantByType = new ConcurrentDictionary<Type, ISerializerAdapter>();
        private readonly ConcurrentDictionary<int, ISerializerAdapter> _customById = new ConcurrentDictionary<int, ISerializerAdapter>();
        private readonly ConcurrentDictionary<Type, ISerializerAdapter> _customByType = new ConcurrentDictionary<Type, ISerializerAdapter>();

        // the adapters
#pragma warning disable CA2213 // Disposable fields should be disposed - they are
        private readonly ISerializerAdapter _nullSerializerAdapter; // null objects serialization
        private readonly ISerializerAdapter _globalSerializerAdapter; // global serializer
        private readonly ISerializerAdapter _dataSerializerAdapter; // identified data serialization
        private readonly ISerializerAdapter _portableSerializerAdapter; // portable serialization
        private readonly ISerializerAdapter _serializableSerializerAdapter; // CLR serialization
#pragma warning restore CA2213 // Disposable fields should be disposed

        // some serializers and stuff that we need to have here
#pragma warning disable CA2213 // Disposable fields should be disposed - they are
        private readonly PortableContext _portableContext;
        private readonly PortableSerializer _portableSerializer;
#pragma warning restore CA2213 // Disposable fields should be disposed

        internal SerializationService(
            SerializationOptions options,
            Endianness endianness, int portableVersion,
            IDictionary<int, IDataSerializableFactory> dataSerializableFactories,
            IDictionary<int, IPortableFactory> portableFactories,
            ICollection<IClassDefinition> portableClassDefinitions,
            SerializerHooks hooks,
            IEnumerable<ISerializerDefinitions> definitions,
            bool validatePortableClassDefinitions, IPartitioningStrategy partitioningStrategy,
            int initialOutputBufferSize,
            ILoggerFactory loggerFactory)
        {
            _options = options;
            Endianness = endianness;
            _globalPartitioningStrategy = partitioningStrategy;
            _enableClrSerialization = true;
            _initialOutputBufferSize = initialOutputBufferSize;

            _logger = loggerFactory.CreateLogger<SerializationService>();

            // note: lookup by type has shortcuts for some of the "known" serializers below, so we
            // could avoid adding them to _constantByType -- however, we need them in _constantById,
            // and it's cleaner to all treat them the same regardless of that optimization.

            // serialization methods as defined in the reference manual are:
            // - Serializable - Java built-in serialization, implemented as .NET serialization
            // - Externalizable - Java thing, not supported by .NET
            // - DataSerializable - old thing that is superseded by IdentifiedDataSerializable?
            // - IdentifiedDataSerializable - identified data serialization, requires serializer & factory
            // - Portable - portable serialization, requires serializer & factory
            // - Custom - custom serialization, requires serializer & plug-in
            // - Compact - compact serialization

            // Registers the constant 'identified data serializer', which implements identified data serialization.
            //   IDataSerializableHook each instantiate one single IDataSerializableFactory that is registered
            //   IDataSerializableFactory can instantiate new objects from their type-id (when de-serializing)
            //   so, dataSerializableFactories directly provides factories whereas hooks provide factory factories
            //   then object itself implements the (de) serialization methods
            var dataSerializer = new DataSerializer(hooks.Hooks, dataSerializableFactories, loggerFactory);
            _dataSerializerAdapter = CreateSerializerAdapter<IIdentifiedDataSerializable>(dataSerializer);
            RegisterConstantSerializer(_dataSerializerAdapter, typeof(IIdentifiedDataSerializable));

            // Registers the constant 'serializable serializer', which implements CLR BinaryFormatter
            // serialization of objects marked with the [Serializable] attributes.
            _serializableSerializerAdapter = CreateSerializerAdapter<object>(new SerializableSerializer());

            // NOTE
            // In Java the javaSerializerAdapter, which would correspond to the _serializableSerializerAdapter here,
            // is 'safeRegister'-ed somehow which means it is registered as a custom serializer, not a constant one.
            // In .NET the _serializableSerializerAdapter was also force-registered as a custom serializer although
            // its type-id is <0, and that would cause TryRegisterCustomSerializer to throw.
            // I cannot find any reason why this constant serializer would need to be registered as custom, breaking
            // all internal conventions, therefore I am not following Java here and I am registering it as constant.
            // This also means that LookupConstantSerializer does not need to exclude the CsharpClrSerializationType
            // anymore as, again, it just is a constant serializer to begin with.
            RegisterConstantSerializer(_serializableSerializerAdapter);

            // Invoke ISerializerDefinitions, which are classes that can register more serializers.
            // Mostly, this is going to register all the primitive constant serializers.
            foreach (var definition in definitions) definition.AddSerializers(this);

            // Registers the constant 'null serializer', which handles serialization of NULL references.
            _nullSerializerAdapter = CreateSerializerAdapter<object>(new NullSerializer());
            RegisterConstantSerializer(_nullSerializerAdapter);

            // Registers the constant 'portable serializer', which implements portable serialization.
            _portableContext = new PortableContext(this, portableVersion);
            _portableSerializer = new PortableSerializer(_portableContext, portableFactories);
            _portableSerializerAdapter = CreateSerializerAdapter<IPortable>(_portableSerializer);
            RegisterConstantSerializer(_portableSerializerAdapter, typeof(IPortable));
            RegisterPortableClassDefinitions(portableClassDefinitions, validatePortableClassDefinitions);

            // Registers the global serializer, if any.
            var globalSerializer = _options.GlobalSerializer;
            if (globalSerializer.IsConfigured)
            {
                _globalSerializerAdapter = CreateSerializerAdapter<object>(globalSerializer.Service);
                _ = TryRegisterCustomSerializer(_globalSerializerAdapter);
                _enableClrSerialization &= !globalSerializer.OverrideClrSerialization;
            }

            // Registers serializers defined in options.
            foreach (var serializer in _options.Serializers)
                RegisterCustomSerializer(serializer.Service, serializer.SerializedType);
        }

#pragma warning disable CA1822 // Mark members as static - might not remain constant forever
        public byte GetVersion() => SerializerVersion;
#pragma warning restore CA1822 // Mark members as static

        public Endianness Endianness { get; }

        /// <summary>
        /// Creates an <see cref="ISerializerAdapter"/> for an <see cref="ISerializer"/>.
        /// </summary>
        /// <param name="type">The type of the objects handled by the <paramref name="serializer"/>.</param>
        /// <param name="serializer">The serializer.</param>
        /// <returns>The adapter.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="type"/> or the <paramref name="serializer"/> is <c>null</c>.</exception>
        /// <exception cref="ServiceFactoryException">Cannot instantiate the adapter class (internal error).</exception>
        private ISerializerAdapter CreateSerializerAdapter(Type type, ISerializer serializer)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            // easiest way to create generic classes based upon 'type' is to use the proper generic method
            // and then for performance purposes, we can statically cache the method info - so we still pay
            // for reflection on each call, but not the full price.
            if (_createSerializerAdapter == null)
            {
                var method = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                    .FirstOrDefault(x => x.Name == nameof(CreateSerializerAdapter) && x.IsGenericMethod);
                if (method == null)
                    throw new ServiceFactoryException($"Internal error (failed to get {nameof(CreateSerializerAdapter)} method).");
                _createSerializerAdapter = method.GetGenericMethodDefinition();
            }

            var createSerializerAdapter = _createSerializerAdapter.MakeGenericMethod(type);
            return (ISerializerAdapter)createSerializerAdapter.Invoke(this, new object[] { serializer });
        }

        /// <summary>
        /// Creates an <see cref="ISerializerAdapter"/> for an <see cref="ISerializer"/>.
        /// </summary>
        /// <typeparam name="T">The type of the objects handled by the <paramref name="serializer"/>.</typeparam>
        /// <param name="serializer">The serializer.</param>
        /// <returns>The adapter.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="serializer"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="serializer"/> does not implement <see cref="IStreamSerializer{T}"/>
        /// nor <see cref="IByteArraySerializer{T}"/>.</exception>
        /// <remarks>
        /// <para>The created adapter is registered for being disposed.</para>
        /// </remarks>
        private ISerializerAdapter CreateSerializerAdapter<T>(ISerializer serializer)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            ISerializerAdapter adapter = serializer switch
            {
                IStreamSerializer<T> streamSerializer => new StreamSerializerAdapter<T>(streamSerializer),
                IByteArraySerializer<T> arraySerializer => new ByteArraySerializerAdapter<T>(arraySerializer),
                _ => throw new ArgumentException("Serializer must implement either IStreamSerializer<T> or IByteArraySerializer<T>.")
            };
            return Using(adapter);
        }

        // IDisposable
        //
        // All ISerializer implementations are IDisposable and must be disposed as they may manage some pools. Conveniently,
        // all ISerializerAdapter are also IDisposable and are required to dispose their inner ISerializer. So all we need to
        // do is make sure we dispose every adapter.
        //
        // Every adapter created by CreateSerializedAdapter<T> is automatically registered for being disposed.
        // Any other adapter must be explicitly registered. Use the 'Using' method to register a disposable.

        /// <summary>
        /// Registers an <see cref="IDisposable"/> implementation to be disposed when this class is disposed.
        /// </summary>
        /// <typeparam name="T">The actual type of the implementation.</typeparam>
        /// <param name="disposable">The implementation.</param>
        /// <returns>The implementation.</returns>
        private T Using<T>(T disposable) where T : IDisposable
        {
            _disposables.Add(disposable);
            return disposable;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                try
                {
                    disposable.Dispose();
                }
                catch
                {
                    _logger.LogError("Failed to dispose {Disposable}.", disposable);
                    // and swallow - there is nothing much we can do at that point
                }
            }
        }
    }
}
