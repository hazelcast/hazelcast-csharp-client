using System;
using System.ComponentModel;
using System.Numerics;
using Hazelcast.Hazelcast.Net.Ext;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO.Serialization.DefaultSerializers
{
    //TODO FIXME
        //public sealed class BigDecimalSerializer : SingletonSerializer<Decimal>
        //{
        //    internal readonly BigIntegerSerializer bigIntegerSerializer = new BigIntegerSerializer();

        //    public override int GetTypeId()
        //    {
        //        return SerializationConstants.DefaultTypeBigDecimal;
        //    }

        //    /// <exception cref="System.IO.IOException"></exception>
        //    public override Decimal Read(IObjectDataInput input)
        //    {
        //        //TODO FIXME
        //        throw new NotImplementedException("BigDecimal");
        //        //var bytes = new byte[input.ReadInt()];

        //        //BigInteger bigInt = bigIntegerSerializer.Read(input);
        //        //int scale = input.ReadInt();
        //        //return new BigDecimal(bigInt, scale);
        //    }

        //    /// <exception cref="System.IO.IOException"></exception>
        //    public override void Write(IObjectDataOutput output, Decimal obj)
        //    {
        //        BigInteger bigInt = obj.UnscaledValue();
        //        int scale = obj.Scale();
        //        bigIntegerSerializer.Write(output, bigInt);
        //        output.WriteInt(scale);
        //    }

        //    public static decimal ToDecimal(byte[] bytes)
        //    {
        //        //check that it is even possible to convert the array
        //        if (bytes.Count() != 16)
        //            throw new Exception("A decimal must be created from exactly 16 bytes");
        //        //make an array to convert back to int32's
        //        Int32[] bits = new Int32[4];
        //        for (int i = 0; i <= 15; i += 4)
        //        {
        //            //convert every 4 bytes into an int32
        //            bits[i / 4] = BitConverter.ToInt32(bytes, i);
        //        }
        //        //Use the decimal's new constructor to
        //        //create an instance of decimal
        //        return new decimal(bits);
        //    }
        //}

        public sealed class BigIntegerSerializer : SingletonSerializer<BigInteger>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.DefaultTypeBigInteger;
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override BigInteger Read(IObjectDataInput input)
            {
                var bytes = new byte[input.ReadInt()];
                input.ReadFully(bytes);
                return new BigInteger(bytes);
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override void Write(IObjectDataOutput output, BigInteger obj)
            {
                byte[] bytes = obj.ToByteArray();
                output.WriteInt(bytes.Length);
                output.Write(bytes);
            }
        }

        public sealed class ClassSerializer : SingletonSerializer<Type>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.DefaultTypeClass;
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override Type Read(IObjectDataInput input)
            {
                //            try {
                //TODO CLASSLOAD
                //                return ClassLoaderUtil.loadClass(in.getClassLoader(), in.readUTF());
                return null;
            }

            //            } catch (ClassNotFoundException e) {
            //                throw new HazelcastSerializationException(e);
            //            }
            /// <exception cref="System.IO.IOException"></exception>
            public override void Write(IObjectDataOutput output, Type obj)
            {
                output.WriteUTF(obj.FullName);
            }
        }

        public sealed class DateSerializer : SingletonSerializer<DateTime>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.DefaultTypeDate;
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override DateTime Read(IObjectDataInput input)
            {
                return new DateTime().CreateDateTime(input.ReadLong());
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override void Write(IObjectDataOutput output, DateTime obj)
            {
                output.WriteLong(obj.Subtract(obj.EpoxDateTime()).Ticks);
            }
        }

        public sealed class EnumSerializer : SingletonSerializer<Enum>
        {
            public override int GetTypeId()
            {
                return SerializationConstants.DefaultTypeEnum;
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override void Write(IObjectDataOutput output, Enum obj)
            {
                output.WriteUTF(obj.GetType().FullName);
                //TODO BURASI DOGRU MU??????
                output.WriteUTF(Enum.GetName(obj.GetType(),obj));
            }

            /// <exception cref="System.IO.IOException"></exception>
            public override Enum Read(IObjectDataInput input)
            {
                //            String clazzName = in.readUTF();
                //            Class clazz;
                //            try {
                //                clazz = ClassLoaderUtil.loadClass(in.getClassLoader(), clazzName);
                //            } catch (ClassNotFoundException e) {
                //                throw new HazelcastSerializationException("Failed to deserialize enum: " + clazzName, e);
                //            }
                //
                //            String name = in.readUTF();
                //            return Enum.valueOf(clazz, name);
                //TODO CLASSLOAD
                return null;
            }
        }

        //public sealed class Externalizer : SingletonSerializer<Externalizable>
        //{
        //    private readonly bool gzipEnabled;

        //    public Externalizer(bool gzipEnabled)
        //    {
        //        this.gzipEnabled = gzipEnabled;
        //    }

        //    public override int GetTypeId()
        //    {
        //        return SerializationConstants.DefaultTypeExternalizable;
        //    }

        //    /// <exception cref="System.IO.IOException"></exception>
        //    public override Externalizable Read(IObjectDataInput input)
        //    {
        //        string className = input.ReadUTF();
        //        //            try {
        //        //                final Externalizable ds = ClassLoaderUtil.newInstance(in.getClassLoader(), className);
        //        //                final ObjectInputStream objectInputStream;
        //        //                final InputStream inputStream = (InputStream) in;
        //        //                if (gzipEnabled) {
        //        //                    objectInputStream = newObjectInputStream(in.getClassLoader(), new GZIPInputStream(inputStream));
        //        //                } else {
        //        //                    objectInputStream = newObjectInputStream(in.getClassLoader(), inputStream);
        //        //                }
        //        //                ds.readExternal(objectInputStream);
        //        //                return ds;
        //        //            } catch (final Exception e) {
        //        //                throw new HazelcastSerializationException("Problem while reading Externalizable class : "
        //        //                        + className + ", exception: " + e);
        //        //            }
        //        //TODO CLASSLOAD
        //        return null;
        //    }

        //    /// <exception cref="System.IO.IOException"></exception>
        //    public override void Write(IObjectDataOutput output, Externalizable obj)
        //    {
        //        output.WriteUTF(obj.GetFieldType().FullName);
        //        ObjectOutputStream objectOutputStream;
        //        var outputStream = (OutputStream) output;
        //        if (gzipEnabled)
        //        {
        //            objectOutputStream = new ObjectOutputStream(new GZIPOutputStream(outputStream));
        //        }
        //        else
        //        {
        //            objectOutputStream = new ObjectOutputStream(outputStream);
        //        }
        //        obj.WriteExternal(objectOutputStream);
        //        // Force flush if not yet written due to internal behavior if pos < 1024
        //        objectOutputStream.Flush();
        //    }
        //}

        //public sealed class ObjectSerializer : SingletonSerializer<object>
        //{
        //    private readonly bool gzipEnabled;
        //    private readonly bool shared;

        //    public ObjectSerializer(bool shared, bool gzipEnabled)
        //    {
        //        this.shared = shared;
        //        this.gzipEnabled = gzipEnabled;
        //    }

        //    public override int GetTypeId()
        //    {
        //        return SerializationConstants.DefaultTypeObject;
        //    }

        //    /// <exception cref="System.IO.IOException"></exception>
        //    public override object Read(IObjectDataInput input)
        //    {
        //        ObjectInputStream objectInputStream;
        //        var inputStream = (InputStream) input;
        //                    if (gzipEnabled) {
        //                        objectInputStream = newObjectInputStream(in.getClassLoader(), new GZIPInputStream(inputStream));
        //                    } else {
        //                        objectInputStream = newObjectInputStream(in.getClassLoader(), inputStream);
        //                    }
                
        //                    final Object result;
        //                    try {
        //                        if (shared) {
        //                            result = objectInputStream.readObject();
        //                        } else {
        //                            result = objectInputStream.readUnshared();
        //                        }
        //                    } catch (ClassNotFoundException e) {
        //                        throw new HazelcastSerializationException(e);
        //                    }
        //                    return result;
        //        //TODO CLASS LOAD
        //        return null;
        //    }

        //    /// <exception cref="System.IO.IOException"></exception>
        //    public override void Write(IObjectDataOutput output, object obj)
        //    {
        //        ObjectOutputStream objectOutputStream;
        //        var outputStream = (OutputStream) output;
        //        if (gzipEnabled)
        //        {
        //            objectOutputStream = new ObjectOutputStream(new GZIPOutputStream(outputStream));
        //        }
        //        else
        //        {
        //            objectOutputStream = new ObjectOutputStream(outputStream);
        //        }
        //        if (shared)
        //        {
        //            objectOutputStream.WriteObject(obj);
        //        }
        //        else
        //        {
        //            objectOutputStream.WriteUnshared(obj);
        //        }
        //        // Force flush if not yet written due to internal behavior if pos < 1024
        //        objectOutputStream.Flush();
        //    }
        //}

        public abstract class SingletonSerializer<T> : IStreamSerializer<T>
        {
            public virtual void Destroy()
            {
            }

            public abstract int GetTypeId();

            public abstract T Read(IObjectDataInput arg1);

            public abstract void Write(IObjectDataOutput arg1, T arg2);
        }

}