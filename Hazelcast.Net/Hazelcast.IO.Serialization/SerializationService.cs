using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Hazelcast.Core;
using Hazelcast.IO.Serialization.DefaultSerializers;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization
{
    internal sealed class SerializationService : ISerializationService
    {
        private const int ConstantSerializersSize = SerializationConstants.ConstantSerializersLength;

        private static readonly IPartitioningStrategy emptyPartitioningStrategy = new EmptyPartitioningStrategy();

        private readonly ISerializerAdapter[] constantTypeIds = new ISerializerAdapter[ConstantSerializersSize];

        private readonly IDictionary<Type, ISerializerAdapter> constantTypesMap =
            new Dictionary<Type, ISerializerAdapter>(ConstantSerializersSize);

        private readonly ISerializerAdapter dataSerializerAdapter;

        private readonly AtomicReference<ISerializerAdapter> global = new AtomicReference<ISerializerAdapter>();
        private readonly IPartitioningStrategy globalPartitioningStrategy;

        private readonly ConcurrentDictionary<int, ISerializerAdapter> idMap =
            new ConcurrentDictionary<int, ISerializerAdapter>();

        private readonly IInputOutputFactory inputOutputFactory;
        private readonly IManagedContext managedContext;
        private readonly int outputBufferSize;

        private readonly ConcurrentQueue<IBufferObjectDataOutput> outputPool =
            new ConcurrentQueue<IBufferObjectDataOutput>();

        private readonly PortableSerializer portableSerializer;

        private readonly ISerializerAdapter portableSerializerAdapter;

        private readonly SerializationContext serializationContext;

        private readonly ConcurrentDictionary<Type, ISerializerAdapter> typeMap =
            new ConcurrentDictionary<Type, ISerializerAdapter>();

        private volatile bool active = true;

        internal SerializationService(IInputOutputFactory inputOutputFactory, int version,
            IDictionary<int, IDataSerializableFactory> dataSerializableFactories,
            IDictionary<int, IPortableFactory> portableFactories,
            ICollection<IClassDefinition> classDefinitions, bool checkClassDefErrors,
            IManagedContext managedContext,
            IPartitioningStrategy partitionStrategy, int initialOutputBufferSize, bool enableCompression,
            bool enableSharedObject)
        {
            this.inputOutputFactory = inputOutputFactory;
            this.managedContext = managedContext;
            globalPartitioningStrategy = partitionStrategy;
            outputBufferSize = initialOutputBufferSize;
            var loader = new PortableHookLoader(portableFactories);
            serializationContext = new SerializationContext(this, loader.GetFactories().Keys, version);
            foreach (IClassDefinition cd in loader.GetDefinitions())
            {
                serializationContext.RegisterClassDefinition(cd);
            }
            dataSerializerAdapter = new StreamSerializerAdapter<IDataSerializable>(this,
                new DataSerializer(dataSerializableFactories));
            portableSerializer = new PortableSerializer(serializationContext, loader.GetFactories());
            portableSerializerAdapter = new StreamSerializerAdapter<IPortable>(this, portableSerializer);

            RegisterConstant<IDataSerializable>(dataSerializerAdapter);
            RegisterConstant<IPortable>(portableSerializerAdapter);

            RegisterConstant<byte>(new ConstantSerializers.ByteSerializer());
            RegisterConstant<bool>(new ConstantSerializers.BooleanSerializer());
            RegisterConstant<char>(new ConstantSerializers.CharSerializer());
            RegisterConstant<short>(new ConstantSerializers.ShortSerializer());
            RegisterConstant<int>(new ConstantSerializers.IntegerSerializer());
            RegisterConstant<long>(new ConstantSerializers.LongSerializer());
            RegisterConstant<float>(new ConstantSerializers.FloatSerializer());
            RegisterConstant<double>(new ConstantSerializers.DoubleSerializer());
            RegisterConstant<byte[]>(new ConstantSerializers.TheByteArraySerializer());
            RegisterConstant<char[]>(new ConstantSerializers.CharArraySerializer());
            RegisterConstant<short[]>(new ConstantSerializers.ShortArraySerializer());
            RegisterConstant<int[]>(new ConstantSerializers.IntegerArraySerializer());
            RegisterConstant<long[]>(new ConstantSerializers.LongArraySerializer());
            RegisterConstant<float[]>(new ConstantSerializers.FloatArraySerializer());
            RegisterConstant<double[]>(new ConstantSerializers.DoubleArraySerializer());
            RegisterConstant<string>(new ConstantSerializers.StringSerializer());

            //FIXME SERIALIZER REGISTERSS
            //SafeRegister<DateTime>( new DefaultSerializers.DateSerializer());
            //SafeRegister<BigInteger>( new DefaultSerializers.BigIntegerSerializer());
            //SafeRegister<BigDecimal>( new DefaultSerializers.BigDecimalSerializer());
            //SafeRegister<Externalizable>( new DefaultSerializers.Externalizer(enableCompression));
            //SafeRegister<ISerializable>(new DefaultSerializers.ObjectSerializer(enableSharedObject, enableCompression));
            //SafeRegister<Type>( new DefaultSerializers.ClassSerializer());
            //SafeRegister<Enum>(new EnumSerializer());
            RegisterClassDefinitions(classDefinitions, checkClassDefErrors);
        }

        public Data ToData(object obj)
        {
            return ToData(obj, globalPartitioningStrategy);
        }

        public Data ToData(object obj, IPartitioningStrategy strategy)
        {
            if (obj == null)
            {
                return null;
            }
            if (obj is Data)
            {
                return (Data) obj;
            }
            try
            {
                ISerializerAdapter serializer = SerializerFor(obj.GetType());
                if (serializer == null)
                {
                    if (active)
                    {
                        throw new HazelcastSerializationException("There is no suitable serializer for " + obj.GetType());
                    }
                    throw new HazelcastInstanceNotActiveException();
                }
                byte[] bytes = serializer.Write(obj);
                var data = new Data(serializer.GetTypeId(), bytes);
                if (obj is IPortable)
                {
                    var portable = (IPortable) obj;
                    data.classDefinition = serializationContext.Lookup(portable.GetFactoryId(), portable.GetClassId());
                }
                if (strategy == null)
                {
                    strategy = globalPartitioningStrategy;
                }
                if (strategy != null)
                {
                    object pk = strategy.GetPartitionKey(obj);
                    if (pk != null && pk != obj)
                    {
                        Data partitionKey = ToData(pk, emptyPartitioningStrategy);
                        data.partitionHash = (partitionKey == null) ? -1 : partitionKey.GetPartitionHash();
                    }
                }
                return data;
            }
            catch (Exception e)
            {
                HandleException(e);
            }
            return null;
        }

        public T ToObject<T>(object input)
        {
            if (input == null)
            {
                return default (T);
            }
            Data data = input as Data;
            if (data == null)
            {
                return (T)input;
            }
            if (data.BufferSize() == 0 && data.IsDataSerializable())
            {
                return default(T);
            }
            try
            {
                int typeId = data.type;
                ISerializerAdapter serializer = SerializerFor(typeId);
                if (serializer == null)
                {
                    if (active)
                    {
                        throw new HazelcastSerializationException("There is no suitable de-serializer for type " +
                                                                  typeId);
                    }
                    throw new HazelcastInstanceNotActiveException();
                }
                if (typeId == SerializationConstants.ConstantTypePortable)
                {
                    serializationContext.RegisterClassDefinition(data.classDefinition);
                }
                object obj = serializer.Read(data);
                if (managedContext != null)
                {
                    obj = managedContext.Initialize(obj);
                }
                return (T)obj;
            }
            catch (Exception e)
            {
                HandleException(e);
            }
            return default(T);
        }

        public void WriteObject(IObjectDataOutput output, object obj)
        {
            bool isNull = obj == null;
            try
            {
                output.WriteBoolean(isNull);
                if (isNull)
                {
                    return;
                }
                ISerializerAdapter serializer = SerializerFor(obj.GetType());
                if (serializer == null)
                {
                    if (active)
                    {
                        throw new HazelcastSerializationException("There is no suitable serializer for " + obj.GetType());
                    }
                    throw new HazelcastInstanceNotActiveException();
                }
                output.WriteInt(serializer.GetTypeId());
                if (obj is IPortable)
                {
                    var portable = (IPortable) obj;
                    IClassDefinition classDefinition = serializationContext.LookupOrRegisterClassDefinition(portable);
                    classDefinition.WriteData(output);
                }
                serializer.Write(output, obj);
            }
            catch (Exception e)
            {
                HandleException(e);
            }
        }

        public object ReadObject(IObjectDataInput input)
        {
            try
            {
                bool isNull = input.ReadBoolean();
                if (isNull)
                {
                    return null;
                }
                int typeId = input.ReadInt();
                ISerializerAdapter serializer = SerializerFor(typeId);
                if (serializer == null)
                {
                    if (active)
                    {
                        throw new HazelcastSerializationException("There is no suitable de-serializer for type " +
                                                                  typeId);
                    }
                    throw new HazelcastInstanceNotActiveException();
                }
                if (typeId == SerializationConstants.ConstantTypePortable && input is PortableContextAwareInputStream)
                {
                    IClassDefinition classDefinition = new ClassDefinition();
                    classDefinition.ReadData(input);
                    classDefinition = serializationContext.RegisterClassDefinition(classDefinition);
                    var ctxIn = (PortableContextAwareInputStream) input;
                    ctxIn.SetClassDefinition(classDefinition);
                }
                object obj = serializer.Read(input);
                if (managedContext != null)
                {
                    obj = managedContext.Initialize(obj);
                }
                return obj;
            }
            catch (Exception e)
            {
                HandleException(e);
            }
            return null;
        }

        public IBufferObjectDataInput CreateObjectDataInput(byte[] data)
        {
            return inputOutputFactory.CreateInput(data, this);
        }

        public IBufferObjectDataInput CreateObjectDataInput(Data data)
        {
            return inputOutputFactory.CreateInput(data, this);
        }

        public IBufferObjectDataOutput CreateObjectDataOutput(int size)
        {
            return inputOutputFactory.CreateOutput(size, this);
        }

        public ObjectDataOutputStream CreateObjectDataOutputStream(BinaryWriter binaryWriter)
        {
            return new ObjectDataOutputStream(binaryWriter, this);
        }

        public ObjectDataInputStream CreateObjectDataInputStream(BinaryReader binaryReader)
        {
            return new ObjectDataInputStream(binaryReader, this);
        }

        public ObjectDataOutputStream CreateObjectDataOutputStream(BinaryWriter binaryWriter, bool isBigEndian)
        {
            return new ObjectDataOutputStream(binaryWriter, this, isBigEndian);
        }

        public ObjectDataInputStream CreateObjectDataInputStream(BinaryReader binaryReader, bool isBigEndian)
        {
            return new ObjectDataInputStream(binaryReader, this, isBigEndian);
        }

        public void Register<T>(ISerializer serializer)
        {
            //if (type == null)
            //{
            //    throw new ArgumentException("Class type information is required!");
            //}

            if (serializer.GetTypeId() <= 0)
            {
                throw new ArgumentException("Type id must be positive! Current: " + serializer.GetTypeId() +
                                            ", Serializer: " + serializer);
            }
            SafeRegister(typeof (T), CreateSerializerAdapter<T>(serializer));
        }


        public void RegisterGlobal(ISerializer serializer)
        {
            ISerializerAdapter adapter = CreateSerializerAdapter<object>(serializer);
            if (!global.CompareAndSet(null, adapter))
            {
                throw new InvalidOperationException("Global serializer is already registered!");
            }
            if (!idMap.TryAdd(serializer.GetTypeId(), adapter))
            {
                ISerializerAdapter current = null;
                idMap.TryGetValue(serializer.GetTypeId(), out current);
                if (current != null && current.GetImpl().GetType() != adapter.GetImpl().GetType())
                {
                    global.CompareAndSet(adapter, null);
                    throw new InvalidOperationException("Serializer [" + current.GetImpl() +
                                                        "] has been already registered for type-id: " +
                                                        serializer.GetTypeId());
                }
            }
        }

        public ISerializationContext GetSerializationContext()
        {
            return serializationContext;
        }

        public IPortableReader CreatePortableReader(Data data)
        {
            return new DefaultPortableReader(portableSerializer, CreateObjectDataInput(data), data.GetClassDefinition());
        }

        private void RegisterClassDefinitions(ICollection<IClassDefinition> classDefinitions,
            bool checkClassDefErrors)
        {
            IDictionary<int, IClassDefinition> classDefMap =
                new Dictionary<int, IClassDefinition>(classDefinitions.Count);
            foreach (IClassDefinition cd in classDefinitions)
            {
                if (classDefMap.ContainsKey(cd.GetClassId()))
                {
                    throw new HazelcastSerializationException("Duplicate registration found for class-id[" +
                                                              cd.GetClassId() + "]!");
                }
                classDefMap.Add(cd.GetClassId(), cd);
            }
            foreach (IClassDefinition classDefinition in classDefinitions)
            {
                RegisterClassDefinition(classDefinition, classDefMap, checkClassDefErrors);
            }
        }

        private void RegisterClassDefinition(IClassDefinition cd, IDictionary<int, IClassDefinition> classDefMap,
            bool checkClassDefErrors)
        {
            for (int i = 0; i < cd.GetFieldCount(); i++)
            {
                IFieldDefinition fd = cd.Get(i);
                if (fd.GetFieldType() == FieldType.Portable || fd.GetFieldType() == FieldType.PortableArray)
                {
                    int classId = fd.GetClassId();
                    IClassDefinition nestedCd;
                    classDefMap.TryGetValue(classId, out nestedCd);
                    if (nestedCd != null)
                    {
                        ((ClassDefinition) cd).AddClassDef(nestedCd);
                        RegisterClassDefinition(nestedCd, classDefMap, checkClassDefErrors);
                        serializationContext.RegisterClassDefinition(nestedCd);
                    }
                    else
                    {
                        if (checkClassDefErrors)
                        {
                            throw new HazelcastSerializationException(
                                "Could not find registered IClassDefinition for class-id: " + classId);
                        }
                    }
                }
            }
            serializationContext.RegisterClassDefinition(cd);
        }

        private void HandleException(Exception e)
        {
            if (e is OutOfMemoryException)
            {
                OutOfMemoryErrorDispatcher.OnOutOfMemory((OutOfMemoryException) e);
                return;
            }
            if (e is HazelcastSerializationException)
            {
                throw e;
            }
            throw new HazelcastSerializationException(e);
        }

        internal IBufferObjectDataOutput Pop()
        {
            IBufferObjectDataOutput output;
            outputPool.TryDequeue(out output);
            if (output == null)
            {
                output = inputOutputFactory.CreateOutput(outputBufferSize, this);
            }
            return output;
        }

        internal void Push(IBufferObjectDataOutput output)
        {
            if (output != null)
            {
                output.Clear();
                outputPool.Enqueue(output);
            }
        }

        private ISerializerAdapter CreateSerializerAdapter<T>(ISerializer serializer)
        {
            return __CreateSerializerAdapter<T>(serializer);
        }

        private ISerializerAdapter __CreateSerializerAdapter<T>(ISerializer serializer)
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
                    throw new ArgumentException(
                        "Serializer must be instance of either IStreamSerializer or IByteArraySerializer!");
                }
            }
            return s;
        }

        public ISerializerAdapter SerializerFor(Type type)
        {
            if (typeof (IDataSerializable).IsAssignableFrom(type))
            {
                return dataSerializerAdapter;
            }
            if (typeof (IPortable).IsAssignableFrom(type))
            {
                return portableSerializerAdapter;
            }
            ISerializerAdapter serializer;
            constantTypesMap.TryGetValue(type, out serializer);
            if (serializer != null)
            {
                return serializer;
            }
            ISerializerAdapter _serializer;
            typeMap.TryGetValue(type, out _serializer);
            if (_serializer == null)
            {
                // look for super classes
                Type typeSuperclass = type.BaseType;
                var interfaces = new HashSet<Type>();
                GetInterfaces(type, interfaces);
                while (typeSuperclass != null)
                {
                    if ((_serializer = RegisterFromSuperType(type, typeSuperclass)) != null)
                    {
                        break;
                    }
                    GetInterfaces(typeSuperclass, interfaces);
                    typeSuperclass = typeSuperclass.BaseType;
                }
                if (_serializer == null)
                {
                    // look for interfaces
                    foreach (Type typeInterface in interfaces)
                    {
                        if ((_serializer = RegisterFromSuperType(type, typeInterface)) != null)
                        {
                            break;
                        }
                    }
                }
                if (_serializer == null && (_serializer = global.Get()) != null)
                {
                    SafeRegister(type, _serializer);
                }
            }
            return _serializer;
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

        private void RegisterConstant<T>(ISerializer serializer)
        {
            RegisterConstant<T>(CreateSerializerAdapter<T>(serializer));
        }

        private void RegisterConstant<T>(ISerializerAdapter serializer)
        {
            constantTypesMap.Add(typeof (T), serializer);
            constantTypeIds[IndexForDefaultType(serializer.GetTypeId())] = serializer;
        }

        private void SafeRegister<T>(ISerializer serializer)
        {
            SafeRegister(typeof (T), CreateSerializerAdapter<T>(serializer));
        }

        private void SafeRegister(Type type, ISerializerAdapter serializer)
        {
            if (constantTypesMap.ContainsKey(type))
            {
                throw new ArgumentException("[" + type + "] serializer cannot be overridden!");
            }
            if (!typeMap.TryAdd(type, serializer))
            {
                ISerializerAdapter current = typeMap.GetOrAdd(type, serializer);
                if (current != null && current.GetImpl().GetType() != serializer.GetImpl().GetType())
                {
                    throw new InvalidOperationException("Serializer[" + current.GetImpl() +
                                                        "] has been already registered for type: " + type);
                }
            }
            if (!idMap.TryAdd(serializer.GetTypeId(), serializer))
            {
                ISerializerAdapter current = idMap.GetOrAdd(serializer.GetTypeId(), serializer);
                if (current != null && current.GetImpl().GetType() != serializer.GetImpl().GetType())
                {
                    throw new InvalidOperationException("Serializer [" + current.GetImpl() +
                                                        "] has been already registered for type-id: " +
                                                        serializer.GetTypeId());
                }
            }
        }

        public ISerializerAdapter SerializerFor(int typeId)
        {
            if (typeId < 0)
            {
                int index = IndexForDefaultType(typeId);
                if (index < ConstantSerializersSize)
                {
                    return constantTypeIds[index];
                }
            }
            ISerializerAdapter rtn;
            idMap.TryGetValue(typeId, out rtn);
            return rtn;
        }

        private int IndexForDefaultType(int typeId)
        {
            return -typeId - 1;
        }

        public IManagedContext GetManagedContext()
        {
            return managedContext;
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