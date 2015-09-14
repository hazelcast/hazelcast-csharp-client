using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Hazelcast.Core;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    internal class SerializationService : ISerializationService
    {
        private const int CONSTANT_SERIALIZERS_SIZE = SerializationConstants.ConstantSerializersLength;

        private static readonly IPartitioningStrategy emptyPartitioningStrategy = new EmptyPartitioningStrategy();
        protected internal readonly IManagedContext managedContext;
        protected internal readonly PortableContext portableContext;
        protected internal readonly IInputOutputFactory inputOutputFactory;
        protected internal readonly IPartitioningStrategy globalPartitioningStrategy;
        private readonly Dictionary<Type, ISerializerAdapter> constantTypesMap = new Dictionary<Type, ISerializerAdapter>(CONSTANT_SERIALIZERS_SIZE);
        private readonly ISerializerAdapter[] constantTypeIds = new ISerializerAdapter[CONSTANT_SERIALIZERS_SIZE];
        private readonly ConcurrentDictionary<Type, ISerializerAdapter> typeMap = new ConcurrentDictionary<Type, ISerializerAdapter>();
        private readonly ConcurrentDictionary<int, ISerializerAdapter> idMap = new ConcurrentDictionary<int, ISerializerAdapter>();
        private readonly AtomicReference<ISerializerAdapter> global = new AtomicReference<ISerializerAdapter>();
        private readonly ThreadLocalOutputCache dataOutputQueue;
        private readonly PortableSerializer portableSerializer;
        private readonly ISerializerAdapter dataSerializerAdapter;
        private readonly ISerializerAdapter portableSerializerAdapter;
        private readonly int outputBufferSize;
        private volatile bool active = true;

        internal SerializationService(IInputOutputFactory inputOutputFactory, int version, IDictionary<int, IDataSerializableFactory> dataSerializableFactories, 
            IDictionary<int, IPortableFactory> portableFactories, ICollection<IClassDefinition> classDefinitions, bool checkClassDefErrors, IManagedContext managedContext,
            IPartitioningStrategy partitionStrategy, int initialOutputBufferSize, bool enableCompression, bool enableSharedObject )
        {
            this.inputOutputFactory = inputOutputFactory;
            this.managedContext = managedContext;
            this.globalPartitioningStrategy = partitionStrategy;
            this.outputBufferSize = initialOutputBufferSize;
            dataOutputQueue = new ThreadLocalOutputCache(this);
            portableContext = new PortableContext(this, version);
            dataSerializerAdapter = CreateSerializerAdapterByGeneric<IDataSerializable>(new DataSerializer(dataSerializableFactories));
            portableSerializer = new PortableSerializer(portableContext, portableFactories);
            portableSerializerAdapter = CreateSerializerAdapterByGeneric<IPortable>(portableSerializer);
            RegisterConstantSerializers();
            RegisterDefaultSerializers();
            RegisterClassDefinitions(classDefinitions, checkClassDefErrors);
        }

        private void RegisterDefaultSerializers()
        {
            SafeRegister(typeof(DateTime), new DefaultSerializers.DateSerializer());
        }

        private void RegisterConstantSerializers()
        {
            RegisterConstant(typeof(IDataSerializable), dataSerializerAdapter);
            RegisterConstant(typeof(IPortable), portableSerializerAdapter);
            RegisterConstant(typeof(byte), new ConstantSerializers.ByteSerializer());
            RegisterConstant(typeof(bool), new ConstantSerializers.BooleanSerializer());
            RegisterConstant(typeof(char), new ConstantSerializers.CharSerializer());
            RegisterConstant(typeof(short), new ConstantSerializers.ShortSerializer());
            RegisterConstant(typeof(int), new ConstantSerializers.IntegerSerializer());
            RegisterConstant(typeof(long), new ConstantSerializers.LongSerializer());
            RegisterConstant(typeof(float), new ConstantSerializers.FloatSerializer());
            RegisterConstant(typeof(double), new ConstantSerializers.DoubleSerializer());
            RegisterConstant(typeof(byte[]), new ConstantSerializers.TheByteArraySerializer());
            RegisterConstant(typeof(char[]), new ConstantSerializers.CharArraySerializer());
            RegisterConstant(typeof(short[]), new ConstantSerializers.ShortArraySerializer());
            RegisterConstant(typeof(int[]), new ConstantSerializers.IntegerArraySerializer());
            RegisterConstant(typeof(long[]), new ConstantSerializers.LongArraySerializer());
            RegisterConstant(typeof(float[]), new ConstantSerializers.FloatArraySerializer());
            RegisterConstant(typeof(double[]), new ConstantSerializers.DoubleArraySerializer());
            RegisterConstant(typeof(string), new ConstantSerializers.StringSerializer());
        }

        private void RegisterClassDefinitions(ICollection<IClassDefinition> classDefinitions, bool checkClassDefErrors)
        {
            IDictionary<int, IClassDefinition> classDefMap = new Dictionary<int, IClassDefinition>(classDefinitions.Count);
            foreach (IClassDefinition cd in classDefinitions)
            {
                if (classDefMap.ContainsKey(cd.GetClassId()))
                {
                    throw new HazelcastSerializationException("Duplicate registration found for class-id["+ cd.GetClassId() + "]!");
                }
                classDefMap.Add(cd.GetClassId(), cd);
            }
            foreach (IClassDefinition classDefinition in classDefinitions)
            {
                RegisterClassDefinition(classDefinition, classDefMap, checkClassDefErrors);
            }
        }

        private void RegisterClassDefinition(IClassDefinition cd, IDictionary<int, IClassDefinition> classDefMap, bool checkClassDefErrors)
        {
            for (int i = 0; i < cd.GetFieldCount(); i++)
            {
                IFieldDefinition fd = cd.GetField(i);
                if (fd.GetFieldType() == FieldType.Portable || fd.GetFieldType() == FieldType.PortableArray)
                {
                    int classId = fd.GetClassId();
                    IClassDefinition nestedCd;
                    classDefMap.TryGetValue(classId, out nestedCd);
                    if (nestedCd != null)
                    {
                        RegisterClassDefinition(nestedCd, classDefMap, checkClassDefErrors);
                        portableContext.RegisterClassDefinition(nestedCd);
                    }
                    else
                    {
                        if (checkClassDefErrors)
                        {
                            throw new HazelcastSerializationException("Could not find registered ClassDefinition for class-id: "+ classId);
                        }
                    }
                }
            }
            portableContext.RegisterClassDefinition(cd);
        }

        public IData ToData(object obj)
        {
            return ToData(obj, globalPartitioningStrategy);
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
            IBufferObjectDataOutput @out = Pop();
            try
            {
                ISerializerAdapter serializer = SerializerFor(obj.GetType());
                @out.WriteInt(serializer.GetTypeId(), ByteOrder.BigEndian);
                int partitionHash = CalculatePartitionHash(obj, strategy);
                bool hasPartitionHash = partitionHash != 0;
                @out.WriteBoolean(hasPartitionHash);
                serializer.Write(@out, obj);
                if (hasPartitionHash)
                {
                    @out.WriteInt(partitionHash, ByteOrder.BigEndian);
                }
                return new DefaultData(@out.ToByteArray());
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

        protected internal int CalculatePartitionHash(object obj, IPartitioningStrategy strategy)
        {
            int partitionHash = 0;
            IPartitioningStrategy partitioningStrategy = strategy ?? globalPartitioningStrategy;
            if (partitioningStrategy != null)
            {
                object pk = partitioningStrategy.GetPartitionKey(obj);
                if (pk != null && pk != obj)
                {
                    var partitionKey = ToData(pk, emptyPartitioningStrategy);
                    partitionHash = partitionKey == null ? 0 : partitionKey.GetPartitionHash();
                }
            }
            return partitionHash;
        }

        public T ToObject<T>(object @object)
        {
            if (!(@object is IData))
            {
                return @object == null ? default(T) : (T) @object;
            }
            IData data = (IData)@object;
            if (IsNullData(data))
            {
                return default(T);
            }
            IBufferObjectDataInput @in = CreateObjectDataInput(data);
            try
            {
                int typeId = data.GetTypeId();
                ISerializerAdapter serializer = SerializerFor(typeId);
                if (serializer == null)
                {
                    if (active)
                    {
                        throw new HazelcastSerializationException("There is no suitable de-serializer for type " + typeId);
                    }
                    throw new HazelcastInstanceNotActiveException();
                }
                object obj = serializer.Read(@in);
                if (managedContext != null)
                {
                    obj = managedContext.Initialize(obj);
                }
                return (T)obj;
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

        internal static bool IsNullData(IData data)
        {
            return data.DataSize() == 0 && data.GetTypeId() == SerializationConstants.ConstantTypeNull;
        }

        public void WriteObject(IObjectDataOutput output, object obj)
        {
            if (obj is IData)
            {
                throw new HazelcastSerializationException("Cannot write a Data instance! Use #writeData(ObjectDataOutput out, Data data) instead.");
            }
            bool isNull = obj == null;
            try
            {
                output.WriteBoolean(isNull);
                if (isNull)
                {
                    return;
                }
                ISerializerAdapter serializer = SerializerFor(obj.GetType());
                output.WriteInt(serializer.GetTypeId());
                serializer.Write(output, obj);
            }
            catch (Exception e)
            {
                throw HandleException(e);
            }
        }

        public T ReadObject<T>(IObjectDataInput input)
        {
            try
            {
                bool isNull = input.ReadBoolean();
                if (isNull)
                {
                    return default(T);
                }
                int typeId = input.ReadInt();
                ISerializerAdapter serializer = SerializerFor(typeId);
                if (serializer == null)
                {
                    if (active)
                    {
                        throw new HazelcastSerializationException("There is no suitable de-serializer for type "
                             + typeId);
                    }
                    throw new HazelcastInstanceNotActiveException();
                }
                object obj = serializer.Read(input);
                if (managedContext != null)
                {
                    obj = managedContext.Initialize(obj);
                }
                return (T)obj;
            }
            catch (Exception e)
            {
                throw HandleException(e);
            }
        }

        public void WriteData(IObjectDataOutput output, IData data)
        {
            try
            {
                bool isNull = data == null;
                output.WriteBoolean(isNull);
                if (isNull)
                {
                    return;
                }
                WriteDataInternal(output, data);
            }
            catch (Exception e)
            {
                throw HandleException(e);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        private void WriteDataInternal(IObjectDataOutput output, IData data)
        {
            output.WriteByteArray(data.ToByteArray());
        }

        public IData ReadData(IObjectDataInput input)
        {
            try
            {
                bool isNull = input.ReadBoolean();
                if (isNull)
                {
                    return null;
                }
                return new DefaultData(input.ReadByteArray());
            }
            catch (Exception e)
            {
                throw HandleException(e);
            }
        }

        public virtual void DisposeData(IData data)
        {
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

        protected internal IBufferObjectDataOutput Pop()
        {
            return dataOutputQueue.Pop();
        }

        protected internal void Push(IBufferObjectDataOutput output)
        {
            dataOutputQueue.Push(output);
        }

        public IBufferObjectDataInput CreateObjectDataInput(byte[] data)
        {
            return inputOutputFactory.CreateInput(data, this);
        }

        public IBufferObjectDataInput CreateObjectDataInput(IData data)
        {
            return inputOutputFactory.CreateInput(data, this);
        }

        public IBufferObjectDataOutput CreateObjectDataOutput(int size)
        {
            return inputOutputFactory.CreateOutput(size, this);
        }

        public ObjectDataOutputStream CreateObjectDataOutputStream(Stream outputStream)
        {
            return new ObjectDataOutputStream(outputStream, this);
        }

        public ObjectDataInputStream CreateObjectDataInputStream(Stream inputSream)
        {
            return new ObjectDataInputStream(inputSream, this);
        }

        public void Register(Type type, ISerializer serializer)
        {
            if (type == null)
            {
                throw new ArgumentException("Class type information is required!");
            }
            if (serializer.GetTypeId() <= 0)
            {
                throw new ArgumentException("Type id must be positive! Current: " + serializer.GetTypeId() + ", Serializer: " + serializer);
            }
            SafeRegister(type, CreateSerializerAdapter(type, serializer));
        }

        public void RegisterGlobal(ISerializer serializer)
        {
            ISerializerAdapter adapter = CreateSerializerAdapterByGeneric<object>(serializer);
            if (!global.CompareAndSet(null, adapter))
            {
                throw new InvalidOperationException("Global serializer is already registered!");
            }
            ISerializerAdapter current = idMap.GetOrAdd(serializer.GetTypeId(), adapter);
            if (current != null && current.GetImpl().GetType() != adapter.GetImpl().GetType())
            {
                global.CompareAndSet(adapter, null);
                throw new InvalidOperationException("Serializer [" + current.GetImpl() + "] has been already registered for type-id: "
                     + serializer.GetTypeId());
            }
        }
        private ISerializerAdapter CreateSerializerAdapter(Type type, ISerializer serializer)
        {
            MethodInfo methodInfo = GetType().GetMethod("CreateSerializerAdapterByGeneric", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo makeGenericMethod = methodInfo.MakeGenericMethod(type);
            object result = makeGenericMethod.Invoke(this, new object[] { serializer });
            return (ISerializerAdapter)result;
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
                    throw new ArgumentException("Serializer must be instance of either " + "StreamSerializer or ByteArraySerializer!");
                }
            }
            return s;
        }

        private ISerializerAdapter SerializerFor(Type type)
        {
            if (typeof(IDataSerializable).IsAssignableFrom(type))
            {
                return dataSerializerAdapter;
            }
            if (typeof(IPortable).IsAssignableFrom(type))
            {
                return portableSerializerAdapter;
            }
            ISerializerAdapter serializer;
            if (constantTypesMap.TryGetValue(type, out serializer) && serializer != null)
            {
                return serializer;
            }
            serializer = LookupSerializer(type);
            if (serializer == null)
            {
                if (active)
                {
                    throw new HazelcastSerializationException("There is no suitable serializer for " + type);
                }
                throw new HazelcastInstanceNotActiveException();
            }
            return serializer;
        }

        private ISerializerAdapter LookupSerializer(Type type)
        {
            ISerializerAdapter serializer;
            typeMap.TryGetValue(type, out serializer);
            if (serializer == null)
            {
                // look for super classes
                Type typeSuperclass = type.BaseType;
                ICollection<Type> interfaces = new HashSet<Type>();//new Type[5]);
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
                    foreach (Type typeInterface in interfaces)
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
                    serializer = global.Get();
                    if (serializer != null)
                    {
                        SafeRegister(type, serializer);
                    }
                }
            }
            return serializer;
        }

        private static void GetInterfaces(Type type, ICollection<Type> interfaces)
        {
            Type[] types = type.GetInterfaces();
            if (types.Length > 0)
            {
                foreach (Type _type in types)
                {
                    interfaces.Add(_type);
                }
                foreach (Type cl in types)
                {
                    GetInterfaces(cl, interfaces);
                }
            }
        }

        private ISerializerAdapter RegisterFromSuperType(Type type, Type superType)
        {
            ISerializerAdapter serializer;
            typeMap.TryGetValue(superType, out serializer);
            if (serializer != null)
            {
                SafeRegister(type, serializer);
            }
            return serializer;
        }

        private void RegisterConstant(Type type, ISerializer serializer)
        {
            RegisterConstant(type, CreateSerializerAdapter(type, serializer));
        }

        private void RegisterConstant(Type type, ISerializerAdapter serializer)
        {
            constantTypesMap.Add(type, serializer);
            constantTypeIds[IndexForDefaultType(serializer.GetTypeId())] = serializer;
        }

        internal virtual void SafeRegister(Type type, ISerializer serializer)
        {
            SafeRegister(type, CreateSerializerAdapter(type, serializer));
        }

        private void SafeRegister(Type type, ISerializerAdapter serializer)
        {
            if (constantTypesMap.ContainsKey(type))
            {
                throw new ArgumentException("[" + type + "] serializer cannot be overridden!");
            }
            ISerializerAdapter current = typeMap.GetOrAdd(type, serializer);
            if (current != null && current.GetImpl().GetType() != serializer.GetImpl().GetType())
            {
                throw new InvalidOperationException("Serializer[" + current.GetImpl() + "] has been already registered for type: "+ type);
            }
            current = idMap.GetOrAdd(serializer.GetTypeId(), serializer);
            if (current != null && current.GetImpl().GetType() != serializer.GetImpl().GetType())
            {
                throw new InvalidOperationException("Serializer [" + current.GetImpl() + "] has been already registered for type-id: " + serializer.GetTypeId());
            }
        }

        protected internal ISerializerAdapter SerializerFor(int typeId)
        {
            if (typeId < 0)
            {
                int index = IndexForDefaultType(typeId);
                if (index < CONSTANT_SERIALIZERS_SIZE)
                {
                    return constantTypeIds[index];
                }
            }
            ISerializerAdapter result;
            idMap.TryGetValue(typeId, out result);
            return idMap.TryGetValue(typeId, out result) ? result : default(ISerializerAdapter);
        }

        private int IndexForDefaultType(int typeId)
        {
            return -typeId - 1;
        }

        public virtual IPortableContext GetPortableContext()
        {
            return portableContext;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public IPortableReader CreatePortableReader(IData data)
        {
            if (!data.IsPortable())
            {
                throw new ArgumentException("Given data is not Portable! -> " + data.GetTypeId());
            }
            IBufferObjectDataInput input = CreateObjectDataInput(data);
            return portableSerializer.CreateReader(input);
        }

        public virtual void Destroy()
        {
            active = false;
            foreach (ISerializerAdapter serializer in typeMap.Values)
            {
                serializer.Destroy();
            }
            typeMap.Clear();
            idMap.Clear();
            global.Set(null);
            constantTypesMap.Clear();
            dataOutputQueue.Clear();
        }

        public IManagedContext GetManagedContext()
        {
            return managedContext;
        }

        public virtual ByteOrder GetByteOrder()
        {
            return inputOutputFactory.GetByteOrder();
        }

        public virtual bool IsActive()
        {
            return active;
        }

        private sealed class ThreadLocalOutputCache
        {
            private readonly ConcurrentDictionary<Thread, ConcurrentQueue<IBufferObjectDataOutput>> map;
            private readonly SerializationService serializationService;
            private readonly int bufferSize;

            internal ThreadLocalOutputCache(SerializationService serializationService)
            {
                this.serializationService = serializationService;
                bufferSize = serializationService.outputBufferSize;
                int initialCapacity = Environment.ProcessorCount;
                map = new ConcurrentDictionary<Thread, ConcurrentQueue<IBufferObjectDataOutput>>(1, initialCapacity);
            }

            internal IBufferObjectDataOutput Pop()
            {
                ConcurrentQueue<IBufferObjectDataOutput> outputQueue;
                Thread t = Thread.CurrentThread;
                map.TryGetValue(t, out outputQueue);
                if (outputQueue == null)
                {
                    outputQueue = new ConcurrentQueue<IBufferObjectDataOutput>();
                    map.TryAdd(t, outputQueue);
                }
                IBufferObjectDataOutput output;
                outputQueue.TryDequeue(out output);
                return output ?? serializationService.CreateObjectDataOutput(bufferSize);
            }

            internal void Push(IBufferObjectDataOutput output)
            {
                if (output == null) return;
                output.Clear();
                ConcurrentQueue<IBufferObjectDataOutput> outputQueue = null;
                map.TryGetValue(Thread.CurrentThread, out outputQueue);
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

            internal void Clear()
            {
                map.Clear();
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
