// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Partitioning.Strategies;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Serialization
{
    internal class SerializationService : IDisposable
    {
        public const byte SerializerVersion = 1;
        private const int ConstantSerializersSize = SerializationConstants.ConstantSerializersArraySize;

        private static readonly IPartitioningStrategy NullPartitioningStrategy = new NullPartitioningStrategy();
        private readonly ILogger _logger;

        private readonly ISerializerAdapter[] _constantTypeIds = new ISerializerAdapter[ConstantSerializersSize];

        private readonly Dictionary<Type, ISerializerAdapter> _constantTypesMap =
            new Dictionary<Type, ISerializerAdapter>(ConstantSerializersSize);

        private readonly ISerializerAdapter _dataSerializerAdapter;
        private volatile ISerializerAdapter _global;

        private readonly ConcurrentDictionary<int, ISerializerAdapter> _idMap =
            new ConcurrentDictionary<int, ISerializerAdapter>();

        // private readonly IInputOutputFactory _inputOutputFactory;
        private readonly ISerializerAdapter _nullSerializerAdapter;
        private readonly int _initialOutputBufferSize;
        private readonly PortableContext _portableContext;
        private readonly PortableSerializer _portableSerializer;
        private readonly ISerializerAdapter _portableSerializerAdapter;
        private readonly ISerializerAdapter _serializableSerializerAdapter;

        private readonly ConcurrentDictionary<Type, ISerializerAdapter> _typeMap =
            new ConcurrentDictionary<Type, ISerializerAdapter>();

        protected internal readonly IPartitioningStrategy GlobalPartitioningStrategy;

        private bool _overrideClrSerialization;

        internal SerializationService(Endianness endianness, int version,
            IDictionary<int, IDataSerializableFactory> dataSerializableFactories,
            IDictionary<int, IPortableFactory> portableFactories, ICollection<IClassDefinition> classDefinitions,
            SerializerHooks hooks,
            IEnumerable<ISerializerDefinitions> definitions,
            bool checkClassDefErrors, IPartitioningStrategy partitioningStrategy, int initialOutputBufferSize,
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SerializationService>();
            Endianness = endianness;
            GlobalPartitioningStrategy = partitioningStrategy;
            _initialOutputBufferSize = initialOutputBufferSize;
            _portableContext = new PortableContext(this, version);

            // create data serializer (will be added as constant)
            var dataSerializer = new DataSerializer(hooks, dataSerializableFactories, loggerFactory);
            _dataSerializerAdapter = CreateSerializerAdapter<IIdentifiedDataSerializable>(dataSerializer);

            // create portable serializer (will be added as constant)
            _portableSerializer = new PortableSerializer(_portableContext, portableFactories);
            _portableSerializerAdapter = CreateSerializerAdapter<IPortable>(_portableSerializer);

            // create the serializer of null objects (will be added as constant)
            _nullSerializerAdapter = CreateSerializerAdapter<object>(new NullSerializer());

            // create the serializable adapter (will be added as constant)
            _serializableSerializerAdapter = CreateSerializerAdapter<object>(new SerializableSerializer());

            // add defined serializers
            foreach (var definition in definitions)
                definition.AddSerializers(this);

            // add constant serializers
            AddMoreSerializers();

            // add class definitions
            RegisterClassDefinitions(classDefinitions, checkClassDefErrors);
        }

#pragma warning disable CA1822 // Mark members as static - might not remain constant forever
        public byte GetVersion() => SerializerVersion;
#pragma warning restore CA1822

        public virtual IPortableContext GetPortableContext() => _portableContext;

        public Endianness Endianness { get; }

        #region DataOutput / DataInput

        private ObjectDataOutput GetDataOutput()
        {
            //TODO pooling
            return CreateObjectDataOutput();
        }

        private static void ReturnDataOutput(ObjectDataOutput output)
        {
            //TODO pooling
            output.Dispose();
        }

        private ObjectDataInput GetDataInput(IData data)
        {
            //TODO pooling
            return new ObjectDataInput(data.ToByteArray(), this, Endianness, HeapData.DataOffset);
        }

        private static void ReturnDataInput(ObjectDataInput input)
        {
            //TODO pooling
            input.Dispose();
        }

        #endregion

        #region Schema TEMP

        public Task FetchSchema(long schemaId) => Task.CompletedTask;

        public Task PublishSchema(long schemaId) => Task.CompletedTask;

        #endregion

        #region ToData, WriteObject, ToObject, ReadObject

        public IData ToData(object o)
            => ToData(o, GlobalPartitioningStrategy);

        public IData ToData(object o, IPartitioningStrategy strategy)
        {
            if (o is null) return null;
            if (o is IData data) return data;

            var output = GetDataOutput();

            try
            {
                var serializer = SerializerFor(o);
                var partitionHash = CalculatePartitionHash(o, strategy);
                output.WriteIntBigEndian(partitionHash);
                output.WriteIntBigEndian(serializer.TypeId);
                serializer.Write(output, o);
                return new HeapData(output.ToByteArray());
            }
            catch (Exception e) when (!(e is OutOfMemoryException) && !(e is SerializationException))
            {
                throw new SerializationException(e);
            }
            finally
            {
                ReturnDataOutput(output);
            }
        }

        public (IData, long) ToData2(object o)
            => (ToData(o, GlobalPartitioningStrategy), -1);

        public T ToObject<T>(object o)
        {
            var oo = ToObject(o);
            return oo switch
            {
                null => default,
                T ot => ot,
                _ => throw new InvalidCastException($"Deserialized object is of type {oo.GetType()}, not {typeof(T)}.")
            };
        }

        public object ToObject(object o)
        {
            if (!(o is IData data))
                return o;

            // TODO: but, if returned, why is it disposable in the first place?
            var input = GetDataInput(data);
            try
            {
                var typeId = data.TypeId;
                var serializer = SerializerFor(typeId);
                if (serializer == null) ThrowMissingSerializer(typeId);
                return serializer.Read(input);
            }
            catch (Exception e) when (!(e is OutOfMemoryException) && !(e is SerializationException))
            {
                throw new SerializationException(e);
            }
            finally
            {
                ReturnDataInput(input);
            }
        }

        public (T, long) ToObject2<T>(object o)
            => (ToObject<T>(o), -1);

        public void WriteObject(IObjectDataOutput output, object o)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));
            if (o is IData) throw new SerializationException("Cannot write IData. Use WriteData instead.");

            try
            {
                var serializer = SerializerFor(o);
                output.WriteInt(serializer.TypeId);
                serializer.Write(output, o);
            }
            catch (Exception e) when (!(e is OutOfMemoryException) && !(e is SerializationException))
            {
                throw new SerializationException(e);
            }
        }

        private static bool CanBeNull<T>()
        {
            var typeOfT = typeof(T);

            var isValueType = typeOfT.IsValueType;
            if (!isValueType) return true;

#pragma warning disable CA1508 // Avoid dead conditional code
            // false positive, https://github.com/dotnet/roslyn-analyzers/issues/4763
            var isNullableType = typeOfT.IsNullableType();
#pragma warning restore CA1508
            if (isNullableType) return true;

            return false;
        }

        public T ReadObject<T>(IObjectDataInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            try
            {
                var typeId = input.ReadInt();
                var serializer = SerializerFor(typeId);
                if (serializer == null)
                    ThrowMissingSerializer(typeId);

                var o = serializer.Read(input);
                if (o is T ot) return ot;

                if (o is null)
                {
                    if (CanBeNull<T>()) return default;
                    throw new SerializationException($"Deserialized null value cannot be of value type {typeof(T)}.");
                }

                throw new InvalidCastException($"Deserialized object is of type {o.GetType()}, not {typeof(T)}.");
            }
            catch (Exception e) when (!(e is OutOfMemoryException) && !(e is SerializationException))
            {
                throw new SerializationException(e);
            }
        }

        [DoesNotReturn]
        private static void ThrowMissingSerializer(int typeId)
        {
            throw new SerializationException($"Could not find a serializer for type {typeId}.");
        }

        [DoesNotReturn]
        private static void ThrowMissingSerializer(Type type)
        {
            throw new SerializationException($"Could not find a serializer for type {type}.");
        }

        #endregion

        #region Create object data input/output

        public ObjectDataInput CreateObjectDataInput(byte[] data)
            => new ObjectDataInput(data, this, Endianness);

        public ObjectDataInput CreateObjectDataInput(IData data)
            => new ObjectDataInput(data.ToByteArray(), this, Endianness, HeapData.DataOffset);

        public ObjectDataOutput CreateObjectDataOutput(int size)
            => new ObjectDataOutput(size, this, Endianness);

        public ObjectDataOutput CreateObjectDataOutput()
            => new ObjectDataOutput(_initialOutputBufferSize, this, Endianness);

        #endregion

        #region Register constant serializers (cannot be overriden)

        private void AddMoreSerializers()
        {
            AddConstantSerializer(null, _nullSerializerAdapter); // TODO: why add it?
            AddConstantSerializer<IIdentifiedDataSerializable>(_dataSerializerAdapter); // TODO: why add it?
            AddConstantSerializer<IPortable>(_portableSerializerAdapter); // TODO: why add it?
            _idMap.TryAdd(_serializableSerializerAdapter.TypeId, _serializableSerializerAdapter); // TODO: why add it?
        }

        private void AddConstantSerializer(Type type, ISerializerAdapter adapter)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));

            if (type != null)
                _constantTypesMap.Add(type, adapter);

            _constantTypeIds[IndexForDefaultType(adapter.TypeId)] = adapter;
        }

        private void AddConstantSerializer(Type type, ISerializer serializer)
            => AddConstantSerializer(type, CreateSerializerAdapter(type, serializer));

        public void AddConstantSerializer<TSerialized>(ISerializerAdapter adapter)
            => AddConstantSerializer(typeof(TSerialized), adapter);

        public void AddConstantSerializer<TSerialized>(ISerializer serializer)
            => AddConstantSerializer<TSerialized>(CreateSerializerAdapter<TSerialized>(serializer));

        private static MethodInfo _createSerializerAdapter;

        private ISerializerAdapter CreateSerializerAdapter(Type type, ISerializer serializer)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            if (_createSerializerAdapter == null)
            {
                var method = typeof(SerializationService).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                    .FirstOrDefault(x => x.Name == nameof(CreateSerializerAdapter) && x.IsGenericMethod);
                if (method == null)
                    throw new ServiceFactoryException("Internal error (failed to get CreateSerializerAdapter method).");
                _createSerializerAdapter = method.GetGenericMethodDefinition();
            }

            var createSerializerAdapter = _createSerializerAdapter.MakeGenericMethod(type);
            return (ISerializerAdapter) createSerializerAdapter.Invoke(this, new object[] {serializer});
        }

        private static ISerializerAdapter CreateSerializerAdapter<T>(ISerializer serializer)
        {
            return serializer switch
            {
                IStreamSerializer<T> streamSerializer => new StreamSerializerAdapter<T>(streamSerializer),
                IByteArraySerializer<T> arraySerializer => new ByteArraySerializerAdapter<T>(arraySerializer),
                _ => throw new ArgumentException("Serializer must be instance of either StreamSerializer or ByteArraySerializer.")
            };
        }

        #endregion

        #region Register configured serializers (cannot override constants)

        public void AddConfiguredSerializer(Type type, ISerializer serializer)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            if (serializer.TypeId <= 0)
                throw new ArgumentException($"Serializer {serializer} has invalid id {serializer.TypeId}", nameof(serializer));

            AddSerializer(type, CreateSerializerAdapter(type, serializer));
        }

        private bool AddSerializer(Type type, ISerializerAdapter adapter)
        {
            if (_constantTypesMap.ContainsKey(type))
                throw new ArgumentException($"Type {type} is a constant type and its serializer cannot be overridden.",
                    nameof(type));

            var added = true;

            if (!_typeMap.TryAdd(type, adapter))
            {
                added = false;
                var existing = _typeMap[type];
                if (existing.Serializer.GetType() != adapter.Serializer.GetType())
                    throw new InvalidOperationException(
                        $"Serializer {existing.Serializer} has already been registered for type {type}.");
            }

            if (!_idMap.TryAdd(adapter.TypeId, adapter))
            {
                added = false;
                var existing = _idMap[adapter.TypeId];
                if (existing.Serializer.GetType() != adapter.Serializer.GetType())
                    throw new InvalidOperationException(
                        $"Serializer {existing.Serializer} has already been registered for type id {adapter.TypeId}.");
            }

            return added;
        }

        private void AddSerializer(Type type, ISerializer serializer)
        {
            AddSerializer(type, CreateSerializerAdapter(type, serializer));
        }

        public void SetGlobalSerializer(ISerializer serializer, bool overrideClrSerialization)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            var adapter = CreateSerializerAdapter<object>(serializer);
            if (Interlocked.CompareExchange(ref _global, adapter, null) != null)
                throw new InvalidOperationException("Global serializer is already registered!");

            _overrideClrSerialization = overrideClrSerialization;
            if (!_idMap.TryAdd(serializer.TypeId, adapter))
            {
                var existing = _idMap[serializer.TypeId];
                if (existing.Serializer.GetType() != adapter.Serializer.GetType())
                {
                    Interlocked.CompareExchange(ref _global, null, adapter);
                    _overrideClrSerialization = false;
                    throw new InvalidOperationException(
                        $"Serializer {existing.Serializer} has already been registered for type id {adapter.TypeId}.");
                }
            }
        }

        #endregion

        #region Get serializers

        /// <summary>
        /// (internal for tests only)
        /// Gets the portable serializer.
        /// </summary>
        internal PortableSerializer PortableSerializer => _portableSerializer;

        private ISerializerAdapter SerializerFor(object obj)
        {
            // try
            // - null serializer if object is null
            // - default
            // - custom
            // - clr
            // - global

            if (obj == null) return _nullSerializerAdapter;

            var type = obj.GetType();

            var serializer = LookupDefaultSerializer(type) ??
                             LookupCustomSerializer(type) ??
                             (_overrideClrSerialization ? null : LookupSerializableSerializer(type)) ??
                             LookupGlobalSerializer(type);

            if (serializer == null) ThrowMissingSerializer(type);
            return serializer;
        }

        protected internal ISerializerAdapter SerializerFor(int typeId)
        {
            if (typeId <= 0)
            {
                var index = IndexForDefaultType(typeId);
                if (index < ConstantSerializersSize &&
                    index != IndexForDefaultType(SerializationConstants.CsharpClrSerializationType))
                {
                    return _constantTypeIds[index];
                }
            }

            return _idMap.TryGetValue(typeId, out var result) ? result : default;
        }

        // lookup default serializer
        // - data serializable
        // - portable
        // - constant types
        private ISerializerAdapter LookupDefaultSerializer(Type type)
        {
            // fast path for data serializable
            if (typeof(IIdentifiedDataSerializable).IsAssignableFrom(type))
                return _dataSerializerAdapter;

            // fast path for portable serialization
            if (typeof(IPortable).IsAssignableFrom(type))
                return _portableSerializerAdapter;

            // else, look for constant serializer
            if (!_constantTypesMap.TryGetValue(type, out var serializer))
                return null;

            if (serializer == null) throw new SerializationException($"Failed to find default serializer for type {type}.");
            return serializer;
        }

        private ISerializerAdapter LookupCustomSerializer(Type type)
        {
            _typeMap.TryGetValue(type, out var serializer);
            if (serializer == null)
            {
                // look for super classes
                var typeSuperclass = type.BaseType;
                ICollection<Type> interfaces = new HashSet<Type>(); //new Type[5]);
                GetInterfaces(type, interfaces);
                while (typeSuperclass != null)
                {
                    serializer = RegisterFromSuperType(type, typeSuperclass);
                    if (serializer != null)
                    {
                        break;
                    }

                    GetInterfaces(typeSuperclass, interfaces);
                    typeSuperclass = typeSuperclass.BaseType;
                }

                if (serializer == null)
                {
                    // look for interfaces
                    foreach (var typeInterface in interfaces)
                    {
                        serializer = RegisterFromSuperType(type, typeInterface);
                        if (serializer != null)
                        {
                            break;
                        }
                    }
                }
            }

            return serializer;
        }

        // lookup for CLR serialization (IsSerializable type)
        private ISerializerAdapter LookupSerializableSerializer(Type type)
        {
            if (!type.IsSerializable) return null;

            // register so we find it faster next time
            if (AddSerializer(type, _serializableSerializerAdapter))
            {
                _logger.LogWarning("Performance hint: Serialization service will use the CLR serialization " +
                                   $"for type {type}. Please consider using a faster serialization option such as " +
                                   "IIdentifiedDataSerializable.");
            }

            return _serializableSerializerAdapter;
        }

        // fallback to global serializer
        private ISerializerAdapter LookupGlobalSerializer(Type type)
        {
            var serializer = _global;

            // register so we find it faster next time
            if (serializer != null)
                AddSerializer(type, serializer);

            return serializer;
        }

        #endregion

        public virtual void DisposeData(IData data)
        {
        }

        /// <exception cref="System.IO.IOException"></exception>
        public IPortableReader CreatePortableReader(IData data)
        {
            if (!data.IsPortable)
            {
                throw new ArgumentException("Given data is not Portable! -> " + data.TypeId);
            }

            ObjectDataInput input = null;
            IPortableReader reader;
            try
            {
                input = CreateObjectDataInput(data);
                reader = _portableSerializer.CreateReader(input);
                input = null;
            }
            finally
            {
#pragma warning disable CA1508 // Avoid dead conditional code - false positive
                input?.Dispose();
#pragma warning restore CA1508
            }

            return reader;
        }

        public virtual void Dispose()
        {
            foreach (var serializer in _typeMap.Values)
            {
                serializer.Dispose();
            }

            _typeMap.Clear();
            _idMap.Clear();
            Interlocked.Exchange(ref _global, null);
            _constantTypesMap.Clear();

            _portableSerializer.Dispose();

            _nullSerializerAdapter.Dispose();
            _portableSerializerAdapter.Dispose();
            _dataSerializerAdapter.Dispose();
            _serializableSerializerAdapter.Dispose();
        }

        protected internal int CalculatePartitionHash(object obj, IPartitioningStrategy strategy)
        {
            // ToData(o) -> ToData(o, GlobalPartitionStrategy)
            // ToData(o, strategy) -> CalculatePartitionHash(o, strategy)
            //  (and this overload is not used anywhere else)

            // strategy: o -> partitionKey
            var partitioningStrategy = strategy ?? GlobalPartitioningStrategy;
            var partitionKey = partitioningStrategy?.GetPartitionKey(obj);

            // returning 0 here means that we're going to create a HeapData that:
            // - has HasPartitionHash == false
            // - returns PartitionHash = GetHashCode() which is overriden with a Murmur3 hasher

            if (partitionKey is null) return 0; // no partition key
            if (partitionKey == obj) return 0; // obj is *not* IData (else we wouldn't be here), so this is a dead end

            // fast: partitionKey *is* a hash
            // TODO: consider implementing this?
            //if (partitionKey is int hash) return hash;

            // fast: partitionKey provides a hash
            if (partitionKey is IData data) return data.PartitionHash;

            // slow: partitionKey wants to be hashed
            //   create a HeapData with NullPartitioningStrategy => CalculatePartitionHash returns zero,
            //   HasPartitionHash is false, and therefore PartitionHash is obtained by Murmur3-hashing
            //   the serialized partitionKey bytes - i.e. "hashing partitionKey"
            var meh = ToData(partitionKey, NullPartitioningStrategy);
            if (meh != null) return meh.PartitionHash;

            // duh - should never happen, really
            return 0;

            // NOTE: this is an exact copy of the Java client algorithm and in case partitionKey is e.g. a string,
            // it probably is not especially efficient (allocates a new HeapData etc) - it would probably be better
            // for the partition strategy to return a dummy IData with the pre-computed hash...
        }

        internal static bool IsNullData(IData data)
        {
            return data.DataSize == 0 && data.TypeId == SerializationConstants.ConstantTypeNull;
        }

        private static void GetInterfaces(Type type, ICollection<Type> interfaces)
        {
            var types = type.GetInterfaces();
            if (types.Length > 0)
            {
                foreach (var t in types)
                {
                    interfaces.Add(t);
                }

                foreach (var cl in types)
                {
                    GetInterfaces(cl, interfaces);
                }
            }
        }

        private static int IndexForDefaultType(int typeId)
        {
            return -typeId;
        }


        private void RegisterClassDefinition(IClassDefinition cd, IDictionary<int, IClassDefinition> classDefMap,
            bool checkClassDefErrors)
        {
            for (var i = 0; i < cd.GetFieldCount(); i++)
            {
                var fd = cd.GetField(i);
                if (fd.FieldType == FieldType.Portable || fd.FieldType == FieldType.PortableArray)
                {
                    var classId = fd.ClassId;
                    classDefMap.TryGetValue(classId, out var nestedCd);
                    if (nestedCd != null)
                    {
                        RegisterClassDefinition(nestedCd, classDefMap, checkClassDefErrors);
                        _portableContext.RegisterClassDefinition(nestedCd);
                    }
                    else
                    {
                        if (checkClassDefErrors)
                        {
                            throw new SerializationException(
                                "Could not find registered ClassDefinition for class-id: " + classId);
                        }
                    }
                }
            }

            _portableContext.RegisterClassDefinition(cd);
        }

        private void RegisterClassDefinitions(ICollection<IClassDefinition> classDefinitions, bool checkClassDefErrors)
        {
            IDictionary<int, IClassDefinition> classDefMap =
                new Dictionary<int, IClassDefinition>(classDefinitions.Count);
            foreach (var cd in classDefinitions)
            {
                if (classDefMap.ContainsKey(cd.ClassId))
                {
                    throw new SerializationException("Duplicate registration found for class-id[" +
                                                     cd.ClassId + "]!");
                }

                classDefMap.Add(cd.ClassId, cd);
            }

            foreach (var classDefinition in classDefinitions)
            {
                RegisterClassDefinition(classDefinition, classDefMap, checkClassDefErrors);
            }
        }

        private ISerializerAdapter RegisterFromSuperType(Type type, Type superType)
        {
            _typeMap.TryGetValue(superType, out var serializer);
            if (serializer != null)
            {
                AddSerializer(type, serializer);
            }

            return serializer;
        }
    }
}
