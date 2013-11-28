using System;
using System.IO;
using Hazelcast.IO.Serialization;

namespace Hazelcast.IO
{
    public sealed class IOUtil
    {
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
        public static void WriteNullableData(IObjectDataOutput output, Data data)
        {
            if (data != null)
            {
                output.WriteBoolean(true);
                data.WriteData(output);
            }
            else
            {
                // null
                output.WriteBoolean(false);
            }
        }

        /// <exception cref="System.IO.IOException"></exception>
        public static Data ReadNullableData(IObjectDataInput input)
        {
            bool isNotNull = input.ReadBoolean();
            if (isNotNull)
            {
                var data = new Data();
                data.ReadData(input);
                return data;
            }
            return null;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public static Data ReadData(IObjectDataInput input)
        {
            var data = new Data();
            data.ReadData(input);
            return data;
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
                catch (IOException)
                {
                }
            }
        }
    }
}