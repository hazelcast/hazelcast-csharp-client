// Copyright (c) 2008-2019, Hazelcast, Inc. All Rights Reserved.
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
using System.Numerics;
using System.Reflection;
using Hazelcast.Core;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    internal class SerializationService : ISerializationService
    {
        public const byte SerializerVersion = 1;
        private const int ConstantSerializersSize = SerializationConstants.ConstantSerializersArraySize;

        private static readonly IPartitioningStrategy TheEmptyPartitioningStrategy = new EmptyPartitioningStrategy();
        private static readonly ILogger Logger = Logging.Logger.GetLogger(typeof (SerializationService));

        private readonly ISerializerAdapter[] _constantTypeIds = new ISerializerAdapter[ConstantSerializersSize];

        private readonly Dictionary<Type, ISerializerAdapter> _constantTypesMap =
            new Dictionary<Type, ISerializerAdapter>(ConstantSerializersSize);

        private readonly BufferPoolThreadLocal _bufferPoolThreadLocal;
        private readonly ISerializerAdapter _dataSerializerAdapter;
        private readonly AtomicReference<ISerializerAdapter> _global = new AtomicReference<ISerializerAdapter>();

        private readonly ConcurrentDictionary<int, ISerializerAdapter> _idMap =
            new ConcurrentDictionary<int, ISerializerAdapter>();

        private readonly IInputOutputFactory _inputOutputFactory;
        private readonly IManagedContext _managedContext;
        private readonly ISerializerAdapter _nullSerializerAdapter;
        private readonly int _outputBufferSize;
        private readonly PortableContext _portableContext;
        private readonly PortableSerializer _portableSerializer;
        private readonly ISerializerAdapter _portableSerializerAdapter;
        private readonly ISerializerAdapter _serializableSerializerAdapter;

        private readonly ConcurrentDictionary<Type, ISerializerAdapter> _typeMap =
            new ConcurrentDictionary<Type, ISerializerAdapter>();

        protected internal readonly IPartitioningStrategy GlobalPartitioningStrategy;

        private volatile bool _isActive = true;
        private bool _overrideClrSerialization;

        internal SerializationService(IInputOutputFactory inputOutputFactory, int version,
            IDictionary<int, IDataSerializableFactory> dataSerializableFactories,
            IDictionary<int, IPortableFactory> portableFactories, ICollection<IClassDefinition> classDefinitions,
            bool checkClassDefErrors, IManagedContext managedContext,
            IPartitioningStrategy partitionStrategy, int initialOutputBufferSize, bool enableCompression,
            bool enableSharedObject)
        {
            _inputOutputFactory = inputOutputFactory;
            _managedContext = managedContext;
            GlobalPartitioningStrategy = partitionStrategy;
            _outputBufferSize = initialOutputBufferSize;
            _bufferPoolThreadLocal = new BufferPoolThreadLocal(this);
            _portableContext = new PortableContext(this, version);
            _dataSerializerAdapter =
                CreateSerializerAdapterByGeneric<IIdentifiedDataSerializable>(
                    new DataSerializer(dataSerializableFactories));
            _portableSerializer = new PortableSerializer(_portableContext, portableFactories);
            _portableSerializerAdapter = CreateSerializerAdapterByGeneric<IPortable>(_portableSerializer);
            _nullSerializerAdapter = CreateSerializerAdapterByGeneric<object>(new ConstantSerializers.NullSerializer());
            _serializableSerializerAdapter =
                CreateSerializerAdapterByGeneric<object>(new DefaultSerializers.SerializableSerializer());

            RegisterConstantSerializers();
            RegisterDefaultSerializers();
            RegisterClassDefinitions(classDefinitions, checkClassDefErrors);
        }

        public IData ToData(object obj)
        {
            return ToData(obj, GlobalPartitioningStrategy);
        }

        public IData ToData(object obj, IPartitioningStrategy strategy)
        {
            if (obj == null)
            {
                return null;
            }
            if (obj is IData)
            {
                return (IData) obj;
            }
            var pool = _bufferPoolThreadLocal.Get();
            var @out = pool.TakeOutputBuffer();
            try
            {
                var serializer = SerializerFor(obj);
                var partitionHash = CalculatePartitionHash(obj, strategy);
                @out.WriteInt(partitionHash, ByteOrder.BigEndian);
                @out.WriteInt(serializer.GetTypeId(), ByteOrder.BigEndian);
                serializer.Write(@out, obj);
                return new HeapData(@out.ToByteArray());
            }
            catch (Exception e)
            {
                throw HandleException(e);
            }
            finally
            {
                pool.ReturnOutputBuffer(@out);
            }
        }

        public T ToObject<T>(object @object)
        {
            var o = ToObject(@object);
            return o == null ? default : (T) o;
        }

        public object ToObject(object @object)
        {
            if (!(@object is IData))
            {
                return @object;
            }

            var data = (IData) @object;
            if (IsNullData(data))
            {
                return null;
            }
            var pool = _bufferPoolThreadLocal.Get();
            var @in = pool.TakeInputBuffer(data);
            try
            {
                var typeId = data.GetTypeId();
                var serializer = SerializerFor(typeId);
                if (serializer == null)
                {
                    if (_isActive)
                    {
                        throw new HazelcastSerializationException("There is no suitable de-serializer for type " +
                                                                  typeId);
                    }
                    throw new HazelcastInstanceNotActiveException();
                }
                var obj = serializer.Read(@in);
                if (_managedContext != null)
                {
                    obj = _managedContext.Initialize(obj);
                }
                return (T) obj;
            }
            catch (Exception e)
            {
                throw HandleException(e);
            }
            finally
            {
                pool.ReturnInputBuffer(@in);
            }
        }

        public void WriteObject(IObjectDataOutput output, object obj)
        {
            if (obj is IData)
            {
                throw new HazelcastSerializationException(
                    "Cannot write a Data instance! Use #writeData(ObjectDataOutput out, Data data) instead.");
            }
            try
            {
                var serializer = SerializerFor(obj);
                output.WriteInt(serializer.GetTypeId());
                serializer.Write(output, obj);
            }
            catch (Exception e)
            {
                throw HandleException(e);
            }
        }

        public byte GetVersion()
        {
            return SerializerVersion;
        }

        public T ReadObject<T>(IObjectDataInput input)
        {
            try
            {
                var typeId = input.ReadInt();
                var serializer = SerializerFor(typeId);
                if (serializer == null)
                {
                    if (_isActive)
                    {
                        throw new HazelcastSerializationException("There is no suitable de-serializer for type "
                                                                  + typeId);
                    }
                    throw new HazelcastInstanceNotActiveException();
                }
                var obj = serializer.Read(input);
                if (_managedContext != null)
                {
                    obj = _managedContext.Initialize(obj);
                }
                try
                {
                    return (T) obj;
                }
                catch (NullReferenceException)
                {
                    throw new HazelcastSerializationException("Trying to cast null value to value type " +
                                                          typeof (T));
                }
            }
            catch (Exception e)
            {
                throw HandleException(e);
            }
        }

        public virtual void DisposeData(IData data)
        {
        }

        public IBufferObjectDataInput CreateObjectDataInput(byte[] data)
        {
            return _inputOutputFactory.CreateInput(data, this);
        }

        public IBufferObjectDataInput CreateObjectDataInput(IData data)
        {
            return _inputOutputFactory.CreateInput(data, this);
        }

        public IBufferObjectDataOutput CreateObjectDataOutput(int size)
        {
            return _inputOutputFactory.CreateOutput(size, this);
        }

        public IBufferObjectDataOutput CreateObjectDataOutput()
        {
            return _inputOutputFactory.CreateOutput(_outputBufferSize, this);
        }

        public virtual IPortableContext GetPortableContext()
        {
            return _portableContext;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public IPortableReader CreatePortableReader(IData data)
        {
            if (!data.IsPortable())
            {
                throw new ArgumentException("Given data is not Portable! -> " + data.GetTypeId());
            }
            var input = CreateObjectDataInput(data);
            return _portableSerializer.CreateReader(input);
        }

        public virtual void Destroy()
        {
            _isActive = false;
            foreach (var serializer in _typeMap.Values)
            {
                serializer.Destroy();
            }
            _typeMap.Clear();
            _idMap.Clear();
            _global.Set(null);
            _constantTypesMap.Clear();
            _bufferPoolThreadLocal.Dispose();
        }

        public IManagedContext GetManagedContext()
        {
            return _managedContext;
        }

        public virtual ByteOrder GetByteOrder()
        {
            return _inputOutputFactory.GetByteOrder();
        }

        public virtual bool IsActive()
        {
            return _isActive;
        }

        public void Register(Type type, ISerializer serializer)
        {
            if (type == null)
            {
                throw new ArgumentException("Class type information is required!");
            }
            if (serializer.GetTypeId() <= 0)
            {
                throw new ArgumentException("Type id must be positive! Current: " + serializer.GetTypeId() +
                                            ", Serializer: " + serializer);
            }
            SafeRegister(type, CreateSerializerAdapter(type, serializer));
        }

        public void RegisterGlobal(ISerializer serializer, bool overrideClrSerialization)
        {
            var adapter = CreateSerializerAdapterByGeneric<object>(serializer);
            if (!_global.CompareAndSet(null, adapter))
            {
                throw new InvalidOperationException("Global serializer is already registered!");
            }
            _overrideClrSerialization = overrideClrSerialization;
            var current = _idMap.GetOrAdd(serializer.GetTypeId(), adapter);
            if (current != null && current.GetImpl().GetType() != adapter.GetImpl().GetType())
            {
                _global.CompareAndSet(adapter, null);
                _overrideClrSerialization = false;
                throw new InvalidOperationException("Serializer [" + current.GetImpl() +
                                                    "] has been already registered for type-id: "
                                                    + serializer.GetTypeId());
            }
        }

        protected internal int CalculatePartitionHash(object obj, IPartitioningStrategy strategy)
        {
            var partitionHash = 0;
            var partitioningStrategy = strategy ?? GlobalPartitioningStrategy;
            if (partitioningStrategy != null)
            {
                var pk = partitioningStrategy.GetPartitionKey(obj);
                if (pk != null && pk != obj)
                {
                    var partitionKey = ToData(pk, TheEmptyPartitioningStrategy);
                    partitionHash = partitionKey == null ? 0 : partitionKey.GetPartitionHash();
                }
            }
            return partitionHash;
        }

        protected internal ISerializerAdapter SerializerFor(int typeId)
        {
            if (typeId <= 0)
            {
                var index = IndexForDefaultType(typeId);
                if (index < ConstantSerializersSize && 
                    index != IndexForDefaultType(SerializationConstants.DefaultTypeSerializable))
                {
                    return _constantTypeIds[index];
                }
            }
            ISerializerAdapter result;
            _idMap.TryGetValue(typeId, out result);
            return _idMap.TryGetValue(typeId, out result) ? result : default(ISerializerAdapter);
        }

        internal PortableSerializer GetPortableSerializer()
        {
            return _portableSerializer;
        }

        internal static bool IsNullData(IData data)
        {
            return data.DataSize() == 0 && data.GetTypeId() == SerializationConstants.ConstantTypeNull;
        }

        internal virtual void SafeRegister(Type type, ISerializer serializer)
        {
            SafeRegister(type, CreateSerializerAdapter(type, serializer));
        }

        private ISerializerAdapter CreateSerializerAdapter(Type type, ISerializer serializer)
        {
            var methodInfo = GetType()
                .GetMethod("CreateSerializerAdapterByGeneric", BindingFlags.NonPublic | BindingFlags.Instance);
            var makeGenericMethod = methodInfo.MakeGenericMethod(type);
            var result = makeGenericMethod.Invoke(this, new object[] {serializer});
            return (ISerializerAdapter) result;
        }

        private ISerializerAdapter CreateSerializerAdapterByGeneric<T>(ISerializer serializer)
        {
            var streamSerializer = serializer as IStreamSerializer<T>;
            if (streamSerializer != null)
            {
                return new StreamSerializerAdapter<T>(streamSerializer);
            }
            var arraySerializer = serializer as IByteArraySerializer<T>;
            if (arraySerializer != null)
            {
                return new ByteArraySerializerAdapter<T>(arraySerializer);
            }
            throw new ArgumentException("Serializer must be instance of either " +
                                        "StreamSerializer or ByteArraySerializer!");
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

        private Exception HandleException(Exception e)
        {
            if (e is OutOfMemoryException)
            {
                return e;
            }
            if (e is HazelcastSerializationException)
            {
                return e;
            }
            return new HazelcastSerializationException(e);
        }

        private int IndexForDefaultType(int typeId)
        {
            return -typeId;
        }

        private ISerializerAdapter LookupCustomSerializer(Type type)
        {
            ISerializerAdapter serializer;
            _typeMap.TryGetValue(type, out serializer);
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

        private ISerializerAdapter LookupDefaultSerializer(Type type)
        {
            if (typeof (IIdentifiedDataSerializable).IsAssignableFrom(type))
            {
                return _dataSerializerAdapter;
            }
            if (typeof (IPortable).IsAssignableFrom(type))
            {
                return _portableSerializerAdapter;
            }
            ISerializerAdapter serializer;
            if (_constantTypesMap.TryGetValue(type, out serializer) && serializer != null)
            {
                return serializer;
            }
            return null;
        }

        private ISerializerAdapter LookupGlobalSerializer(Type type)
        {
            var serializer = _global.Get();
            if (serializer != null)
            {
                SafeRegister(type, serializer);
            }
            return serializer;
        }

        private ISerializerAdapter LookupSerializableSerializer(Type type)
        {
            if (type.IsSerializable)
            {
                if (SafeRegister(type, _serializableSerializerAdapter))
                {
                    Logger.Warning("Performance Hint: Serialization service will use CLR Serialization for : " + type
                                   +
                                   ". Please consider using a faster serialization option such as IIdentifiedDataSerializable.");
                }
                return _serializableSerializerAdapter;
            }
            return null;
        }

        private void RegisterClassDefinition(IClassDefinition cd, IDictionary<int, IClassDefinition> classDefMap,
            bool checkClassDefErrors)
        {
            for (var i = 0; i < cd.GetFieldCount(); i++)
            {
                var fd = cd.GetField(i);
                if (fd.GetFieldType() == FieldType.Portable || fd.GetFieldType() == FieldType.PortableArray)
                {
                    var classId = fd.GetClassId();
                    IClassDefinition nestedCd;
                    classDefMap.TryGetValue(classId, out nestedCd);
                    if (nestedCd != null)
                    {
                        RegisterClassDefinition(nestedCd, classDefMap, checkClassDefErrors);
                        _portableContext.RegisterClassDefinition(nestedCd);
                    }
                    else
                    {
                        if (checkClassDefErrors)
                        {
                            throw new HazelcastSerializationException(
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
                if (classDefMap.ContainsKey(cd.GetClassId()))
                {
                    throw new HazelcastSerializationException("Duplicate registration found for class-id[" +
                                                              cd.GetClassId() + "]!");
                }
                classDefMap.Add(cd.GetClassId(), cd);
            }
            foreach (var classDefinition in classDefinitions)
            {
                RegisterClassDefinition(classDefinition, classDefMap, checkClassDefErrors);
            }
        }

        private void RegisterConstant(Type type, ISerializer serializer)
        {
            RegisterConstant(type, CreateSerializerAdapter(type, serializer));
        }

        private void RegisterConstant(Type type, ISerializerAdapter serializer)
        {
            if (type != null)
            {
                _constantTypesMap.Add(type, serializer);
            }
            _constantTypeIds[IndexForDefaultType(serializer.GetTypeId())] = serializer;
        }

        private void RegisterConstantSerializers()
        {
            RegisterConstant(null, _nullSerializerAdapter);
            RegisterConstant(typeof (IIdentifiedDataSerializable), _dataSerializerAdapter);
            RegisterConstant(typeof (IPortable), _portableSerializerAdapter);
            RegisterConstant(typeof (byte), new ConstantSerializers.ByteSerializer());
            RegisterConstant(typeof (bool), new ConstantSerializers.BooleanSerializer());
            RegisterConstant(typeof (char), new ConstantSerializers.CharSerializer());
            RegisterConstant(typeof (short), new ConstantSerializers.ShortSerializer());
            RegisterConstant(typeof (int), new ConstantSerializers.IntegerSerializer());
            RegisterConstant(typeof (long), new ConstantSerializers.LongSerializer());
            RegisterConstant(typeof (float), new ConstantSerializers.FloatSerializer());
            RegisterConstant(typeof (double), new ConstantSerializers.DoubleSerializer());
            RegisterConstant(typeof (bool[]), new ConstantSerializers.BooleanArraySerializer());
            RegisterConstant(typeof (byte[]), new ConstantSerializers.ByteArraySerializer());
            RegisterConstant(typeof (char[]), new ConstantSerializers.CharArraySerializer());
            RegisterConstant(typeof (short[]), new ConstantSerializers.ShortArraySerializer());
            RegisterConstant(typeof (int[]), new ConstantSerializers.IntegerArraySerializer());
            RegisterConstant(typeof (long[]), new ConstantSerializers.LongArraySerializer());
            RegisterConstant(typeof (float[]), new ConstantSerializers.FloatArraySerializer());
            RegisterConstant(typeof (double[]), new ConstantSerializers.DoubleArraySerializer());
            RegisterConstant(typeof (string[]), new ConstantSerializers.StringArraySerializer());
            RegisterConstant(typeof (string), new ConstantSerializers.StringSerializer());
        }

        private void RegisterDefaultSerializers()
        {
            RegisterConstant(typeof (DateTime), new DefaultSerializers.DateSerializer());

            //TODO: proper support for generic types
            RegisterConstant(typeof (JavaClass), new DefaultSerializers.JavaClassSerializer());
            RegisterConstant(typeof(HazelcastJsonValue), new DefaultSerializers.HazelcastJsonValueSerializer());
            RegisterConstant(typeof (BigInteger), new DefaultSerializers.BigIntegerSerializer());
            RegisterConstant(typeof (JavaEnum), new DefaultSerializers.JavaEnumSerializer());
            RegisterConstant(typeof (List<object>), new DefaultSerializers.ListSerializer<object>());
            RegisterConstant(typeof (LinkedList<object>), new DefaultSerializers.LinkedListSerializer<object>());

            _idMap.TryAdd(_serializableSerializerAdapter.GetTypeId(), _serializableSerializerAdapter);
        }

        private ISerializerAdapter RegisterFromSuperType(Type type, Type superType)
        {
            ISerializerAdapter serializer;
            _typeMap.TryGetValue(superType, out serializer);
            if (serializer != null)
            {
                SafeRegister(type, serializer);
            }
            return serializer;
        }

        private bool SafeRegister(Type type, ISerializerAdapter serializer)
        {
            if (_constantTypesMap.ContainsKey(type))
            {
                throw new ArgumentException("[" + type + "] serializer cannot be overridden!");
            }
            var current = _typeMap.GetOrAdd(type, serializer);
            if (current != null && current.GetImpl().GetType() != serializer.GetImpl().GetType())
            {
                throw new InvalidOperationException("Serializer[" + current.GetImpl() +
                                                    "] has been already registered for type: " + type);
            }
            current = _idMap.GetOrAdd(serializer.GetTypeId(), serializer);
            if (current != null && current.GetImpl().GetType() != serializer.GetImpl().GetType())
            {
                throw new InvalidOperationException("Serializer [" + current.GetImpl() +
                                                    "] has been already registered for type-id: " +
                                                    serializer.GetTypeId());
            }
            return current == null;
        }

        /// <summary>
        /// Searches for a serializer for the provided object
        /// Serializers will be  searched in this order;
        ///  1-NULL serializer
        ///  2-Default serializers, like primitives, arrays, String and some C# types
        ///  3-Custom registered types by user
        ///  4-CLR serialization if type is Serializable
        ///  5-Global serializer if registered by user
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private ISerializerAdapter SerializerFor(object obj)
        {
            if (obj == null)
            {
                return _nullSerializerAdapter;
            }
            var type = obj.GetType();

            var serializer = LookupDefaultSerializer(type);
            if (serializer == null)
            {
                serializer = LookupCustomSerializer(type);
            }
            if (serializer == null && !_overrideClrSerialization)
            {
                serializer = LookupSerializableSerializer(type);
            }
            if (serializer == null)
            {
                serializer = LookupGlobalSerializer(type);
            }
            if (serializer == null)
            {
                if (_isActive)
                {
                    throw new HazelcastSerializationException("There is no suitable serializer for " + type);
                }
                throw new HazelcastInstanceNotActiveException();
            }
            return serializer;
        }

        private sealed class EmptyPartitioningStrategy : IPartitioningStrategy
        {
            public object GetPartitionKey(object key)
            {
                return null;
            }
        }
    }
}