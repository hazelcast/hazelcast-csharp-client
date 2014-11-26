using System;
using System.IO;
using Hazelcast.IO.Serialization;
using Hazelcast.Logging;
using Hazelcast.Net.Ext;

namespace Hazelcast.IO
{
    internal sealed class IOUtil
    {
        public const byte PRIMITIVE_TYPE_BOOLEAN = 1;
        public const byte PRIMITIVE_TYPE_BYTE = 2;
        public const byte PRIMITIVE_TYPE_SHORT = 3;
        public const byte PRIMITIVE_TYPE_INTEGER = 4;
        public const byte PRIMITIVE_TYPE_LONG = 5;
        public const byte PRIMITIVE_TYPE_FLOAT = 6;
        public const byte PRIMITIVE_TYPE_DOUBLE = 7;
        public const byte PRIMITIVE_TYPE_UTF = 8;

        /// <summary>
        /// This method has a direct dependency on how objects are serialized in
        /// <see cref="DataSerializer">com.hazelcast.nio.serialization.DataSerializer</see>
        /// ! If the stream
        /// format is ever changed this extraction method needs to be changed too!
        /// </summary>
        /// <exception cref="System.IO.IOException"></exception>
        public static long ExtractOperationCallId(IData data, ISerializationService serializationService)
        {
            IObjectDataInput input = serializationService.CreateObjectDataInput(data.GetData());
            bool identified = input.ReadBoolean();
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

        /// <exception cref="System.IO.IOException"></exception>
        public static void WriteByteArray(IObjectDataOutput output, byte[] value)
        {
            int size = (value == null) ? 0 : value.Length;
            output.WriteInt(size);
            if (size > 0)
            {
                output.Write(value);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public static byte[] ReadByteArray(IObjectDataInput input)
        {
            int size = input.ReadInt();
            if (size == 0)
            {
                return null;
            }
            var b = new byte[size];
            input.ReadFully(b);
            return b;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public static void WriteObject(IObjectDataOutput output, object @object)
        {
            bool isBinary = @object is IData;
            output.WriteBoolean(isBinary);
            if (isBinary)
            {
                output.WriteData((IData)@object);
            }
            else
            {
                output.WriteObject(@object);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public static T ReadObject<T>(IObjectDataInput input)
        {
            bool isBinary = input.ReadBoolean();
            if (isBinary)
            {
                return (T)input.ReadData();
            }
            return input.ReadObject<T>();
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
                    Logger.GetLogger(typeof(IOUtil)).Finest("closeResource failed", e);
                }
            }
        }

        public static int CopyToHeapBuffer(ByteBuffer src, ByteBuffer dest)
        {
            if (src == null) return 0;
            int n = Math.Min(src.Remaining(), dest.Remaining());
            if (n > 0)
            {
                if (n < 16)
                {
                    for (int i = 0; i < n; i++)
                    {
                        dest.Put(src.Get());
                    }
                }
                else
                {
                    int srcPosition = src.Position;
                    int destPosition = dest.Position;
                    Array.Copy(src.Array(), srcPosition, dest.Array(), destPosition, n);
                    src.Position = srcPosition + n;
                    dest.Position = destPosition + n;
                }
            }
            return n;
        }

        public static void WriteAttributeValue(object value, IObjectDataOutput output)
        {
            var type = value.GetType();
            if (type == typeof (bool))
            {
                output.WriteByte(PRIMITIVE_TYPE_BOOLEAN);
                output.WriteBoolean((Boolean) value);
            }
            else if (type == typeof (byte))
            {
                output.WriteByte(PRIMITIVE_TYPE_BYTE);
                output.WriteByte((byte) value);
            }
            else if (type == typeof (short))
            {
                output.WriteByte(PRIMITIVE_TYPE_SHORT);
                output.WriteShort((short) value);
            }
            else if (type == typeof (int))
            {
                output.WriteByte(PRIMITIVE_TYPE_INTEGER);
                output.WriteInt((int) value);
            }
            else if (type == typeof (long))
            {
                output.WriteByte(PRIMITIVE_TYPE_LONG);
                output.WriteLong((long) value);
            }
            else if (type == typeof (float))
            {
                output.WriteByte(PRIMITIVE_TYPE_FLOAT);
                output.WriteFloat((float) value);
            }
            else if (type == typeof (double))
            {
                output.WriteByte(PRIMITIVE_TYPE_DOUBLE);
                output.WriteDouble((double) value);
            }
            else if (type == typeof (string))
            {
                output.WriteByte(PRIMITIVE_TYPE_UTF);
                output.WriteUTF((string) value);
            }
            else
            {
                throw new InvalidOperationException("Illegal attribute type id found");
            }
        }

        public static object ReadAttributeValue(IObjectDataInput input)
        {
            byte type = input.ReadByte();
            switch (type)
            {
                case PRIMITIVE_TYPE_BOOLEAN:
                    return input.ReadBoolean();
                case PRIMITIVE_TYPE_BYTE:
                    return input.ReadByte();
                case PRIMITIVE_TYPE_SHORT:
                    return input.ReadShort();
                case PRIMITIVE_TYPE_INTEGER:
                    return input.ReadInt();
                case PRIMITIVE_TYPE_LONG:
                    return input.ReadLong();
                case PRIMITIVE_TYPE_FLOAT:
                    return input.ReadFloat();
                case PRIMITIVE_TYPE_DOUBLE:
                    return input.ReadDouble();
                case PRIMITIVE_TYPE_UTF:
                    return input.ReadUTF();
            }
            throw new NotSupportedException("Illegal type id found");
        }
    }
}