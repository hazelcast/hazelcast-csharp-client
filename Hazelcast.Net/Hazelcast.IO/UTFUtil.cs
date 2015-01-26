using System;
using System.IO;
using System.Reflection;
using Hazelcast.Logging;
using Hazelcast.Util;

namespace Hazelcast.IO
{
    /// <summary>Class to encode/decode UTF-Strings to and from byte-arrays.</summary>
    internal sealed class UTFEncoderDecoder
    {
        public const int UTF_BUFFER_SIZE = 1024;
        private const int STRING_CHUNK_SIZE = 16 * 1024;

        private static readonly Hazelcast.IO.UTFEncoderDecoder INSTANCE;

        //private const bool ASCII_AWARE = false;
        //System.Boolean.Parse(Environment.GetEnvironmentVariable(("hazelcast.nio.asciiaware"));

        static UTFEncoderDecoder()
        {
            // Because this flag is not set for Non-Buffered Data Output classes
            // but results may be compared in unit tests.
            // Buffered Data Output may set this flag
            // but Non-Buffered Data Output class always set this flag to "false".
            // So their results may be different.
            INSTANCE = BuildUTFUtil();
        }

        private readonly IStringCreator stringCreator;
        private readonly IUtfWriter utfWriter;

        internal UTFEncoderDecoder(IStringCreator stringCreator, IUtfWriter utfWriter)
        {
            this.stringCreator = stringCreator;
            this.utfWriter = utfWriter;
        }

        /// <exception cref="System.IO.IOException"></exception>
        public static void WriteUTF(IDataOutput output, string str, byte[] buffer)
        {
            INSTANCE.WriteUTF0(output, str, buffer);
        }

        /// <exception cref="System.IO.IOException"></exception>
        public static string ReadUTF(IDataInput input, byte[] buffer)
        {
            return INSTANCE.ReadUTF0(input, buffer);
        }

        // ********************************************************************* //
        /// <exception cref="System.IO.IOException"></exception>
        public void WriteUTF0(IDataOutput output, string str, byte[] buffer)
        {
            if (!QuickMath.IsPowerOfTwo(buffer.Length))
            {
                throw new ArgumentException("Size of the buffer has to be power of two, was " + buffer
                    .Length);
            }
            bool isNull = str == null;
            output.WriteBoolean(isNull);
            if (isNull)
            {
                return;
            }
            int length = str.Length;
            output.WriteInt(length);
            output.WriteInt(length);
            if (length > 0)
            {
                int chunkSize = (length / STRING_CHUNK_SIZE) + 1;
                for (int i = 0; i < chunkSize; i++)
                {
                    int beginIndex = Math.Max(0, i * STRING_CHUNK_SIZE - 1);
                    int endIndex = Math.Min((i + 1) * STRING_CHUNK_SIZE - 1, length);
                    utfWriter.WriteShortUTF(output, str, beginIndex, endIndex, buffer);
                }
            }
        }

        internal interface IUtfWriter
        {
            /// <exception cref="System.IO.IOException"></exception>
            void WriteShortUTF(IDataOutput output, string str, int beginIndex, int endIndex, byte[] buffer);
        }

        internal abstract class AbstractCharArrayUtfWriter : IUtfWriter
        {
            /// <exception cref="System.IO.IOException"></exception>
            public void WriteShortUTF(IDataOutput output, string str, int beginIndex, int endIndex, byte[] buffer)
            {
                bool isBufferObjectDataOutput = output is IBufferObjectDataOutput;
                IBufferObjectDataOutput bufferObjectDataOutput = isBufferObjectDataOutput ? (IBufferObjectDataOutput)output : null;
                char[] value = GetCharArray(str);
                int i;
                int c;
                int bufferPos = 0;
                int utfLength = 0;
                int utfLengthLimit;
                int pos = 0;
                if (isBufferObjectDataOutput)
                {
                    // At most, one character can hold 3 bytes
                    utfLengthLimit = str.Length * 3;
                    // We save current position of buffer data output.
                    // Then we write the length of UTF and ASCII state to here
                    pos = bufferObjectDataOutput.Position();
                    // Moving position explicitly is not good way
                    // since it may cause overflow exceptions for example "ByteArrayObjectDataOutput".
                    // So, write dummy data and let DataOutput handle it by expanding or etc ...
                    bufferObjectDataOutput.WriteShort(0);
                    //if (ASCII_AWARE)
                    //{
                    //    bufferObjectDataOutput.WriteBoolean(false);
                    //}
                }
                else
                {
                    utfLength = CalculateUtf8Length(value, beginIndex, endIndex);
                    if (utfLength > 65535)
                    {
                        throw new InvalidDataException("encoded string too long:" + utfLength + " bytes");
                    }
                    utfLengthLimit = utfLength;
                    output.WriteShort(utfLength);
                    //if (ASCII_AWARE)
                    //{
                    //    // We cannot determine that all characters are ASCII or not without iterating over it
                    //    // So, we mark it as not ASCII, so all characters will be checked.
                    //    output.WriteBoolean(false);
                    //}
                }
                if (buffer.Length >= utfLengthLimit)
                {
                    for (i = beginIndex; i < endIndex; i++)
                    {
                        c = value[i];
                        if (!(c <= unchecked((int)(0x007F)) && c >= unchecked((int)(0x0001))))
                        {
                            break;
                        }
                        buffer[bufferPos++] = unchecked((byte)c);
                    }
                    for (; i < endIndex; i++)
                    {
                        c = value[i];
                        if (c <= unchecked((int)(0x007F)) && c >= unchecked((int)(0x0001)))
                        {
                            buffer[bufferPos++] = unchecked((byte)c);
                        }
                        else
                        {
                            if (c > unchecked((int)(0x07FF)))
                            {
                                buffer[bufferPos++] = unchecked((byte)(unchecked((int)(0xE0)) | ((c >> 12) & unchecked((int)(0x0F)))));
                                buffer[bufferPos++] = unchecked((byte)(unchecked((int)(0x80)) | ((c >> 6) & unchecked((int)(0x3F)))));
                                buffer[bufferPos++] = unchecked((byte)(unchecked((int)(0x80)) | ((c) & unchecked((int)(0x3F)))));
                            }
                            else
                            {
                                buffer[bufferPos++] = unchecked((byte)(unchecked((int)(0xC0)) | ((c >> 6) & unchecked((int)(0x1F)))));
                                buffer[bufferPos++] = unchecked((byte)(unchecked((int)(0x80)) | ((c) & unchecked((int)(0x3F)))));
                            }
                        }
                    }
                    output.Write(buffer, 0, bufferPos);
                    if (isBufferObjectDataOutput)
                    {
                        utfLength = bufferPos;
                    }
                }
                else
                {
                    for (i = beginIndex; i < endIndex; i++)
                    {
                        c = value[i];
                        if (!(c <= unchecked((int)(0x007F)) && c >= unchecked((int)(0x0001))))
                        {
                            break;
                        }
                        bufferPos = Buffering(buffer, bufferPos, unchecked((byte)c), output);
                    }
                    if (isBufferObjectDataOutput)
                    {
                        utfLength = i - beginIndex;
                    }
                    for (; i < endIndex; i++)
                    {
                        c = value[i];
                        if (c <= unchecked((int)(0x007F)) && c >= unchecked((int)(0x0001)))
                        {
                            bufferPos = Buffering(buffer, bufferPos, unchecked((byte)c), output);
                            if (isBufferObjectDataOutput)
                            {
                                utfLength++;
                            }
                        }
                        else
                        {
                            if (c > unchecked((int)(0x07FF)))
                            {
                                bufferPos = Buffering(buffer, bufferPos, unchecked((byte)(unchecked((int)(0xE0))
                                    | ((c >> 12) & unchecked((int)(0x0F))))), output);
                                bufferPos = Buffering(buffer, bufferPos, unchecked((byte)(unchecked((int)(0x80))
                                    | ((c >> 6) & unchecked((int)(0x3F))))), output);
                                bufferPos = Buffering(buffer, bufferPos, unchecked((byte)(unchecked((int)(0x80))
                                    | ((c) & unchecked((int)(0x3F))))), output);
                                if (isBufferObjectDataOutput)
                                {
                                    utfLength += 3;
                                }
                            }
                            else
                            {
                                bufferPos = Buffering(buffer, bufferPos, unchecked((byte)(unchecked((int)(0xC0))
                                    | ((c >> 6) & unchecked((int)(0x1F))))), output);
                                bufferPos = Buffering(buffer, bufferPos, unchecked((byte)(unchecked((int)(0x80))
                                    | ((c) & unchecked((int)(0x3F))))), output);
                                if (isBufferObjectDataOutput)
                                {
                                    utfLength += 2;
                                }
                            }
                        }
                    }
                    int length = bufferPos % buffer.Length;
                    output.Write(buffer, 0, length == 0 ? buffer.Length : length);
                }
                if (isBufferObjectDataOutput)
                {
                    if (utfLength > 65535)
                    {
                        throw new InvalidDataException("encoded string too long:" + utfLength + " bytes");
                    }
                    // Write the length of UTF to saved position before
                    bufferObjectDataOutput.WriteShort(pos, utfLength);
                    // Write the ASCII status of UTF to saved position before
                    //if (ASCII_AWARE)
                    //{
                    //    bufferObjectDataOutput.WriteBoolean(pos + 2, utfLength == str.Length);
                    //}
                }
            }

            protected internal abstract bool IsAvailable();

            protected internal abstract char[] GetCharArray(string str);
        }

        internal class StringBasedUtfWriter : IUtfWriter
        {
            /// <exception cref="System.IO.IOException"></exception>
            public virtual void WriteShortUTF(IDataOutput output, string str, int beginIndex, int endIndex, byte[] buffer)
            {
                bool isBufferObjectDataOutput = output is IBufferObjectDataOutput;
                IBufferObjectDataOutput bufferObjectDataOutput = isBufferObjectDataOutput ? (IBufferObjectDataOutput)output : null;
                int i;
                int c;
                int bufferPos = 0;
                int utfLength = 0;
                int utfLengthLimit;
                int pos = 0;
                if (isBufferObjectDataOutput)
                {
                    // At most, one character can hold 3 bytes
                    utfLengthLimit = str.Length * 3;
                    // We save current position of buffer data output.
                    // Then we write the length of UTF and ASCII state to here
                    pos = bufferObjectDataOutput.Position();
                    // Moving position explicitly is not good way
                    // since it may cause overflow exceptions for example "ByteArrayObjectDataOutput".
                    // So, write dummy data and let DataOutput handle it by expanding or etc ...
                    bufferObjectDataOutput.WriteShort(0);
                    //if (ASCII_AWARE)
                    //{
                    //    bufferObjectDataOutput.WriteBoolean(false);
                    //}
                }
                else
                {
                    utfLength = CalculateUtf8Length(str, beginIndex, endIndex);
                    if (utfLength > 65535)
                    {
                        throw new InvalidDataException("encoded string too long:" + utfLength + " bytes");
                    }
                    utfLengthLimit = utfLength;
                    output.WriteShort(utfLength);
                    //if (ASCII_AWARE)
                    //{
                    //    // We cannot determine that all characters are ASCII or not without iterating over it
                    //    // So, we mark it as not ASCII, so all characters will be checked.
                    //    output.WriteBoolean(false);
                    //}
                }
                if (buffer.Length >= utfLengthLimit)
                {
                    for (i = beginIndex; i < endIndex; i++)
                    {
                        c = str[i];
                        if (!(c <= unchecked((int)(0x007F)) && c >= unchecked((int)(0x0001))))
                        {
                            break;
                        }
                        buffer[bufferPos++] = unchecked((byte)c);
                    }
                    for (; i < endIndex; i++)
                    {
                        c = str[i];
                        if ((c >= unchecked((int)(0x0001))) && (c <= unchecked((int)(0x007F))))
                        {
                            // 0x0001 <= X <= 0x007F
                            buffer[bufferPos++] = unchecked((byte)c);
                        }
                        else
                        {
                            if (c > unchecked((int)(0x07FF)))
                            {
                                // 0x007F < X <= 0x7FFF
                                buffer[bufferPos++] = unchecked((byte)(unchecked((int)(0xE0)) | ((c >> 12) & unchecked((int)(0x0F)))));
                                buffer[bufferPos++] = unchecked((byte)(unchecked((int)(0x80)) | ((c >> 6) & unchecked((int)(0x3F)))));
                                buffer[bufferPos++] = unchecked((byte)(unchecked((int)(0x80)) | ((c) & unchecked((int)(0x3F)))));
                            }
                            else
                            {
                                // X == 0 or 0x007F < X < 0x7FFF
                                buffer[bufferPos++] = unchecked((byte)(unchecked((int)(0xC0)) | ((c >> 6) & unchecked((int)(0x1F)))));
                                buffer[bufferPos++] = unchecked((byte)(unchecked((int)(0x80)) | ((c) & unchecked((int)(0x3F)))));
                            }
                        }
                    }
                    output.Write(buffer, 0, bufferPos);
                    if (isBufferObjectDataOutput)
                    {
                        utfLength = bufferPos;
                    }
                }
                else
                {
                    for (i = beginIndex; i < endIndex; i++)
                    {
                        c = str[i];
                        if (!(c <= unchecked((int)(0x007F)) && c >= unchecked((int)(0x0001))))
                        {
                            break;
                        }
                        bufferPos = Buffering(buffer, bufferPos, unchecked((byte)c), output);
                    }
                    if (isBufferObjectDataOutput)
                    {
                        utfLength = i - beginIndex;
                    }
                    for (; i < endIndex; i++)
                    {
                        c = str[i];
                        if (c <= unchecked((int)(0x007F)) && c >= unchecked((int)(0x0001)))
                        {
                            // 0x0001 <= X <= 0x007F
                            bufferPos = Buffering(buffer, bufferPos, unchecked((byte)c), output);
                            if (isBufferObjectDataOutput)
                            {
                                utfLength++;
                            }
                        }
                        else
                        {
                            if (c > unchecked((int)(0x07FF)))
                            {
                                // 0x007F < X <= 0x7FFF
                                bufferPos = Buffering(buffer, bufferPos, unchecked((byte)(unchecked((int)(0xE0))
                                    | ((c >> 12) & unchecked((int)(0x0F))))), output);
                                bufferPos = Buffering(buffer, bufferPos, unchecked((byte)(unchecked((int)(0x80))
                                    | ((c >> 6) & unchecked((int)(0x3F))))), output);
                                bufferPos = Buffering(buffer, bufferPos, unchecked((byte)(unchecked((int)(0x80))
                                    | ((c) & unchecked((int)(0x3F))))), output);
                                if (isBufferObjectDataOutput)
                                {
                                    utfLength += 3;
                                }
                            }
                            else
                            {
                                // X == 0 or 0x007F < X < 0x7FFF
                                bufferPos = Buffering(buffer, bufferPos, unchecked((byte)(unchecked((int)(0xC0))
                                    | ((c >> 6) & unchecked((int)(0x1F))))), output);
                                bufferPos = Buffering(buffer, bufferPos, unchecked((byte)(unchecked((int)(0x80))
                                    | ((c) & unchecked((int)(0x3F))))), output);
                                if (isBufferObjectDataOutput)
                                {
                                    utfLength += 2;
                                }
                            }
                        }
                    }
                    int length = bufferPos % buffer.Length;
                    output.Write(buffer, 0, length == 0 ? buffer.Length : length);
                }
                if (isBufferObjectDataOutput)
                {
                    if (utfLength > 65535)
                    {
                        throw new InvalidDataException("encoded string too long:" + utfLength + " bytes");
                    }
                    // Write the length of UTF to saved position before
                    bufferObjectDataOutput.WriteShort(pos, utfLength);
                    // Write the ASCII status of UTF to saved position before
                    //if (ASCII_AWARE)
                    //{
                    //    bufferObjectDataOutput.WriteBoolean(pos + 2, utfLength == str.Length);
                    //}
                }
            }
        }

        // ********************************************************************* //
        /// <exception cref="System.IO.IOException"></exception>
        public string ReadUTF0(IDataInput input, byte[] buffer)
        {
            if (!QuickMath.IsPowerOfTwo(buffer.Length))
            {
                throw new ArgumentException("Size of the buffer has to be power of two, was " + buffer.Length);
            }
            bool isNull = input.ReadBoolean();
            if (isNull)
            {
                return null;
            }
            int length = input.ReadInt();
            int lengthCheck = input.ReadInt();
            if (length != lengthCheck)
            {
                throw new InvalidDataException("Length check failed, maybe broken bytestream or wrong stream position");
            }
            char[] data = new char[length];
            if (length > 0)
            {
                int chunkSize = length / STRING_CHUNK_SIZE + 1;
                for (int i = 0; i < chunkSize; i++)
                {
                    int beginIndex = Math.Max(0, i * STRING_CHUNK_SIZE - 1);
                    ReadShortUTF(input, data, beginIndex, buffer);
                }
            }
            return stringCreator.BuildString(data);
        }

        /// <exception cref="System.IO.IOException"></exception>
        private void ReadShortUTF(IDataInput input, char[] data, int beginIndex, byte[] buffer)
        {
            int utfLength = input.ReadShort() & unchecked((int)(0xFFFF));
            //bool allAscii = ASCII_AWARE ? input.ReadBoolean() : false;
            // buffer[0] is used to hold read data
            // so actual useful length of buffer is as "length - 1"
            int minUtfLenght = Math.Min(utfLength, buffer.Length - 1);
            int bufferLimit = minUtfLenght + 1;
            int readCount = 0;
            // We use buffer[0] to hold read data, so position starts from 1
            int bufferPos = 1;
            int c1 = 0;
            int c2 = 0;
            int c3 = 0;
            int cTemp = 0;
            int charArrCount = beginIndex;
            // The first readable data is at 1. index since 0. index is used to hold read data.
            input.ReadFully(buffer, 1, minUtfLenght);
            //if (allAscii)
            //{
            //    while (bufferPos != bufferLimit)
            //    {
            //        data[charArrCount++] = (char)(buffer[bufferPos++] & unchecked((int)(0xFF)));
            //    }
            //    for (readCount = bufferPos - 1; readCount < utfLength; readCount++)
            //    {
            //        bufferPos = Buffered(buffer, bufferPos, utfLength, readCount, input);
            //        data[charArrCount++] = (char)(buffer[0] & unchecked((int)(0xFF)));
            //    }
            //}
            //else
            //{
                c1 = buffer[bufferPos++] & unchecked((int)(0xFF));
                while (bufferPos != bufferLimit)
                {
                    if (c1 > 127)
                    {
                        break;
                    }
                    data[charArrCount++] = (char)c1;
                    c1 = buffer[bufferPos++] & unchecked((int)(0xFF));
                }
                bufferPos--;
                readCount = bufferPos - 1;
                while (readCount < utfLength)
                {
                    bufferPos = Buffered(buffer, bufferPos, utfLength, readCount++, input);
                    c1 = buffer[0] & unchecked((int)(0xFF));
                    cTemp = c1 >> 4;
                    if (cTemp >> 3 == 0)
                    {
                        // ((cTemp & 0xF8) == 0) or (cTemp <= 7 && cTemp >= 0)
                        data[charArrCount++] = (char)c1;
                    }
                    else
                    {
                        if (cTemp == 12 || cTemp == 13)
                        {
                            if (readCount + 1 > utfLength)
                            {
                                throw new InvalidDataException("malformed input: partial character at end");
                            }
                            bufferPos = Buffered(buffer, bufferPos, utfLength, readCount++, input);
                            c2 = buffer[0] & unchecked((int)(0xFF));
                            if ((c2 & unchecked((int)(0xC0))) != unchecked((int)(0x80)))
                            {
                                throw new InvalidDataException("malformed input around byte " + beginIndex + readCount+ 1);
                            }
                            data[charArrCount++] = (char)(((c1 & unchecked((int)(0x1F))) << 6) | (c2 & unchecked((int)(0x3F))));
                        }
                        else
                        {
                            if (cTemp == 14)
                            {
                                if (readCount + 2 > utfLength)
                                {
                                    throw new InvalidDataException("malformed input: partial character at end");
                                }
                                bufferPos = Buffered(buffer, bufferPos, utfLength, readCount++, input);
                                c2 = buffer[0] & unchecked((int)(0xFF));
                                bufferPos = Buffered(buffer, bufferPos, utfLength, readCount++, input);
                                c3 = buffer[0] & unchecked((int)(0xFF));
                                if (((c2 & unchecked((int)(0xC0))) != unchecked((int)(0x80))) || ((c3 & unchecked((int)(0xC0))) != unchecked((int)(0x80))))
                                {
                                    throw new InvalidDataException("malformed input around byte " + (beginIndex + readCount+ 1));
                                }
                                data[charArrCount++] = (char)(((c1 & unchecked((int)(0x0F))) << 12) | ((c2 & unchecked((int)(0x3F))) << 6) | ((c3 & unchecked((int)(0x3F)))));
                            }
                            else
                            {
                                throw new InvalidDataException("malformed input around byte " + (beginIndex + readCount));
                            }
                        }
                    }
                }
            //}
        }

        private static int CalculateUtf8Length(char[] value, int beginIndex, int endIndex)
        {
            int utfLength = 0;
            for (int i = beginIndex; i < endIndex; i++)
            {
                int c = value[i];
                if (c <= unchecked((int)(0x007F)) && c >= unchecked((int)(0x0001)))
                {
                    utfLength += 1;
                }
                else
                {
                    if (c > unchecked((int)(0x07FF)))
                    {
                        utfLength += 3;
                    }
                    else
                    {
                        utfLength += 2;
                    }
                }
            }
            return utfLength;
        }

        private static int CalculateUtf8Length(string str, int beginIndex, int endIndex)
        {
            int utfLength = 0;
            for (int i = beginIndex; i < endIndex; i++)
            {
                int c = str[i];
                if (c <= unchecked((int)(0x007F)) && c >= unchecked((int)(0x0001)))
                {
                    utfLength += 1;
                }
                else
                {
                    if (c > unchecked((int)(0x07FF)))
                    {
                        utfLength += 3;
                    }
                    else
                    {
                        utfLength += 2;
                    }
                }
            }
            return utfLength;
        }

        /// <exception cref="System.IO.IOException"></exception>
        private static int Buffering(byte[] buffer, int pos, byte value, IDataOutput output)
        {
            if (pos < buffer.Length)
            {
                buffer[pos] = value;
                return pos + 1;
            }
            output.Write(buffer, 0, buffer.Length);
            buffer[0] = value;
            return 1;
        }

        /// <exception cref="System.IO.IOException"></exception>
        private int Buffered(byte[] buffer, int pos, int utfLength, int readCount, IDataInput input)
        {
            if (pos < buffer.Length)
            {
                // 0. index of buffer is used to hold read data
                // so copy read data to there.
                buffer[0] = buffer[pos];
                return pos + 1;
            }
            input.ReadFully(buffer, 1, Math.Min(buffer.Length - 1, utfLength - readCount));
            // The first readable data is at 1. index since 0. index is used to
            // hold read data.
            // So the next one will be 2. index.
            buffer[0] = buffer[1];
            return 2;
        }

        private static UTFEncoderDecoder BuildUTFUtil()
        {
            IUtfWriter utfWriter = CreateUtfWriter();
            IStringCreator stringCreator = CreateStringCreator();
            return new UTFEncoderDecoder(stringCreator, utfWriter);
        }

        internal static IStringCreator CreateStringCreator()
        {
            return new DefaultStringCreator();
        }

        internal static IUtfWriter CreateUtfWriter()
        {
            return new StringBasedUtfWriter();
        }

        internal interface IStringCreator
        {
            string BuildString(char[] chars);
        }

        private class DefaultStringCreator : IStringCreator
        {
            public string BuildString(char[] chars)
            {
                return new string(chars);
            }
        }

    }
}
