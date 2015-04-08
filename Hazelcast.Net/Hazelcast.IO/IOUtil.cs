using System;
using System.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;

namespace Hazelcast.IO
{
    internal sealed class IOUtil
    {
        public const byte PrimitiveTypeBoolean = 1;
        public const byte PrimitiveTypeByte = 2;
        public const byte PrimitiveTypeShort = 3;
        public const byte PrimitiveTypeInteger = 4;
        public const byte PrimitiveTypeLong = 5;
        public const byte PrimitiveTypeFloat = 6;
        public const byte PrimitiveTypeDouble = 7;
        public const byte PrimitiveTypeUtf = 8;

        private IOUtil()
        {
        }

        /// <summary>
        ///     This method has a direct dependency on how objects are serialized in
        ///     <see cref="IDataSerializable" />
        ///     ! If the stream
        ///     format is ever changed this extraction method needs to be changed too!
        /// </summary>
        /// <exception cref="System.IO.IOException" />
        public static long ExtractOperationCallId(IData data, ISerializationService serializationService)
        {
            IObjectDataInput input = serializationService.CreateObjectDataInput(data);
            var identified = input.ReadBoolean();
            if (identified)
            {
                // read factoryId
                input.ReadInt();
                // read typeId
                input.ReadInt();
            }
            else
            {
                // read classname
                input.ReadUTF();
            }
            // read callId
            return input.ReadLong();
        }

        /// <exception cref="System.IO.IOException" />
        public static void WriteByteArray(IObjectDataOutput @out, byte[] value)
        {
            var size = (value == null) ? 0 : value.Length;
            @out.WriteInt(size);
            if (size > 0)
            {
                @out.Write(value);
            }
        }

        /// <exception cref="System.IO.IOException" />
        public static byte[] ReadByteArray(IObjectDataInput @in)
        {
            var size = @in.ReadInt();
            if (size == 0)
            {
                return null;
            }
            var b = new byte[size];
            @in.ReadFully(b);
            return b;
        }

        /// <exception cref="System.IO.IOException" />
        public static void WriteObject(IObjectDataOutput @out, object @object)
        {
            var isBinary = @object is IData;
            @out.WriteBoolean(isBinary);
            if (isBinary)
            {
                @out.WriteData((IData) @object);
            }
            else
            {
                @out.WriteObject(@object);
            }
        }

        /// <exception cref="System.IO.IOException" />
        public static T ReadObject<T>(IObjectDataInput @in)
        {
            var isBinary = @in.ReadBoolean();
            if (isBinary)
            {
                return (T) @in.ReadData();
            }
            return @in.ReadObject<T>();
        }

        /// <exception cref="System.IO.IOException" />
        public static void WriteAttributeValue(object value, IObjectDataOutput @out)
        {
            var type = value.GetType();
            if (type.Equals(typeof (bool)))
            {
                @out.WriteByte(PrimitiveTypeBoolean);
                @out.WriteBoolean((bool) value);
            }
            else if (type.Equals(typeof (byte)))
            {
                @out.WriteByte(PrimitiveTypeByte);
                @out.WriteByte((byte) value);
            }
            else if (type.Equals(typeof (short)))
            {
                @out.WriteByte(PrimitiveTypeShort);
                @out.WriteShort((short) value);
            }
            else if (type.Equals(typeof (int)))
            {
                @out.WriteByte(PrimitiveTypeInteger);
                @out.WriteInt((int) value);
            }
            else if (type.Equals(typeof (long)))
            {
                @out.WriteByte(PrimitiveTypeLong);
                @out.WriteLong((long) value);
            }
            else if (type.Equals(typeof (float)))
            {
                @out.WriteByte(PrimitiveTypeFloat);
                @out.WriteFloat((float) value);
            }
            else if (type.Equals(typeof (double)))
            {
                @out.WriteByte(PrimitiveTypeDouble);
                @out.WriteDouble((double) value);
            }
            else if (type.Equals(typeof (string)))
            {
                @out.WriteByte(PrimitiveTypeUtf);
                @out.WriteUTF((string) value);
            }
            else
            {
                throw new InvalidOperationException("Illegal attribute type id found");
            }
        }

        /// <exception cref="System.IO.IOException" />
        public static object ReadAttributeValue(IObjectDataInput @in)
        {
            var type = @in.ReadByte();
            switch (type)
            {
                case PrimitiveTypeBoolean:
                {
                    return @in.ReadBoolean();
                }

                case PrimitiveTypeByte:
                {
                    return @in.ReadByte();
                }

                case PrimitiveTypeShort:
                {
                    return @in.ReadShort();
                }

                case PrimitiveTypeInteger:
                {
                    return @in.ReadInt();
                }

                case PrimitiveTypeLong:
                {
                    return @in.ReadLong();
                }

                case PrimitiveTypeFloat:
                {
                    return @in.ReadFloat();
                }

                case PrimitiveTypeDouble:
                {
                    return @in.ReadDouble();
                }

                case PrimitiveTypeUtf:
                {
                    return @in.ReadUTF();
                }

                default:
                {
                    throw new InvalidOperationException("Illegal attribute type id found");
                }
            }
        }

        /// <summary>Closes the Closable quietly.</summary>
        /// <remarks>Closes the Closable quietly. So no exception will be thrown. Can also safely be called with a null value.</remarks>
        /// <param name="closeable">the Closeable to close.</param>
        public static void CloseResource(IDisposable closeable)
        {
            if (closeable != null)
            {
                try
                {
                    closeable.Dispose();
                }
                catch (IOException e)
                {
                    Logger.GetLogger(typeof (IOUtil)).Finest("closeResource failed", e);
                }
            }
        }
    }
}