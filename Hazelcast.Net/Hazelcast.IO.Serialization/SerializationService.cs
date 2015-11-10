// Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
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
using System.Reflection;
using System.Threading;
using Hazelcast.Core;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    internal class SerializationService : ISerializationService
    {
        public const byte SerializerVersion = 1;
        private const int ConstantSerializersSize = SerializationConstants.ConstantSerializersLength;

        private static readonly IPartitioningStrategy TheEmptyPartitioningStrategy = new EmptyPartitioningStrategy();

        private readonly ISerializerAdapter[] _constantTypeIds = new ISerializerAdapter[ConstantSerializersSize];

        private readonly Dictionary<Type, ISerializerAdapter> _constantTypesMap =
            new Dictionary<Type, ISerializerAdapter>(ConstantSerializersSize);

        private readonly ThreadLocalOutputCache _dataOutputQueue;
        private readonly ISerializerAdapter _dataSerializerAdapter;
        private readonly AtomicReference<ISerializerAdapter> _global = new AtomicReference<ISerializerAdapter>();

        private readonly ConcurrentDictionary<int, ISerializerAdapter> _idMap =
            new ConcurrentDictionary<int, ISerializerAdapter>();

        private readonly IInputOutputFactory _inputOutputFactory;
        private readonly IManagedContext _managedContext;
        private readonly int _outputBufferSize;
        private readonly PortableContext _portableContext;
        private readonly PortableSerializer _portableSerializer;
        private readonly ISerializerAdapter _portableSerializerAdapter;

        private readonly ConcurrentDictionary<Type, ISerializerAdapter> _typeMap =
            new ConcurrentDictionary<Type, ISerializerAdapter>();

        protected internal readonly IPartitioningStrategy GlobalPartitioningStrategy;

        private volatile bool _isActive = true;

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
            _dataOutputQueue = new ThreadLocalOutputCache(this);
            _portableContext = new PortableContext(this, version);
            _dataSerializerAdapter =
                CreateSerializerAdapterByGeneric<IDataSerializable>(new DataSerializer(dataSerializableFactories));
            _portableSerializer = new PortableSerializer(_portableContext, portableFactories);
            _portableSerializerAdapter = CreateSerializerAdapterByGeneric<IPortable>(_portableSerializer);
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
            var @out = Pop();
            try
            {
                var serializer = SerializerFor(obj.GetType());
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
                Push(@out);
            }
        }

        public T ToObject<T>(object @object)
        {
            if (!(@object is IData))
            {
                return @object == null ? default(T) : (T) @object;
            }
            var data = (IData) @object;
            if (IsNullData(data))
            {
                return default(T);
            }
            var @in = CreateObjectDataInput(data);
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
                IOUtil.CloseResource(@in);
            }
        }

        public void WriteObject(IObjectDataOutput output, object obj)
        {
            if (obj is IData)
            {
                throw new HazelcastSerializationException(
                    "Cannot write a Data instance! Use #writeData(ObjectDataOutput out, Data data) instead.");
            }
            var isNull = obj == null;
            try
            {
                output.WriteBoolean(isNull);
                if (isNull)
                {
                    return;
                }
                var serializer = SerializerFor(obj.GetType());
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
                var isNull = input.ReadBoolean();
                if (isNull)
                {
                    return default(T);
                }
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
                return (T) obj;
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
            _dataOutputQueue.Clear();
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

        public void RegisterGlobal(ISerializer serializer)
        {
            var adapter = CreateSerializerAdapterByGeneric<object>(serializer);
            if (!_global.CompareAndSet(null, adapter))
            {
                throw new InvalidOperationException("Global serializer is already registered!");
            }
            var current = _idMap.GetOrAdd(serializer.GetTypeId(), adapter);
            if (current != null && current.GetImpl().GetType() != adapter.GetImpl().GetType())
            {
                _global.CompareAndSet(adapter, null);
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

        protected internal IBufferObjectDataOutput Pop()
        {
            return _dataOutputQueue.Pop();
        }

        protected internal void Push(IBufferObjectDataOutput output)
        {
            _dataOutputQueue.Push(output);
        }

        protected internal ISerializerAdapter SerializerFor(int typeId)
        {
            if (typeId < 0)
            {
                var index = IndexForDefaultType(typeId);
                if (index < ConstantSerializersSize)
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
            ISerializerAdapter s;
            if (serializer is IStreamSerializer<T>)
            {
                s = new StreamSerializerAdapter<T>(this, (IStreamSerializer<T>) serializer);
            }
            else
            {
                if (serializer is IByteArraySerializer<T>)
                {
                    s = new ByteArraySerializerAdapter<T>((IByteArraySerializer<T>) serializer);
                }
                else
                {
                    throw new ArgumentException("Serializer must be instance of either " +
                                                "StreamSerializer or ByteArraySerializer!");
                }
            }
            return s;
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
                //OutOfMemoryErrorDispatcher.OnOutOfMemory((OutOfMemoryException)e);
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
            return -typeId - 1;
        }

        private ISerializerAdapter LookupSerializer(Type type)
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
                if (serializer == null)
                {
                    serializer = _global.Get();
                    if (serializer != null)
                    {
                        SafeRegister(type, serializer);
                    }
                }
            }
            return serializer;
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
            _constantTypesMap.Add(type, serializer);
            _constantTypeIds[IndexForDefaultType(serializer.GetTypeId())] = serializer;
        }

        private void RegisterConstantSerializers()
        {
            RegisterConstant(typeof (IDataSerializable), _dataSerializerAdapter);
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
            SafeRegister(typeof (DateTime), new DefaultSerializers.DateSerializer());
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

        private void SafeRegister(Type type, ISerializerAdapter serializer)
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
        }

        private ISerializerAdapter SerializerFor(Type type)
        {
            if (typeof (IDataSerializable).IsAssignableFrom(type))
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
            serializer = LookupSerializer(type);
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

        private sealed class ThreadLocalOutputCache
        {
            private readonly int _bufferSize;
            private readonly ConcurrentDictionary<Thread, ConcurrentQueue<IBufferObjectDataOutput>> _map;
            private readonly SerializationService _serializationService;

            internal ThreadLocalOutputCache(SerializationService serializationService)
            {
                _serializationService = serializationService;
                _bufferSize = serializationService._outputBufferSize;
                var initialCapacity = Environment.ProcessorCount;
                _map = new ConcurrentDictionary<Thread, ConcurrentQueue<IBufferObjectDataOutput>>(1, initialCapacity);
            }

            internal void Clear()
            {
                _map.Clear();
            }

            internal IBufferObjectDataOutput Pop()
            {
                ConcurrentQueue<IBufferObjectDataOutput> outputQueue;
                var t = Thread.CurrentThread;
                _map.TryGetValue(t, out outputQueue);
                if (outputQueue == null)
                {
                    outputQueue = new ConcurrentQueue<IBufferObjectDataOutput>();
                    _map.TryAdd(t, outputQueue);
                }
                IBufferObjectDataOutput output;
                outputQueue.TryDequeue(out output);
                return output ?? _serializationService.CreateObjectDataOutput(_bufferSize);
            }

            internal void Push(IBufferObjectDataOutput output)
            {
                if (output == null) return;
                output.Clear();
                ConcurrentQueue<IBufferObjectDataOutput> outputQueue;
                _map.TryGetValue(Thread.CurrentThread, out outputQueue);
                if (outputQueue == null)
                {
                    IOUtil.CloseResource(output);
                    return;
                }
                try
                {
                    outputQueue.Enqueue(output);
                }
                catch (Exception)
                {
                    IOUtil.CloseResource(output);
                }
            }
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